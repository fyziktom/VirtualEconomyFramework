using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Database.Models;
using VEDrivers.Economy.Wallets;
using VEDrivers.Nodes;

namespace VEDrivers.Database
{
    public class DbEconomyContext : DbContext
    {
        public static string ConnectString { private get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Node> Nodes { get; set; }

        public DbEconomyContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                    .UseNpgsql(ConnectString)
                    .UseLazyLoadingProxies();
    }
}
