using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthWebApi.Models.Posts
{
    public class Post
    {
        [Key]
        public int Id { get; set; }
        public string PostTtile { get; set; }
        public string Description { get; set; } 
        public int Likes { get; set; }
        public List<PostFiles> Files { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserId { get; set; }
        [NotMapped]
        public string UserFacebookId { get; set; }
        [NotMapped]
        public string FirstName { get; set; }
        [NotMapped]
        public string LastName { get; set; }
    }

    public class PostFiles
    {
        public PostFiles() { }
        public PostFiles(int postId, string url)
        {
            PostId = postId;
            Url = url;
        }
        [Key]
        public int Id { get; set; }
        public int PostId { get; set; }
        public string Url { get;set; }
    }

    public class PostLike
    {
        [Key]
        public int LikeId { get; set; }
        public string UserId { get; set; }
        public int PostId { get; set; }
        public DateTime LikeTime { get; set; }
        public int LikeCount { get; set; }
        [NotMapped]
        public int PostLikesCount { get; set; }

    }
}
