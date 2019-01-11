using System;

namespace DataModelLayer.Models.Posts
{
    public class GetPostRequest
    {
        public int PostId { get; set; }
    }

    public class GetAdditionalComments
    {
        public int PostId { get; set; }
        public DateTime LastCommentDate { get; set; }
    }
}
