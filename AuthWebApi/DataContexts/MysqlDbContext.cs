using DataModelLayer.Models.Comments;
using DataModelLayer.Models.Entities;
using DataModelLayer.Models.Posts;
using DataModelLayer.Models.Tikets;
using Microsoft.EntityFrameworkCore;

namespace AuthWebApi.DataContexts
{
    public class MysqlDbContext : DbContext
    {
        public MysqlDbContext(DbContextOptions<MysqlDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>()
                .HasIndex(comment => comment.UserId);
            modelBuilder.Entity<Post>()
                .HasIndex(post => post.UserId);
            modelBuilder.Entity<PostFiles>()
                .HasIndex(postFile => postFile.PostId);
            modelBuilder.Entity<CommentFiles>()
                .HasIndex(commentFile => commentFile.CommentId);
            modelBuilder.Entity<CommentLike>()
                .HasIndex(commentLike => commentLike.CommentId);
        }

        public DbSet<UserCommonData> UserCommonData { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostFiles> PostFiles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentFiles> CommentFiles { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<ReportEntity> ReportEntity { get; set; }
        public DbSet<Ticket> ReportTickets { get; set; }

        //public DbSet<UserStatus> UsersStatuses { get; set; }
        //public DbSet<AdminProposal> AdminProposals { get; set; }
        //public DbSet<Ticket> Tikets { get; set; }
    }
}