using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthWebApi.DataContexts;
using DataAccessLayer.IMySqlRepos;
using DataAccessLayer.IRepository;
using DataModelLayer.Models.Entities;
using DataModelLayer.ViewModels.UserManagement;
using DataModelLayer.ViewModels.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<AppUser> _userManager;
        private readonly MsSqlUserDbContext _appDbContext;
        private readonly IUserCommonDataRepository _userCommonDataRepository;

        public UserManagementController(
            UserManager<AppUser> userManager,
            IUserManagementRepository userManagementRepository, 
            IHttpContextAccessor httpContextAccessor,
            MsSqlUserDbContext appDbContext, 
            IUserCommonDataRepository userCommonDataRepository)
        {
            _userManager = userManager;
            _userManagementRepository = userManagementRepository;
            _caller = httpContextAccessor.HttpContext.User;
            _appDbContext = appDbContext;
            _userCommonDataRepository = userCommonDataRepository;
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

        [Route("DeleteAccount")]
        [HttpPost]
        public async Task<IActionResult> DeleteAccount([FromBody]DeleteUser model)
        {
            var userToVerify = await _userManager.FindByNameAsync(model.Email);

            if (userToVerify == null)
                return BadRequest();

            var customerToRemove = _appDbContext.Customers
                .FirstOrDefault(customer => customer.IdentityId == userToVerify.Id);

            if (customerToRemove == null)
                return BadRequest();

            _appDbContext.Customers.Remove(customerToRemove);

            await _userManager.DeleteAsync(userToVerify);
            await _userCommonDataRepository.DeleteUserCommonData(userToVerify.Id);

            return new OkObjectResult("Account Deleted");
        }

        private string GetCallerId()
        {
            var callerId = string.Empty;
            if (_caller.Identity.IsAuthenticated)
                callerId = _caller.Claims.Single(claim =>
                        string.Equals(claim.Type, "id", StringComparison.OrdinalIgnoreCase))
                    .ToString().Remove(0, 4);

            return callerId;
        }
    }
}