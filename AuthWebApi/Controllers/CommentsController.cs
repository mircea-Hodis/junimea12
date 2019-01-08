using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthWebApi.Authorize;
using AuthWebApi.IMySqlRepos;
using AuthWebApi.IUploadHelpers;
using DataModelLayer.Models.Comments;
using DataModelLayer.ViewModels.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthWebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Comments")]
    [Authorize]

    // ReSharper disable once HollowTypeName
    public class CommentsController : Controller
    {
        private readonly ClaimsPrincipal _caller;
        private readonly ICommentRepository _commentRepository;
        private readonly IAuthorizationHelper _authHelper;
        private readonly ICommentFilesUploader _ulploadHelper;

        public CommentsController(
            IHttpContextAccessor httpContextAccessor,
            ICommentRepository commentsRepository,
            ICommentFilesUploader ulploadHelper,
            IAuthorizationHelper authHelper)
        {
            _caller = httpContextAccessor.HttpContext.User;
            _commentRepository = commentsRepository;
            _ulploadHelper = ulploadHelper;
            _authHelper = authHelper;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] CommentViewModel model)
        {
            var comment = await MapViewModelToCommentInstance(model);

            if (await _authHelper.CheckIfUserIsBanned(comment.UserId))
                return new BadRequestObjectResult(
                    new
                    {
                        Message = "User currently bannend",
                        StatusCodes.Status403Forbidden
                    });

            comment = await _commentRepository.CreateAsync(comment);
            if (model.Files != null)
            {
                comment.Files = await _ulploadHelper.UploadFiles(model.Files, comment.Id);
                comment = await _commentRepository.AddCommentFiles(comment);
            }

            return new OkObjectResult(new
            {
                Message = "Whuhu",
                comment
            });
        }

        [HttpPost]
        [Route("LikeComment")]
        public async Task<IActionResult> LikeComment([FromBody] LikeCommentViewModel model)
        {
            return Ok();
        }

        private async Task<Comment> MapViewModelToCommentInstance(CommentViewModel model)
        {
            return new Comment
            {
                PostId = model.PostId,
                Message = model.Comment,
                UserId = await _authHelper.GetCallerId(_caller),
                Likes = 0,
                CreateDate = DateTime.Now
            };
        }
    }
}