using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthWebApi.Authorize;
using AuthWebApi.IUploadHelpers;
using DataAccessLayer.IRepository;
using DataModelLayer.Models.Posts;
using DataModelLayer.ViewModels.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthWebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [SuppressMessage("ReSharper", "TooManyDependencies")]
    // ReSharper disable once HollowTypeName
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

            if (await _authHelper.CheckIfUserIsBanned(post.UserId))
                return new BadRequestObjectResult(
                    new
                    {
                        Message ="User currently bannend",
                        StatusCodes.Status403Forbidden
                    });

            post = await _postRepository.CreateAsync(post);

            // ReSharper disable once ComplexConditionExpression
            if (model.Files == null || !model.Files.Any())
                return new OkObjectResult(new
                {
                    Message = " bine ba ai postat o glumitza",
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

        [HttpPost]
        [Route("UpdatePost")]
        public async Task<IActionResult> UpdatePost([FromForm]UpdatePostViewModel viewModel)
        {
            var callerId = await _authHelper.GetCallerId(_caller);

            if (await _authHelper.CheckIfUserIsBanned(callerId))
                return new BadRequestObjectResult(
                    new
                    {
                        Message = "User currently bannend",
                        StatusCodes.Status403Forbidden
                    });

            var result = await _postRepository.UpdatePostAsync(new UpdatePost(viewModel, callerId));

            var post = await _postRepository.GetPostById(viewModel.Id);

            post.Files = await _postFilesUploadHelper.ReplacePostFiles(post.Files, viewModel.Files, post.Id);

            await _filesRepository.UpdatePostImagesAsync(post.Files, post.Id);

            if (result > 0)
            {
                return new OkObjectResult(new
                {
                    Message = " ai fost ba in stare sa faci un post klumea bine ba usere ba",
                    post,
                    result
                });
            }

            return new BadRequestObjectResult(
                new
                {
                    result,
                    StatusCodes.Status403Forbidden
                });
        }

        [HttpPost]
        [Route("DeletePost")]
        public async Task<IActionResult> DeletePost([FromBody] DeletePostViewModel deletePostViewModel)
        {
            var callerId = await _authHelper.GetCallerId(_caller);
            
            if (await _authHelper.CheckIfUserIsBanned(callerId))
                return new BadRequestObjectResult(
                    new
                    {
                        Message = "User currently bannend",
                        StatusCodes.Status403Forbidden
                    });

            var result = await _postRepository.DeletePost(deletePostViewModel.PostId, callerId);
            _postFilesUploadHelper.DeleteFiles(result.Post);
            if(result.Successfull)
                return new OkObjectResult(new
                {
                    result
                });
            return new BadRequestObjectResult(
                new
                {
                    result,
                    StatusCodes.Status403Forbidden
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
                // ReSharper disable once TooManyChainedReferences
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
