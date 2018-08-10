using Microsoft.EntityFrameworkCore;
using OdataCoreTemplate.Data;
using ODataCoreTemplate.Models;
using System.Threading.Tasks;

namespace OdataCoreTemplate.Models {
    public class ApiContext : DbContext {
        public ApiContext(DbContextOptions<ApiContext> options)
             : base(options) {
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Address>().HasMany(u => u.Users);
            modelBuilder.Entity<User>().HasMany(u => u.Addresses);
        }

        public void AddMockData() {
            // Populate the database if it is empty
            foreach (var b in MockData.GetUsers()) {
                Users.Add(b);
            }
            foreach (var b in MockData.GetAddresses()) {
                Addresses.Add(b);
            }
            // ToDo - Add some User to Address associations here
            SaveChangesAsync();
        }

    }
}
