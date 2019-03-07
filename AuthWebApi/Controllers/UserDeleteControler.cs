using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthWebApi.DataContexts;
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
    public class UserDeleteController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly MsSqlUserDbContext _appDbContext;
        private readonly ClaimsPrincipal _caller;

        // ReSharper disable once TooManyDependencies
        public UserDeleteController(UserManager<AppUser> userManager,
                                 MsSqlUserDbContext appDbContext,
                                 IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _caller = httpContextAccessor.HttpContext.User;
            _appDbContext = appDbContext;
        }

        [Route("DeleteAccount")]
        [HttpPost]
        public async Task<IActionResult> DeleteAccount([FromBody]DeleteUser model)
        {
            var userToVerify = await _userManager.FindByNameAsync(model.Email);
            var customerToRemove = _appDbContext.Customers
                .FirstOrDefault(customer => customer.IdentityId == userToVerify.Id);

            if (ValidateDeleteion(customerToRemove))
                return BadRequest();

            if (customerToRemove != null) _appDbContext.Customers.Remove(customerToRemove);

            await _userManager.DeleteAsync(userToVerify);
            return new OkObjectResult("Account Deleted");
        }

        private bool ValidateDeleteion(Customer identity) => 
            identity == null || !GetCallerId().Equals(identity.IdentityId);


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
