using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Configuration {
    public class ApiDbContext : DbContext {

        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) {
        }

        public DbSet<Address> Addresses { get; set; }
        public DbSet<AddressNote> AddressNotes { get; set; }
        public DbSet<ClaimRoles> ClaimRoles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserNote> UserNotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Address>(entity => {
                entity.HasMany(a => a.Users);
                entity.HasMany(a => a.Notes);
                entity.Property(c => c.Type).HasConversion<int>(); // Store enum in database as an int
            });
            modelBuilder.Entity<ClaimRoles>();
            modelBuilder.Entity<User>(entity => {
                entity.HasMany(u => u.Addresses);
                entity.HasMany(u => u.Notes);
            });
        }

    }
}
