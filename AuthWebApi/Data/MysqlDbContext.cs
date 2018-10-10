using AuthWebApi.Models.Comments;
using Microsoft.EntityFrameworkCore;

namespace AuthWebApi.Data
{
    public class MysqlDbContext : DbContext
    {
        public MysqlDbContext(DbContextOptions<MysqlDbContext> options)
            : base(options)
        {
        }

        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentFiles> CommentFiles { get; set; }
    }
}
