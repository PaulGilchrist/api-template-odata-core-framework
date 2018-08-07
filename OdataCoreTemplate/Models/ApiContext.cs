﻿using Microsoft.EntityFrameworkCore;
using ODataCoreTemplate.Models;

namespace OdataCoreTemplate.Models {
    public class ApiContext : DbContext {
        public ApiContext(DbContextOptions<ApiContext> options)
             : base(options) {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            //modelBuilder.Entity<User>().HasMany(u => u.Addresses);
        }
    }
}
