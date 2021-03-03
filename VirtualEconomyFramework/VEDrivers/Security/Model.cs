using System;
using System.Collections.Generic;
using System.Text;

namespace VEDrivers.Security
{
    public class Credentials
    {
        public string Login { get; set; }
        public string Pass { get; set; }
    }

    public class ChangePass
    {
        public string OldPass { get; set; }
        public string Pass { get; set; }
    }

    public class LoggedUser
    {
        public string Login { get; set; }
        public string Name { get; set; }
        public Rights Rights { get; set; }
    }

    public class UserDto
    {
        public string Login { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public Rights Rights { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool Active { get; set; }
    }
}
