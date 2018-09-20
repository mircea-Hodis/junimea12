using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthWebApi.Authorize;
using AuthWebApi.Data;
using AuthWebApi.IRepository;
using AuthWebApi.IUploadHelpers;
using AuthWebApi.IValidators;
using AuthWebApi.Models.Entities;
using AuthWebApi.Models.Posts;
using AuthWebApi.ViewModels.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthWebApi.Controllers
{
    [Authorize(Policy = "ApiUser")]
    [Route("api/[controller]")]
    public class PostController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IPostRepository _postRepository;
        private readonly IPostValidator _postValidator;
        private readonly IHostingEnvironment _environment;
        private readonly IPostFilesUploadHelper _postFilesUploadHelper;
        private readonly IFilesRepository _filesRepository;
        private readonly ClaimsPrincipal _caller;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IAuthorizationHelper _authorizationHelper;

        public PostController(
            UserManager<AppUser> userManager,
            IPostRepository postRepository,
            IPostValidator postValidator,
            IHostingEnvironment environment,
            IPostFilesUploadHelper postFilesUploadHelper,
            IFilesRepository filesRepository,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext applicationDbContext,
            IAuthorizationHelper authorizationHelper)
        {
            _userManager = userManager;
            _postRepository = postRepository;
            _postValidator = postValidator;
            _environment = environment;
            _postFilesUploadHelper = postFilesUploadHelper;
            _filesRepository = filesRepository;
            _caller = httpContextAccessor.HttpContext.User;
            _applicationDbContext = applicationDbContext;
            _authorizationHelper = authorizationHelper;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm]PostViewModel model)
        {
            var post = MapViewModelToPostInstance(model);
            post.UserId = _caller.Claims.Single(claim => claim.Type == "id").ToString().Remove(0, 4);
            //if (_postValidator.ValidatePost(model))
            //    return BadRequest();

            post = await _postRepository.CreateAsync(post);

            post = await _postFilesUploadHelper.UploadFiles(model.Files, post);

            await _filesRepository.AddPostImagesAsync(post.Files);

            return Ok();
        }

        public Post MapViewModelToPostInstance(PostViewModel postViewModel) =>
          new Post()
          {
              PostTtile = postViewModel.PostTitle,
              Description = postViewModel.Description,
              Likes = 0,
              Files = new List<PostFiles>(),
              CreatedDate = DateTime.Now
          };


    }
}
