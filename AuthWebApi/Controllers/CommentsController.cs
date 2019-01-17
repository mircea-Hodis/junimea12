using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthWebApi.Authorize;
using AuthWebApi.IMySqlRepos;
using AuthWebApi.IUploadHelpers;
using DataAccessLayer.IMySqlRepos;
using DataModelLayer.Models.Comments;
using DataModelLayer.ViewModels.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LikeCommentViewModel = DataModelLayer.Models.Comments.LikeCommentViewModel;

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
        private readonly ICommentFilesUploader _uploadHelper;

        // ReSharper disable once TooManyDependencies
        public CommentsController(
            IHttpContextAccessor httpContextAccessor,
            ICommentRepository commentsRepository,
            ICommentFilesUploader uploadHelper,
            IAuthorizationHelper authHelper)
        {
            _caller = httpContextAccessor.HttpContext.User;
            _commentRepository = commentsRepository;
            _uploadHelper = uploadHelper;
            _authHelper = authHelper;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] CommentViewModel model)
        {
            var comment = await MapViewModelToCommentInstance(model);
            var callerId = await _authHelper.GetCallerId(_caller);

            if (await _authHelper.CheckIfUserIsBanned(callerId))
                return new BadRequestObjectResult(
                    new
                    {
                        Message = "User currently bannend",
                        StatusCodes.Status403Forbidden
                    });

            comment = await _commentRepository.CreateAsync(comment);
            if (model.Files != null)
            {
                comment.Files = await _uploadHelper.UploadFiles(model.Files, comment.Id);
                comment = await _commentRepository.AddCommentFiles(comment);
            }

            return new OkObjectResult(new
            {
                Message = "Whuhu",
                comment
            });
        }

        [HttpPost]
        [Route("DeleteComment")]
        public async Task<IActionResult> DeleteComment([FromBody] DeleteComment model)
        {
            var callerId = await _authHelper.GetCallerId(_caller);

            if (await _authHelper.CheckIfUserIsBanned(callerId))
                return new BadRequestObjectResult(
                    new
                    {
                        Message = "User currently bannend",
                        StatusCodes.Status403Forbidden
                    });
            var result = await _commentRepository.DeleteComment(model.CommentId, callerId);

            if (result.Successfull)
            {
                if (result.RemainingFiles.Any())
                    _uploadHelper.DeleteCommentFiles(result.RemainingFiles);
                return new OkObjectResult(new
                {
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
        [Route("LikeComment")]
        public async Task<IActionResult> LikeComment([FromBody]LikeCommentViewModel viewModel)
        {
            var commentLike = new CommentLike   
            {
                // ReSharper disable once TooManyChainedReferences
                UserId = _caller.Claims.Single(claim => claim.Type == "id").ToString().Remove(0, 4),
                CommentId = viewModel.PostId,
                LikeTime = DateTime.Now,
                LikeCount = viewModel.Value
            };
            var result = await _commentRepository.LikeComment(commentLike);

            return new OkObjectResult(new
            {
                result
            });
        }

        [HttpPost]
        [Route("UpdateComment")]
        public async Task<IActionResult> UpdateComment([FromForm] UpdateCommentViewModel model)
        {
            var callerId = await _authHelper.GetCallerId(_caller);

            if (await _authHelper.CheckIfUserIsBanned(callerId))
                return new BadRequestObjectResult(
                    new
                    {
                        Message = "User currently bannend",
                        StatusCodes.Status403Forbidden
                    });
            var updateCommentObj = new UpdateComment(model.Id, model.Comment, callerId);
            var updatedComment = await _commentRepository.UpdateComment(updateCommentObj);
            if (updatedComment.Files != null && updatedComment.Files.Any())
            {
                _uploadHelper.DeleteCommentFiles(updatedComment.Files);
                updatedComment.Files = await _uploadHelper.UploadFiles(model.Files, model.Id);
            }
            await _commentRepository.UpdateCommentImages(updatedComment.Files, updatedComment.Id);
            return new OkObjectResult(new
            {
                updatedComment
            });
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