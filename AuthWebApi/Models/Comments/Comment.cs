using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthWebApi.Models.Comments
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
        public string UserId { get; set; }
        public string CreateDate { get; set; }
        [NotMapped]
        public string UserFirstName { get; set; }
        [NotMapped]
        public string UserLastName { get; set; }
        [NotMapped]
        public string UserProfilePicUrl { get; set; }
        [NotMapped]
        public List<CommentFiles> Files { get; set; }
    }

    [Table("Comment_Files")]
    public class CommentFiles
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long CommentId { get; set; }
        public string Url { get; set; }
        public CommentFiles(){}
        public CommentFiles(long commentId, string url)
        {
            CommentId = commentId;
            Url = url;
        }
    }
}
    