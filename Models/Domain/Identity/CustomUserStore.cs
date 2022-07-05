using Microsoft.AspNet.Identity.EntityFramework;

namespace Models.Domain.Identity
{
    public class CustomUserStore : UserStore<User, CustomRole, int, CustomUserLogin, CustomUserRole, CustomUserClaim>
    {
        public CustomUserStore(efilingDbContext context)
            : base(context)
        {
        }
    }
}
