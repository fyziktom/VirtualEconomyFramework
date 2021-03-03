using System;
using VEDrivers.Security;

namespace VEUsersUtility
{
    internal static class UsersUtilities
    {
        public static void SetUserPassword(string conn, string param)
        {
            var split = param.Split(new[] { ' ', ',' }, 2, StringSplitOptions.RemoveEmptyEntries);
            var login = split[0];
            var pass = split[1];

            var hash = SecurityUtil.HashPassword(pass);

            //Console.WriteLine(SecurityUtil.VerifyPassword(pass, hash));

            using (var ctx = new SecurityDatabaseContext(conn))
            {
                var user = ctx.Users.Find(login);
                user.PasswordHash = hash;
                ctx.SaveChanges();
            }
        }

        public static void AddUser(string conn, string param)
        {
            var split = param.Split(new[] { ' ', ',' }, 3, StringSplitOptions.RemoveEmptyEntries);
            using (var ctx = new SecurityDatabaseContext(conn))
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
    }
}
