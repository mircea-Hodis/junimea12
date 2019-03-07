using System.Threading.Tasks;
using AuthWebApi.DataContexts;
using AuthWebApi.Helpers;
using AutoMapper;
using DataAccessLayer.IMySqlRepos;
using DataModelLayer.Models.Entities;
using DataModelLayer.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthWebApi.Controllers
{
    [Route("api/[controller]")]
    // ReSharper disable once HollowTypeName
    public class AccountsController : Controller
    {
        private readonly MsSqlUserDbContext _appDbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUserCommonDataRepository _userCommonDataRepository;

        // ReSharper disable once TooManyDependencies
        public AccountsController(UserManager<AppUser> userManager,
                                 IMapper mapper,
                                 MsSqlUserDbContext appDbContext,
                                 IUserCommonDataRepository userCommonDataRepository)
        {
            _userManager = userManager;
            _mapper = mapper;
            _appDbContext = appDbContext;
            _userCommonDataRepository = userCommonDataRepository;
        }

        // POST api/accounts
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]RegistrationViewModel model)
        {
            await Task.Delay(250);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdentity = _mapper.Map<AppUser>(model);

            var result = await _userManager.CreateAsync(userIdentity, model.Password);
            if (!result.Succeeded) return new BadRequestObjectResult(Errors.AddErrorsToModelState(result, ModelState));

            await _appDbContext.Customers.AddAsync(new Customer { IdentityId = userIdentity.Id, Location = model.Location });
            await _appDbContext.SaveChangesAsync();

            var userCommonData = CreateUserCommonDataObject(userIdentity);

            await _userCommonDataRepository.AddUserCommonData(userCommonData);

            return new OkObjectResult("Account created");
        }

        //[Route("DeleteAccount")]
        //[HttpPost]
        //public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountViewModel model)
        //{
        //    _userManager.GetUserAsync();
        //    _userManager.DeleteAsync();
        //}


        private UserCommonData CreateUserCommonDataObject(AppUser userIdentity)
        {
            return new UserCommonData
            {
                UserId = userIdentity.Id,
                FacebookId = userIdentity.FacebookId ?? 0,
                FirstName = userIdentity.FirstName,
                LastName = userIdentity.LastName,
                UserName = userIdentity.UserName,
                UserEmail = userIdentity.Email,
                UserLevel = 1
            };

        }
    }
}
