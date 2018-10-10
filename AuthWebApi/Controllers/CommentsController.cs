using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthWebApi.IMySqlRepos;
using AuthWebApi.IRepository;
using AuthWebApi.IUploadHelpers;
using AuthWebApi.Models.Comments;
using AuthWebApi.ViewModels.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthWebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Comments")]
    [Authorize(Policy = "ApiUser")]
        
    public class CommentsController : Controller
    {
        private readonly ClaimsPrincipal _caller;
        private readonly ICommentRepository _commentRepository;
        private readonly ICommentFilesUploadHelpers _ulploadHelper;

        public CommentsController(
            IHttpContextAccessor httpContextAccessor,
            ICommentRepository commentsRepository,
            ICommentFilesUploadHelpers ulploadHelper)
        {
            _caller = httpContextAccessor.HttpContext.User;
            _commentRepository = commentsRepository;
            _ulploadHelper = ulploadHelper;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromForm] CommentViewModel model)
        {
            var comment = await MapViewModelToCommentInstance(model);
            comment = await _commentRepository.CreateAsync(comment);

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
                UserId = _caller
                    .Claims
                    .Single(
                        claim =>
                            string.Equals(claim.Type, "id", StringComparison.OrdinalIgnoreCase))
                    .ToString().Remove(0, 4),
                Likes = 0,
                Files = await _ulploadHelper.UploadFiles(model.Files, model.PostId)
        };
        }
    }
}