using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace TimeZonesVue
{
    [Route("api/account")]
    public class AccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IAuthorizationService _authorizationService;

        private readonly List<PrivateUserDto> defaultUsers = new List<PrivateUserDto> {
            new PrivateUserDto(){ Id = "ba1b3980-d864-44ba-ab86-f15177895920", Name = "Tywin", Password = "Lion1", Role = "Admin" },
            new PrivateUserDto(){ Id = "6cad255e-0724-4bb9-80b7-c4d54685048d", Name = "Ed", Password = "DireWolf5", Role = "Admin" },
            new PrivateUserDto(){ Id = "ef2a76e9-3453-4c32-b72b-1d06f5de9922", Name = "Robb", Password = "DireWolf5", Role = "Manager" },
            new PrivateUserDto(){ Id = "d2c57c4a-a38e-4543-b837-031a18690c78", Name = "Joffrey", Password = "Jerk", Role = "User" },
            new PrivateUserDto(){ Id = "a22440f8-8c2b-46cf-bcea-a9ee3c5a5197", Name = "Arya", Password = "Needle", Role = "User" }
        };

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration,
            IAuthorizationService authorizationService
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _authorizationService = authorizationService;
        }

        [Authorize]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var requestingUserRole = Enum.Parse<Role>(User.Claims.First(c => c.Type == "Role").Value);
            if (requestingUserRole == Role.User)
            {
                return Unauthorized();
            }

            List<UserDto> ret = new List<UserDto>();
            foreach (var user in _userManager.Users)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                ret.Add(new UserDto()
                {
                    Id = user.Id,
                    Name = user.UserName,
                    Role = claims.First(c => c.Type == "Role")?.Value
                });
            }

            return Ok(ret);
        }

        [Authorize]
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser([FromBody]UserDto user)
        {
            // TODO: remove custom 'Role', use ASP.NET Identity role system
            if (defaultUsers.Any(u => u.Id == user.Id))
            {
                // Don't allow editing of default users
                return Unauthorized();
            }

            var appUser = _userManager.Users.SingleOrDefault(r => r.Id == user.Id);
            if (appUser != null)
            {
                var claims = await _userManager.GetClaimsAsync(appUser);
                var roleClaim = claims.First(c => c.Type == "Role");

                var requestingUserRole = Enum.Parse<Role>(User.Claims.First(c => c.Type == "Role").Value);
                var targetUserRole = Enum.Parse<Role>(roleClaim.Value);
                var canEdit = requestingUserRole == Role.Admin || (requestingUserRole == Role.Manager && targetUserRole != Role.Admin && user.Role != "Admin");
                if (!canEdit)
                {
                    return Unauthorized();
                }

                await _userManager.RemoveClaimAsync(appUser, roleClaim);
                await _userManager.AddClaimAsync(appUser, new Claim("Role", user.Role.ToString()));
                return Ok();
            }

            return BadRequest();
        }

        [Authorize]
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (defaultUsers.Any(u => u.Id == id))
            {
                // Don't allow editing of default users
                return Unauthorized();

            }
            var appUser = _userManager.Users.SingleOrDefault(r => r.Id == id);
            if (appUser != null)
            {
                var claims = await _userManager.GetClaimsAsync(appUser);
                var requestingUserRole = Enum.Parse<Role>(User.Claims.First(c => c.Type == "Role").Value);
                var targetUserRole = Enum.Parse<Role>(claims.First(c => c.Type == "Role").Value);
                var canEdit = requestingUserRole == Role.Admin || (requestingUserRole == Role.Manager && targetUserRole != Role.Admin);
                if (!canEdit)
                {
                    return Unauthorized();
                }

                await _userManager.DeleteAsync(appUser);
                return Ok();
            }

            return BadRequest();
        }

        [HttpPost("login")]
        public async Task<object> Login([FromBody] LoginDto model)
        {            
            //while (_userManager.Users.Count() > 0)
            //{
            //    await _userManager.DeleteAsync(_userManager.Users.First());
            //}
            //await VerifyDefaultUsers();

            var result = await _signInManager.PasswordSignInAsync(model.Name, model.Password, false, false);

            if (result.Succeeded)
            {
                var appUser = _userManager.Users.SingleOrDefault(r => r.Email == model.Name);
                var claims = await _userManager.GetClaimsAsync(appUser);
                Role role = (Role)Enum.Parse(typeof(Role), claims.First(c => c.Type == "Role").Value);
                return await GenerateJwtToken(model.Name, appUser, role);
            }

            throw new ApplicationException("INVALID_LOGIN_ATTEMPT");
        }

        [HttpPost("register")]
        public async Task<object> Register([FromBody] RegisterDto model)
        {
            var user = new IdentityUser
            {
                UserName = model.Name,
                Email = model.Name
            };

            var result = await _userManager.CreateAsync(user, model.Password);            

            if (result.Succeeded)
            {
                await _userManager.AddClaimAsync(user, new Claim("Role", Role.User.ToString()));
                await _signInManager.SignInAsync(user, false);
                return await GenerateJwtToken(model.Name, user, Role.User);
            }

            throw new ApplicationException("UNKNOWN_ERROR");
        }

        private async Task<object> GenerateJwtToken(string email, IdentityUser user, Role role)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserId", user.Id),
                new Claim("Role", role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSigningKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtExpirationDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtHost"],
                _configuration["JwtHost"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task VerifyDefaultUsers()
        {
            try
            {
                foreach (var defaultUser in defaultUsers)
                {
                    var user = new IdentityUser
                    {
                        UserName = defaultUser.Name,
                        Email = defaultUser.Name,
                        Id = defaultUser.Id
                    };
                    var result = await _userManager.CreateAsync(user, defaultUser.Password);

                    if (result.Succeeded)
                    {
                        // TODO: is ID generated in some way...? do we need to do that here...?
                        await _userManager.AddClaimAsync(user, new Claim("Role", defaultUser.Role));
                    }
                }
            }
            catch (Exception) { }
        }

    }

    public class LoginDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Password { get; set; }

    }

    public class RegisterDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "PASSWORD_MIN_LENGTH", MinimumLength = 6)]
        public string Password { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

    }

    public class PrivateUserDto : UserDto
    {
        public string Password { get; set; }
    }
}