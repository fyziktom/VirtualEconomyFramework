using System;

namespace VEUsersUtility
{
    class Program
    {
        static string DbProvider = "SQLite";
        const string ConnectionStringSQLiteDefault = "Data Source=C:\\VEFramework\\veframeworkdb.db";
        static string ConnectionString = "Data Source=C:\\VEFramework\\veframeworkdb.db";
        //static string ConnectionString = "Host=localhost;Port=5432;Database=veframework;Username=veadmin;Password=veadmin"; // for PostgreSQL
        static void Main(string[] args)
        {
            Console.WriteLine("---------------------");
            Console.WriteLine("VE Users Utility 1.0 ");
            Console.WriteLine("---------------------");
            Console.WriteLine("Default Connection String:" + ConnectionString);

            var end = false;

            while (!end)
            {
                Console.WriteLine("---------------------");
                Console.WriteLine("Select option:");
                Console.WriteLine("1. Add New User");
                Console.WriteLine("2. Set User Password");
                Console.WriteLine("3. Set Connection String");
                Console.WriteLine("4. Exit");
                Console.WriteLine("Please type number of option and hit enter:");
                var opt = Console.ReadLine();

                if (opt.Contains('1'))
                {
                    AddUser();
                }
                else if (opt.Contains('2'))
                {
                    SetUserPass();
                }
                else if (opt.Contains('3'))
                {
                    SetConnectionString();
                }
                else if (opt.Contains('4'))
                {
                    end = true;
                }
            }
        }

        static void AddUser()
        {
            Console.WriteLine("---------------------");
            Console.WriteLine("Adding new User");
            Console.WriteLine("---------------------");
            Console.WriteLine("Please input new User Login:");
            var login = Console.ReadLine();
            Console.WriteLine("Please input new User Name and Surname:");
            var name = Console.ReadLine();

            if (!string.IsNullOrEmpty(login) && !string.IsNullOrWhiteSpace(login) &&
                !string.IsNullOrEmpty(name) && !string.IsNullOrWhiteSpace(name))
            {
                UsersUtilities.AddUser(ConnectionString, DbProvider, $"{login},Administrator,{name}");
            }
            else
            {
                Console.WriteLine("Wrong input, please try it again.");
            }
        }

        static void SetUserPass()
        {
            Console.WriteLine("---------------------");
            Console.WriteLine("Setting User Password");
            Console.WriteLine("---------------------");
            Console.WriteLine("Please input new User Login:");
            var login = Console.ReadLine();
            Console.WriteLine("Please input new User Password:");
            var pass = Console.ReadLine();

            if (!string.IsNullOrEmpty(login) && !string.IsNullOrWhiteSpace(login) &&
                !string.IsNullOrEmpty(pass) && !string.IsNullOrWhiteSpace(pass))
            {
                UsersUtilities.SetUserPassword(ConnectionString, DbProvider, $"{login},{pass}");
            }
            else
            {
                Console.WriteLine("Wrong input, please try it again.");
            }
        }

        static void SetConnectionString()
        {
            Console.WriteLine("Setting of Db connecion");
            Console.WriteLine("Actual Db Provider:" + DbProvider);
            Console.WriteLine("Actual Connection string:" + ConnectionString);

            Console.WriteLine("Please input new Db Provider (hit enter for default - SQLite):");
            var dbp = Console.ReadLine();

            if (!string.IsNullOrEmpty(dbp) && !string.IsNullOrWhiteSpace(dbp))
            {
                DbProvider = dbp;

                Console.WriteLine("New Db Provider setted!");
                Console.WriteLine("New Db Provider:" + DbProvider);
            }
            else
            {
                DbProvider = "SQLite";

                Console.WriteLine("New Db Provider setted!");
                Console.WriteLine("New Db Provider:" + DbProvider);
            }

            Console.WriteLine("Please input new Connection string (hit enter for restore default):");
            var con = Console.ReadLine();

            if (!string.IsNullOrEmpty(con) && !string.IsNullOrWhiteSpace(con))
            {
                ConnectionString = con;

                Console.WriteLine("New Connection string setted!");
                Console.WriteLine("New Connection string:" + ConnectionString);
            }
            else
            {
                ConnectionString = ConnectionStringSQLiteDefault;
                Console.WriteLine("New Connection string setted!");
                Console.WriteLine("New Connection string:" + ConnectionString);
            }

        }
    }
}
