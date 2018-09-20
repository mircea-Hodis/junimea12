using AuthWebApi.IRepository;
using AuthWebApi.Models.Posts;
using AuthWebApi.ViewModels.UserProfile;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AuthWebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/PostGetters")]
    public class PostGettersController : Controller
    {
        private readonly IPostRepository _postRepository;
        
        public PostGettersController(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        [HttpPost]
        [Route("GetPosts")]
        public async Task<IActionResult> GetPosts([FromBody]GetRecentRequest request)
        {
            var startDate = DateTime.Parse(request.StartDate);
            var result = await _postRepository.GetList(startDate);
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

        //[HttpPost]
        //[Route("GetAllUserPosts")]
        //public async Task<IActionResult> GetUsersPosts([FromBody]UserProfileRequest request)
        //{
        //    var result = await _postRepository.GetPostById(request.UserId);
        //    return new OkObjectResult(new
        //    {
        //        Message = "Whuhu",
        //        result
        //    });
        //}
    }
}