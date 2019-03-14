using System;
using DataModelLayer.Models.Comments;
using DataModelLayer.Models.Entities;
using DataModelLayer.Models.Posts;
using DataModelLayer.Models.Tikets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PostReport = DataModelLayer.Models.Tikets.PostReport;

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
            AddIndexes(modelBuilder);
            
            SetDefaultValues(modelBuilder);
        }

        private void AddIndexes(ModelBuilder modelBuilder)
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

        private void SetDefaultValues(ModelBuilder modelBuilder)
        {
            SetDefaultValuesForPostReports(modelBuilder.Entity<PostReport>());
        }

        private void SetDefaultValuesForPostReports(EntityTypeBuilder<PostReport> entityBuilder)
        {
            entityBuilder.Property(model => model.AddresDateTime).HasDefaultValue(DateTime.Now);
            entityBuilder.Property(model => model.CreatedDate).HasDefaultValue(DateTime.Now);
        }

        private void SetDefaultValuesForCommentReports(EntityTypeBuilder<CommentReport> entityBuilder)
        {
            entityBuilder.Property(model => model.AddresDateTime).HasDefaultValue(DateTime.Now);
            entityBuilder.Property(model => model.CreatedDate).HasDefaultValue(DateTime.Now);
        }

        public DbSet<UserCommonData> UserCommonData { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostFiles> PostFiles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentFiles> CommentFiles { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<PostReport> PostReports { get; set; }
        public DbSet<CommentReport> CommentReports { get; set; }
        public DbSet<Ticket> ReportTickets { get; set; }
    }
}