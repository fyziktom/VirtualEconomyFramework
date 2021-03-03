using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace VEDrivers.Security
{
    public class SecurityDatabaseContext : DbContext
    {
        private readonly string connectString;
        public DbSet<UserEntity> Users { get; set; }

        public SecurityDatabaseContext(string connectString) 
        {
            this.connectString = connectString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                    .UseNpgsql(connectString)
                    .UseLazyLoadingProxies();
    }
}


