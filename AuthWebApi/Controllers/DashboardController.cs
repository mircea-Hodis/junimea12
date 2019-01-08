using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthWebApi.DataContexts;
using DataModelLayer.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthWebApi.Controllers
{
  [Authorize]
  [Route("api/[controller]/[action]")]
  public class DashboardController : Controller
  {
    private readonly ClaimsPrincipal _caller;
    private readonly MsSqlUserDbContext _appDbContext;

    public DashboardController(UserManager<AppUser> userManager, MsSqlUserDbContext appDbContext, IHttpContextAccessor httpContextAccessor)
    {
      _caller = httpContextAccessor.HttpContext.User;
      _appDbContext = appDbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Home()
    {
      // retrieve the user info
      var userId = _caller.Claims.Single(c => c.Type == "id");
      var customer = await _appDbContext.Customers.Include(c => c.Identity).SingleAsync(c => c.Identity.Id == userId.Value);

      return new OkObjectResult(new
      {
        Message = "This is secure API and user data!",
        customer.Identity.FirstName,
        customer.Identity.LastName,
        customer.Identity.PictureUrl,
        customer.Identity.FacebookId,
        customer.Location,
        customer.Locale,
        customer.Gender
      });
    }
  }
}
