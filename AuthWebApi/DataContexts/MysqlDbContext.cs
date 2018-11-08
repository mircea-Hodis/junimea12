using AuthWebApi.Models.Comments;
using AuthWebApi.Models.Entities;
using AuthWebApi.Models.Posts;
using Microsoft.EntityFrameworkCore;

namespace AuthWebApi.DataContexts
{
    public class MysqlDbContext : DbContext
    {
        public MysqlDbContext(DbContextOptions<MysqlDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserCommonData> UserCommonData { get; set; }

        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentFiles> CommentFiles { get; set; }
        public DbSet<PostReport> PostReports { get; set; }

    }
}