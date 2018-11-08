using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthWebApi.DataContexts;
using AuthWebApi.IRepository;

namespace AuthWebApi.Authorize
{
    public class AuthorizationHelper : IAuthorizationHelper
    {
        private readonly ClaimsPrincipal _caller;
        private readonly MsSqlUserDbContext _appDbContext;
        private readonly IUserManagementRepository _userManager;

        public AuthorizationHelper(IUserManagementRepository  userManager)
        {
            _userManager = userManager;
        }

        public async Task<string> GetCallerId(ClaimsPrincipal caller)
        {
            return caller
                .Claims
                    .Single(claim => 
                        string.Equals(
                            claim.Type, 
                            "id", 
                            StringComparison.OrdinalIgnoreCase))
                    .ToString().Remove(0, 4);
        }

        public async Task<bool> CheckIfUserIsBanned(string userId)
        {
            return await _userManager.CheckIfUserIsBanned(userId);
        }
    }

}
