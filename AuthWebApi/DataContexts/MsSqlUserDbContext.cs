using AuthWebApi.Models.Entities;
using AuthWebApi.Models.Posts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Customer = AuthWebApi.Models.Entities.Customer;

namespace AuthWebApi.DataContexts
{
    public class MsSqlUserDbContext : IdentityDbContext<AppUser>
    {
        public MsSqlUserDbContext(DbContextOptions<MsSqlUserDbContext> options) : base(options) { }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostFiles> PostFiles { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<AppAdministrationRoles> AppRoles { get; set; }
        public DbSet<UserAdministrationRoles> UserHighRoles { get; set; }
        public DbSet<Ban> Bans { get; set; }
    }
}
