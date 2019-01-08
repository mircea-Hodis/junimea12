using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthWebApi.IRepository;
using DataAccessLayer.IRepository;
using DataModelLayer.ViewModels.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthWebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/UserManagement")]
    [Authorize(Policy = "ApiAdmin")]
    // ReSharper disable once HollowTypeName
    public class UserManagementController : Controller
    {
        private readonly IUserManagementRepository _userManagementRepository;
        private readonly ClaimsPrincipal _caller;

        public UserManagementController(
            IUserManagementRepository userManagementRepository, 
            IHttpContextAccessor httpContextAccessor)
        {
            _userManagementRepository = userManagementRepository;
            _caller = httpContextAccessor.HttpContext.User;
        }

        [Route("BanUser")]
        [HttpPost]
        public async Task<IActionResult> BanUser([FromBody]UserBanViewModel banViewModel)
        {
            var callerId = GetCallerId();
            if (string.IsNullOrEmpty(callerId))
                return new BadRequestObjectResult(new
                {
                    Message = "You cannot ban this user"
                });
            var result = await _userManagementRepository.BanUserAsync(banViewModel, callerId);
            return new OkObjectResult(new
            {
                Message = "User banned succesfully",
                result
            });

        }

        [Route("UnbanUser")]
        [HttpPost]
        public async Task<IActionResult> UnBanUser([FromBody]UnbanUserViewModel model)
        {
            var callerId = GetCallerId();
            if (string.IsNullOrEmpty(callerId))
                return new BadRequestObjectResult(new
                {
                    Message = "Login to unban"
                });

            await _userManagementRepository.UnBanUser(model.UserId);

            return Ok();
        }

        [Route("GetBans")]
        public async Task<IActionResult> GetBans()
        {
            var result = await _userManagementRepository.GetBans();
            var message = string.Empty;
            if (result.Any())
                message = "No bans to display";

            return new OkObjectResult(new
            {
                Message = message,
                result
            });
        }

        [Route("GetUserBan")]
        public async Task<IActionResult> GetUserBan([FromBody]GetUserBanRequest model)
        {
            var result = await _userManagementRepository.GetThisUserBan(model.UserId);
            if (result == null)
                return new OkObjectResult(new
                {
                    Message = "User is not banned"
                });
            return new OkObjectResult(new
            {
                result
            });
        }

        private string GetCallerId()
        {
            if (_caller.Identity.IsAuthenticated)
                return _caller.Claims.Single(claim =>
                        string.Equals(claim.Type, "id", StringComparison.OrdinalIgnoreCase))
                    .ToString().Remove(0, 4);
            return string.Empty;
        }
    }
}