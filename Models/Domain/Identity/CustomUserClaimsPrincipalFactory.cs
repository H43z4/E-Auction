using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Models.Domain.Identity
{
    public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<User>
    {
        public CustomUserClaimsPrincipalFactory(
            UserManager<User> userManager,
            IOptions<IdentityOptions> optionsAccessor)
                : base(userManager, optionsAccessor)
        {
        }
        
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("FullName", user.FullName ?? "[Click to edit profile]"));
            return identity;
        }
    }

    //public class MyUserClaimsPrincipalFactory<User, CustomRole> : UserClaimsPrincipalFactory<User, CustomRole>
    //    where User : class
    //    where CustomRole : class
    //{
    //    public MyUserClaimsPrincipalFactory(
    //        UserManager<User> userManager,
    //        RoleManager<CustomRole> roleManager,
    //        IOptions<IdentityOptions> optionsAccessor)
    //            : base(userManager, roleManager, optionsAccessor)
    //    {
    //    }

    //    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
    //    {
    //        var identity = await base.GenerateClaimsAsync(user);
    //        identity.AddClaim(new Claim("ContactName", user.Name ?? "[Click to edit profile]"));
    //        return identity;
    //    }
    //}
}
