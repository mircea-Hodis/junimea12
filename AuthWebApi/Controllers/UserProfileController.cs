using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthWebApi.DataContexts;
using DataAccessLayer.IMySqlRepos;
using DataModelLayer.Models.Entities;
using DataModelLayer.ViewModels.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthWebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    // ReSharper disable once HollowTypeName
    public class UserProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly MsSqlUserDbContext _appDbContext;
        private readonly ClaimsPrincipal _caller;
        private readonly IUserCommonDataRepository _userCommonDataRepository;

        // ReSharper disable once TooManyDependencies
        public UserProfileController(
            UserManager<AppUser> userManager,
            MsSqlUserDbContext appDbContext,
            IHttpContextAccessor httpContextAccessor,
            IUserCommonDataRepository userCommonDataRepository)
        {
            _userManager = userManager;
            _caller = httpContextAccessor.HttpContext.User;
            _appDbContext = appDbContext;
            _userCommonDataRepository = userCommonDataRepository;
        }

        [Route("DeleteAccount")]
        [HttpPost]
        public async Task<IActionResult> DeleteAccount([FromBody]DeleteUser model)
        {
            var userToVerify = await _userManager.FindByEmailAsync(model.Email);

            if (userToVerify == null)
                return BadRequest();

            var callerId = GetCallerId();
            if (callerId != userToVerify.Id)
                return BadRequest();

            var customerToRemove = _appDbContext.Customers.FirstOrDefault(customer => customer.IdentityId == userToVerify.Id);

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
