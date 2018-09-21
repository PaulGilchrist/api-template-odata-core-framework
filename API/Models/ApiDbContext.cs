using Microsoft.EntityFrameworkCore;
using OdataCoreTemplate.Data;
using ODataCoreTemplate.Models;
using System;
using System.Threading.Tasks;

namespace OdataCoreTemplate.Models {
    public class ApiDbContext : DbContext {

        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) {
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Address>().HasMany(u => u.Users);
            modelBuilder.Entity<User>().HasMany(u => u.Addresses);
        }

    }
}
