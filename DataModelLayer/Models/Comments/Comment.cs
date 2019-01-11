using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace DataModelLayer.Models.Comments
{
    [Table("Comments")]
    public class Comment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public int PostId { get; set; }
        public string Message { get; set; }
        public int Likes { get; set; }
        [NotMapped]
        public long FacebookId { get; set;}
        [Required]
        [MaxLength(50)]
        public string UserId { get; set; }
        public DateTime CreateDate { get; set; }
        [NotMapped]
        public string FirstName { get; set; }
        [NotMapped]
        public string LastName { get; set; }
        [NotMapped]
        public string UserProfilePicUrl { get; set; }
        [NotMapped]
        public List<CommentFiles> Files { get; set; }
    }

    public class UpdateComment
    {
        public UpdateComment(long id, string comment, string userId)
        {
            Id = id;
            Comment = comment;
            UserId = userId;
        }
        public long Id { get; set; }
        public string Comment { get; set; }
        public string UserId { get; set; }
        public List<CommentFiles> Files { get; set; }
    }

    public class UpdateCommentViewModel
    {
        public long Id { get; set; }
        public string Comment { get; set; }
        public List<IFormFile> Files { get; set; }
    }

    public class DeleteComment
    {
        public int CommentId { get; set; }
    }

    public class DeleteCommentResponse
    {
        public string Message { get; set; }
        public bool Successfull { get; set; }
        public List<CommentFiles> RemainingFiles { get; set; }
    }

    [Table("Comment_Files")]
    public class CommentFiles
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long CommentId { get; set; }
        public string Url { get; set; }
        public CommentFiles(long commentId, string url)
        {
            CommentId = commentId;
            Url = url;
        }

        public CommentFiles(long id, long commentId, string url)
        {
            Id = id;
            CommentId = commentId;
            Url = url;
        }
    }

    public class CommentLikes
    {
        [Key]
        public string Id { get; set; }
        public long CommentId { get; set; }
        public string UserId { get; set; }
    }
}