using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccessLayer.IMySqlRepos;
using DataAccessLayer.IRepository;
using Microsoft.AspNetCore.Http;
using DataModelLayer.Models.Posts;
using DataModelLayer.ViewModels.UserProfile;

namespace AuthWebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/PostGetters")]
    // ReSharper disable once HollowTypeName
    public class PostGettersController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly ClaimsPrincipal _caller;
        private readonly ICommentRepository _commentRepository;
        public PostGettersController(
            IPostRepository postRepository,
            IHttpContextAccessor httpContextAccessor,
            ICommentRepository commentRepository)
        {
            _postRepository = postRepository;
            _caller = httpContextAccessor.HttpContext.User;
            _commentRepository = commentRepository;
        }

        [HttpPost]
        [Route("GetPosts")]
        public async Task<IActionResult> GetPosts([FromBody]GetRecentRequest request)
        {
            var callerId = GetCallerId();

            var startDate = DateTime.Parse(request.StartDate);
            var result = await _postRepository.GetList(startDate, callerId);
            return new OkObjectResult(new
            {
                Message = "Whuhu",
                result
            });
        }

        [HttpPost]
        [Route("GetPostById")]
        public async Task<IActionResult> GetPostById([FromBody]GetPostRequest request)
        {
            var callerId = GetCallerId();

            var result = await _postRepository.GetPostById(request.PostId, callerId);
            return new OkObjectResult(new
            {
                Message = "Whuhu",
                result
            });
        }

        [HttpPost]
        [Route("GetPostBatchInitial")]
        public async Task<IActionResult> GetPostBatchInitial()
        {
            var callerId = GetCallerId();

            var result = await _postRepository.GetListInitial(callerId);
            return new OkObjectResult(new
            {
                Message = "Whuhu",
                result
            });
        }

        [HttpPost]
        [Route("GetAdditionalComments")]
        public async Task<IActionResult> GetAdditionalComments([FromBody]GetAdditionalComments request)
        {
            var result = await _postRepository.GetRemainingComments(request.PostId, request.LastCommentDate);
            return new OkObjectResult(new
            {
                Message = "Whuhu",
                result
            });
        }

        [HttpPost]
        [Route("GetAllUserPosts")]
        public async Task<IActionResult> GetUsersPosts([FromBody]UserProfileRequest request)
        {
            var result = await _postRepository.GetUserPosts(request.UserId, request.StartDate);
            return new OkObjectResult(new
            {
                Message = "Whuhu",
                result
            });
        }

        [HttpPost]
        [Route("GetNext")]
        public async Task<IActionResult> GetNextPost([FromBody]NextPreviousPost request)
        {
            var callerId = GetCallerId();
            var result = await _postRepository.GetNextPost(request.CurrentId, callerId);
            return new OkObjectResult(new
            {
                Message = "Whuhu",
                result
            });
        }

        [HttpPost]
        [Route("GetPrevious")]
        public async Task<IActionResult> GetPreviousPost([FromBody]NextPreviousPost request)
        {
            var callerId = GetCallerId();
            var result = await _postRepository.GetPrevious(request.CurrentId, callerId);
            return new OkObjectResult(new
            {
                Message = "Whuhu",
                result
            });
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