using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthWebApi.Authorize;
using AuthWebApi.IRepository;
using AuthWebApi.IUploadHelpers;
using AuthWebApi.Models.Posts;
using AuthWebApi.ViewModels.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthWebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class PostController : Controller
    {
        private readonly IPostRepository _postRepository;
        //private readonly IPostValidator _postValidator;
        private readonly IAuthorizationHelper _authHelper;
        private readonly IPostFilesUploadHelper _postFilesUploadHelper;
        private readonly IFilesRepository _filesRepository;
        private readonly ClaimsPrincipal _caller;

        public PostController(
            IPostRepository postRepository,
            IPostFilesUploadHelper postFilesUploadHelper,
            IFilesRepository filesRepository,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationHelper authHelper)
        {
            _postRepository = postRepository;
            _postFilesUploadHelper = postFilesUploadHelper;
            _filesRepository = filesRepository;
            _caller = httpContextAccessor.HttpContext.User;
            _authHelper = authHelper;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm]PostViewModel model)
        {
            var post = MapViewModelToPostInstance(model);

            post.UserId = await _authHelper.GetCallerId(_caller);

            var x = await _authHelper.CheckIfUserIsBanned(post.UserId);
            if (await _authHelper.CheckIfUserIsBanned(post.UserId))
                return new BadRequestObjectResult(
                    new
                    {
                        Message ="User currently bannend",
                        StatusCodes.Status403Forbidden
                    });

            post = await _postRepository.CreateAsync(post);
            if (!model.Files.Any())
                return new OkObjectResult(new
                {
                    Message =" bine ba ai postat o glumitza",
                    post
                });

            post = await _postFilesUploadHelper.UploadFiles(model.Files, post);

            await _filesRepository.AddPostImagesAsync(post.Files);

            return new OkObjectResult(new
            {
                Message = " ai fost ba in stare sa faci un post klumea bine ba usere ba",
                post
            });
        }

        public Post MapViewModelToPostInstance(PostViewModel postViewModel) =>
          new Post
          {
              PostTtile = postViewModel.PostTitle,
              Description = postViewModel.Description,
              Likes = 0,
              Files = new List<PostFiles>(),
              CreatedDate = DateTime.Now
          };

        [HttpPost]
        [Route("LikePost")]
        public async Task<IActionResult> LikePost([FromBody]LikePostViewModel viewModel)
        {
            var postLike = new PostLike
            {
                UserId = _caller.Claims.Single(claim => claim.Type == "id").ToString().Remove(0, 4),
                PostId = viewModel.PostId,
                LikeTime = DateTime.Now,
                LikeCount = viewModel.Value
            };
            var result = await _postRepository.LikePost(postLike);

            return new OkObjectResult(new
            {
                Message = " bine ba ai postat o glumitza",
                result
            });
        }
    }
}
