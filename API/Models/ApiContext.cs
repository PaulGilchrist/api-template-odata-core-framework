using Microsoft.EntityFrameworkCore;
using OdataCoreTemplate.Data;
using ODataCoreTemplate.Models;
using System;
using System.Threading.Tasks;

namespace OdataCoreTemplate.Models {
    public class ApiContext : DbContext {

        static Random rnd = new Random();

        public ApiContext(DbContextOptions<ApiContext> options)
             : base(options) {
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Address>().HasMany(u => u.Users);
            modelBuilder.Entity<User>().HasMany(u => u.Addresses);
        }

        public async Task AddMockDataAsync() {
            var userCount = await Users.CountAsync();
            if(userCount == 0) {
                var addressCount = await Addresses.CountAsync();
                // Populate the database if it is empty
                foreach (var user in MockData.GetUsers()) {
                    Users.Add(user);
                }
                foreach (var address in MockData.GetAddresses()) {
                    Addresses.Add(address);
                }
                await SaveChangesAsync();
                userCount = await Users.CountAsync();
                addressCount = await Addresses.CountAsync();
                // Associate 1 random address to every user
                foreach (var user in Users) {
                    int r = rnd.Next(addressCount);
                    var address = await Addresses.FindAsync(r);
                    user.Addresses.Add(address);
                }
                // Associate 1 random user to every address(means some users will have 2 addresses(a good thing)
                foreach (var address in Addresses) {
                    int r = rnd.Next(userCount);
                    var user = await Users.FindAsync(r);
                    address.Users.Add(user);
                }
                await SaveChangesAsync();
            }
        }

    }
}
