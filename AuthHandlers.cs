using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TimeZonesVue
{
    public class TimeZoneAuthHandler : AuthorizationHandler<TimeZoneEditRequirement, UserDefinedTimeZone>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       TimeZoneEditRequirement requirement,
                                                       UserDefinedTimeZone resource)
        {
            var userId = context.User.Claims.First(c => c.Type == "UserId")?.Value;
            var role = Enum.Parse<Role>(context.User.Claims.First(c => c.Type == "Role")?.Value);
            if (userId == resource.OwnerId || role == Role.Admin)
            {
                context.Succeed(requirement);
            }
        }
    }

    public class TimeZoneEditRequirement : IAuthorizationRequirement { }

    public class UserAuthHandler : AuthorizationHandler<UserEditRequirement, string>
    {
        UserManager<IdentityUser> _userManager;

        public UserAuthHandler(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       UserEditRequirement requirement,
                                                       string userId)
        {
            var user = await _userManager.GetUserAsync(context.User);
            var claims = await _userManager.GetClaimsAsync(user);
            var role = claims.First(c => c.Type == "Role")?.Value;
            if (role == "Admin" || role == "Manager")
            {
                context.Succeed(requirement);
            }
        }
    }

    public class UserEditRequirement : IAuthorizationRequirement { }
}