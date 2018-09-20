using System.Security.Claims;

namespace AuthWebApi.Authorize
{
    public interface IAuthorizationHelper
    {
        Claim GetCallerId();
    }
}
