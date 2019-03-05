﻿using Microsoft.Extensions.Caching.Memory;
using OdataCoreTemplate.Models;
using System;
using System.Threading.Tasks;

namespace API.Classes {
    public class Security {
        private IMemoryCache _cache;
        private OdataCoreTemplate.Models.ApiDbContext _db;

        public Security(ApiDbContext context, IMemoryCache memoryCache) {
            _cache = memoryCache;
            _db = context;
        }

        public async Task<string[]> GetRoles(string name) {
            // Returns a comma separated list of claim roles as a string
            // First try and get roles from memory cache
            string[] roles = null;
            if (!_cache.TryGetValue("roles-" + name, out roles)) {
                // try and get roles from database
                var claimRoles = await _db.ClaimRoles.FindAsync(name);
                if(claimRoles != null) {
                    roles = claimRoles.Roles.Split(",");
                }
                // Add roles to the cache even if they were not found in the database (roles=null)
                MemoryCacheEntryOptions memoryCacheEntryOptions = new MemoryCacheEntryOptions();
                memoryCacheEntryOptions.SetSlidingExpiration(new TimeSpan(4, 0, 0));
                _cache.Set("roles-" + name, roles, memoryCacheEntryOptions);
            }
            return roles;
        }
    }
}
