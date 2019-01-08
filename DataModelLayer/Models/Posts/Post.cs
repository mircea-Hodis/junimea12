using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataModelLayer.Models.Comments;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace DataModelLayer.Models.Posts
{
    [Table("Posts")]
    public class Post
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string PostTtile { get; set; }
        public string Description { get; set; } 
        public int Likes { get; set; }
        public List<PostFiles> Files { get; set; }
        public DateTime CreatedDate { get; set; }
        [Required]
        [MaxLength(50)]
        public string UserId { get; set; }
        [NotMapped]
        public long UserFacebookId { get; set; }
        [NotMapped]
        public string FirstName { get; set; }
        [NotMapped]
        public string LastName { get; set; }
        [NotMapped]
        public int LikeCount { get; set; }
        [NotMapped]
        public List<Comment> Comments { get; set; }
    }

    public class UpdatePostViewModel
    {
        public int Id { get; set; }
        public string PostTtile { get; set; }
        public string Description { get; set; }
        public List<IFormFile> Files { get; set; }
    }

    public class UpdatePost
    {
        public UpdatePost(UpdatePostViewModel viewModel, string userId)
        {
            Id = viewModel.Id;
            PostTtile = viewModel.PostTtile;
            Description = viewModel.Description;
            UserId = userId;
        }
        public int Id { get; set; }
        public string PostTtile { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public List<PostFiles> Files { get; set; }
    }

    public class DeletePostResponse
    {
        public string Message { get; set; }
        public bool Successfull { get; set; }
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
