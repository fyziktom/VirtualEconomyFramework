using Microsoft.EntityFrameworkCore;
using System;
using VEDrivers.Common;
using VEDrivers.Database;
using VEDrivers.Security;

namespace VEUsersUtility
{
    internal static class UsersUtilities
    {
        public static void SetUserPassword(string conn, string provider, string param)
        {
            var split = param.Split(new[] { ' ', ',' }, 2, StringSplitOptions.RemoveEmptyEntries);
            var login = split[0];
            var pass = split[1];

            var hash = SecurityUtil.HashPassword(pass);

            //Console.WriteLine(SecurityUtil.VerifyPassword(pass, hash));

            using (var ctx = GetDbContext(conn, provider))
            {
                var user = ctx.Users.Find(login);
                user.PasswordHash = hash;
                ctx.SaveChanges();
            }
        }

        public static void AddUser(string conn, string provider, string param)
        {
            var split = param.Split(new[] { ' ', ',' }, 3, StringSplitOptions.RemoveEmptyEntries);
            using (var ctx = GetDbContext(conn, provider))
            {
                if (!Enum.TryParse<Rights>("Role" + split[1], out var rights))
                {
                    Console.WriteLine($"Nonexistent role {split[1]}");
                    return;
                }

                var entity = new UserEntity()
                {
                    Login = split[0],
                    Name = split[2],
                    Rights = rights,
                    PasswordHash = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 },
                    Active = true,
                    CreatedBy = Environment.UserName,
                    CreatedOn = DateTime.UtcNow,
                    ModifiedBy = Environment.UserName,
                    ModifiedOn = DateTime.UtcNow,
                };
                ctx.Users.Add(entity);
                ctx.SaveChanges();
            }

        }

        public static DbEconomyContext GetDbContext(string connectionString, string provider = "SQLite")
        {
            // Check Provider and get ConnectionString
            if (provider == "SQLite")
            {
                var optionsBuilder = new DbContextOptionsBuilder<DbEconomyContext>();
                optionsBuilder.UseSqlite(connectionString);
                return new DbEconomyContext(optionsBuilder.Options);
            }
            else if (provider == "MSSQL")
            {
                var optionsBuilder = new DbContextOptionsBuilder<DbEconomyContext>();
                optionsBuilder.UseSqlServer(connectionString);
                return new DbEconomyContext(optionsBuilder.Options);
            }
            else if (provider == "PostgreSQL")
            {
                var optionsBuilder = new DbContextOptionsBuilder<DbEconomyContext>();
                optionsBuilder.UseNpgsql(connectionString);
                return new DbEconomyContext(optionsBuilder.Options);
            }
            // Exception
            else
            { throw new ArgumentException("Not a valid database type"); }
        }
    }
}
