using DataModelLayer.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthWebApi.DataContexts
{
    public class MsSqlUserDbContext : IdentityDbContext<AppUser>
    {
        public MsSqlUserDbContext(DbContextOptions<MsSqlUserDbContext> options) : base(options) { }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<AppAdministrationRoles> AppRoles { get; set; }
        public DbSet<UserAdministrationRoles> UserHighRoles { get; set; }
        public DbSet<Ban> Bans { get; set; }
    }
}
