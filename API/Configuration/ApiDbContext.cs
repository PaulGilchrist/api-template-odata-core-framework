﻿using API.Models;
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
        public object Markets { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Address>().HasMany(a => a.Users);
            modelBuilder.Entity<Address>().HasMany(a => a.Notes);
            modelBuilder.Entity<Address>().Property(c => c.Type).HasConversion<int>(); // Store enum in database as an int
            modelBuilder.Entity<ClaimRoles>();
            modelBuilder.Entity<User>().HasMany(u => u.Addresses);
            modelBuilder.Entity<User>().HasMany(u => u.Notes);
        }

    }
}