using AuthWebApi.IRepository;
using AuthWebApi.Models.Posts;
using AuthWebApi.ViewModels.UserProfile;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthWebApi.IMySqlRepos;
using Microsoft.AspNetCore.Http;

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
            var callerId = string.Empty;
            if(_caller.Identity.IsAuthenticated)
                callerId = _caller.Claims.Single(claim =>
                    string.Equals(claim.Type, "id", StringComparison.OrdinalIgnoreCase))
                    .ToString().Remove(0, 4);

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
            var result = await _postRepository.GetPostById(request.PostId);
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
            var result = await _postRepository.GetUserPosts(request.UserId);
            return new OkObjectResult(new
            {
                Message = "Whuhu",
                result
            });
        }
    }
}