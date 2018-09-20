using AuthWebApi.Models.Entities;
using AuthWebApi.Models.Posts;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Customer = AuthWebApi.Models.Entities.Customer;

namespace AuthWebApi.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostFiles> PostFiles { get; set; }
    }
}
