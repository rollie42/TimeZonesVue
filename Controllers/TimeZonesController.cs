using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TimeZonesVue.Controllers
{
    [Route("api/timezones")]
    public class TimeZoneController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuthorizationService _authorizationService;

        public TimeZoneController(
            UserManager<IdentityUser> userManager,
            IAuthorizationService authorizationService
            )
        {
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        [HttpGet("")]
        public IEnumerable<UserDefinedTimeZone> GetTimezones(ApplicationDbContext dbContext)
        {
            return dbContext.TimeZones;
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimezone(ApplicationDbContext dbContext, string id)
        {
            var record = dbContext.TimeZones.Find(id);
            if (record != null)
            {
                var authResult = await _authorizationService.AuthorizeAsync(User, record, "EditPolicy");
                if (!authResult.Succeeded)
                {
                    return Unauthorized();
                }
                dbContext.TimeZones.Remove(record);
                dbContext.SaveChanges();
                return Ok();
            }

            return BadRequest();
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTimezone(ApplicationDbContext dbContext, string id, [FromBody]UserDefinedTimeZone timeZone)
        {
            var record = dbContext.TimeZones.Find(id);
            if (record != null)
            {
                var authResult = await _authorizationService.AuthorizeAsync(User, record, "EditPolicy");
                if (!authResult.Succeeded)
                {
                    return Unauthorized();
                }

                record.Name = timeZone.Name;
                record.City = timeZone.City;
                record.GmtOffset = timeZone.GmtOffset;
                dbContext.SaveChanges();
                return Ok(timeZone);
            }

            return BadRequest();
        }

        [Authorize]
        [HttpPost("")]
        public IActionResult PostTimezone(ApplicationDbContext dbContext, [FromBody]UserDefinedTimeZone timeZone)
        {
            timeZone.Id = Guid.NewGuid().ToString();
            timeZone.OwnerId = User.FindFirst("UserId").Value;
            timeZone.Owner = User.FindFirst(JwtRegisteredClaimNames.Sub).Value;
            dbContext.TimeZones.Add(timeZone);
            dbContext.SaveChanges();
            return Ok(timeZone);

        }
    }
}
