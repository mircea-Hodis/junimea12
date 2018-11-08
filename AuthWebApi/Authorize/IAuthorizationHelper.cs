using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthWebApi.Authorize
{
    public interface IAuthorizationHelper
    {
        Task<string> GetCallerId(ClaimsPrincipal caller);
        Task<bool> CheckIfUserIsBanned(string UserId);
    }
}
