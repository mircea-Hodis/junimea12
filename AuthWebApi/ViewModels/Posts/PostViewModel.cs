using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace AuthWebApi.ViewModels.Posts
{
    public class PostViewModel
    {
        public string PostTitle { get; set; }
        public string Description { get; set; }
        public List<IFormFile> Files { get; set; }
    }

    public class LikePostViewModel
    {
        public int PostId { get; set; }
        public int Value { get; set; }
    }

    public class CommentViewModel
    {
        public int PostId { get; set; }
        public string Comment { get; set; }
        public List<IFormFile> Files { get; set; }
    }

    public class LikeCommentViewModel
    {
        public int Comment { get; set; }
        public int Value { get; set; }
    }
}
