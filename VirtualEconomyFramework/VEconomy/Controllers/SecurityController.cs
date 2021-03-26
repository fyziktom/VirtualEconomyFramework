using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using VEconomy.Common;
using VEDrivers.Database;
using VEDrivers.Security;

namespace VEconomy.Controllers
{
    [Route("api/security")]
    [ApiController]
    public class SecurityController : Controller
    {
        /*
        private readonly IConfiguration settings;
        public SecurityController(IConfiguration settings)
        {
            this.settings = settings;
        }
        */
        private readonly IDbConnectorService dbService;
        private readonly DbEconomyContext _context;
        public SecurityController(DbEconomyContext context)
        {
            _context = context;
            dbService = new DbConnectorService(_context);
        }

        [HttpGet]
        [Route("getRights")]
        public Dictionary<int,string> getRights()
        {
            var dict = new Dictionary<int, string>();
            var values = Enum.GetValues(typeof(Rights));

            foreach (var value in values)
            {
                int key = (int)value;

                dict.TryAdd(key, value.ToString());
            }

            return dict;
        }

        [HttpGet]
        [Route("login")]
        public LoggedUser QueryLogin()
        {
            var identity = ((ClaimsIdentity)User?.Identity);
            if (identity == null || !identity.IsAuthenticated) return null;
            var claims = identity.Claims;
            Enum.TryParse<Rights>(claims?.FirstOrDefault(c => c.Type == nameof(LoggedUser.Rights))?.Value, out var rights);
            return new LoggedUser()
            {
                Login = claims.FirstOrDefault(c => c.Type == nameof(LoggedUser.Login))?.Value,
                Name = claims.FirstOrDefault(c => c.Type == nameof(LoggedUser.Name))?.Value,
                Rights = rights,
            };
        }

        [HttpPost]
        [Route("login")]
        public async Task<LoggedUser> DoLogin([FromBody] Credentials credentials)
        {
            await Logout();
            var user = ValidateUser(credentials);
            if (user == null) throw new HttpResponseException(HttpStatusCode.BadRequest, "Invalid credentials supplied");

            var claims = new[]
            {
                new Claim(nameof(LoggedUser.Login), user.Login),
                new Claim(nameof(LoggedUser.Name), user.Name ),
                new Claim(nameof(LoggedUser.Rights), ((int)user.Rights).ToString()),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                //AllowRefresh = true,
                // Refreshing the authentication session should be allowed.

                //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                // The time at which the authentication ticket expires. A 
                // value set here overrides the ExpireTimeSpan option of 
                // CookieAuthenticationOptions set with AddCookie.

                //IsPersistent = true,
                // Whether the authentication session is persisted across 
                // multiple requests. When used with cookies, controls
                // whether the cookie's lifetime is absolute (matching the
                // lifetime of the authentication ticket) or session-based.

                //IssuedUtc = <DateTimeOffset>,
                // The time at which the authentication ticket was issued.

                //RedirectUri = <string>
                // The full path or absolute URI to be used as an http 
                // redirect response value.
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
            return user;
        }

        private LoggedUser ValidateUser(Credentials credentials)
        {
            //using (var ctx = new SecurityDatabaseContext(settings["ConnectionStrings:VEFrameworkDb"]))
            {
                var now = DateTime.UtcNow;
                var user = _context.Users
                    .Where(u => u.Login == credentials.Login && u.Active && (u.ValidFrom == null || u.ValidFrom <= now) && (u.ValidTo == null || u.ValidTo >= now))
                    .Select(u => new { u.Login, u.Name, u.Rights, u.PasswordHash })
                    .FirstOrDefault();

                if (user == null) return null;
                if (!SecurityUtil.VerifyPassword(credentials.Pass, user.PasswordHash)) return null;

                return new LoggedUser()
                {
                    Login = user.Login,
                    Name = user.Name,
                    Rights = user.Rights,
                };
            }
        }

        [HttpPost]
        [Route("logout")]
        public async Task Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        [HttpPut]
        [Route("changePass")]
        public void ChangePassword(ChangePass data)
        {
            var login = CurrentLogin();
            var now = DateTime.UtcNow;
            try
            {
                //using (var ctx = new SecurityDatabaseContext(settings["ConnectionStrings:VEFrameworkDb"]))
                {
                    var user = _context.Users.FirstOrDefault(u => u.Login == login);
                    if (user == null) throw new Exception();

                    if (!SecurityUtil.VerifyPassword(data.OldPass, user.PasswordHash)) throw new Exception();

                    user.PasswordHash = SecurityUtil.HashPassword(data.Pass);
                    user.ModifiedBy = login;
                    user.ModifiedOn = now;
                    _context.SaveChanges();
                }
            }
            catch 
            {
                throw new HttpResponseException(HttpStatusCode.InternalServerError, $"Cannot change password for user '{login}'");
            }

        }

        /*----------------------- administration of users --------------------------*/

        [HttpGet]
        [Authorize(Rights.Administration)]
        [Route("users")]
        public IEnumerable<UserDto> ListUsers()
        {
            //using (var ctx = new SecurityDatabaseContext(settings["ConnectionStrings:VEFrameworkDb"]))
            {
                return _context.Users.Select(u => new UserDto
                {
                    Login = u.Login,
                    Name = u.Name,
                    Email = u.Email,
                    Description = u.Description,
                    Rights = u.Rights,
                    ValidFrom = u.ValidFrom,
                    ValidTo = u.ValidTo,
                    Active = u.Active,
                }).ToList();
            }
        }

        [HttpPost]
        [Authorize(Rights.Administration)]
        [Route("user")]
        public void AddUser([FromBody] UserDto data)
        {
            var login = CurrentLogin();
            var now = DateTime.UtcNow;
            try
            {
                //using (var ctx = new SecurityDatabaseContext(settings["ConnectionStrings:VEFrameworkDb"]))
                {
                    if (_context.Users.Any(u => u.Login == data.Login)) throw new HttpResponseException(HttpStatusCode.BadRequest, $"Duplicate user '{data.Login}'");
                    _context.Users.Add(new UserEntity()
                    {
                        Login = data.Login,
                        Name = data.Name,
                        Email = data.Email,
                        Description = data.Description,
                        Rights = data.Rights,
                        ValidFrom = data.ValidFrom,
                        ValidTo = data.ValidTo,
                        Active = data.Active,

                        CreatedBy = login,
                        CreatedOn = now,
                        ModifiedBy = login,
                        ModifiedOn = now,
                    });
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                if (ex is HttpResponseException) throw;
                throw new HttpResponseException(HttpStatusCode.InternalServerError, $"Cannot add user '{data.Login}'");
            }
        }

        [HttpPut]
        [Authorize(Rights.Administration)]
        [Route("user")]
        public void UpdateUser(UserDto data)
        {
            var login = CurrentLogin();
            var now = DateTime.UtcNow;
            try
            {
                //using (var ctx = new SecurityDatabaseContext(settings["ConnectionStrings:VEFrameworkDb"]))
                {
                    var user = _context.Users.FirstOrDefault(u => u.Login == data.Login);
                    if (user == null) throw new HttpResponseException(HttpStatusCode.BadRequest, $"Non existing user '{data.Login}'");
                    //user.Login = data.Login;
                    user.Name = data.Name;
                    user.Email = data.Email;
                    user.Description = data.Description;
                    user.Rights = data.Rights;
                    user.ValidFrom = data.ValidFrom;
                    user.ValidTo = data.ValidTo;
                    user.Active = data.Active;

                    user.ModifiedBy = login;
                    user.ModifiedOn = now;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                if (ex is HttpResponseException) throw;
                throw new HttpResponseException(HttpStatusCode.InternalServerError, $"Cannot update user '{data.Login}'");
            }
        }

        [HttpDelete]
        [Authorize(Rights.Administration)]
        [Route("user")]
        public void RemoveUser([FromBody]string login)
        {
            try
            {
                //using (var ctx = new SecurityDatabaseContext(settings["ConnectionStrings:VEFrameworkDb"]))
                {
                    var user = _context.Users.FirstOrDefault(u => u.Login == login);
                    if (user == null) throw new HttpResponseException(HttpStatusCode.BadRequest, $"Non existing user '{login}'");
                    _context.Users.Remove(user);
                    //user.Active = false;
                    //user.ModifiedBy = login; TODO: change to current login
                    //user.ModifiedOn = now; TODO: get now
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                if (ex is HttpResponseException) throw; 
                throw new HttpResponseException(HttpStatusCode.InternalServerError, $"Cannot delete user '{login}'"); 
            }
        }

        [HttpPut]
        [Authorize(Rights.Administration)]
        [Route("userPassword")]
        public void UpdateUserPassword(Credentials data)
        {
            var login = CurrentLogin();
            var now = DateTime.UtcNow;
            try
            {
                //using (var ctx = new SecurityDatabaseContext(settings["ConnectionStrings:VEFrameworkDb"]))
                {
                    var user = _context.Users.FirstOrDefault(u => u.Login == data.Login);
                    if (user == null) throw new HttpResponseException(HttpStatusCode.BadRequest, $"Non existing user '{data.Login}'");
                    user.PasswordHash = SecurityUtil.HashPassword(data.Pass);
                    user.ModifiedBy = login;
                    user.ModifiedOn = now;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                if (ex is HttpResponseException) throw;
                throw new HttpResponseException(HttpStatusCode.InternalServerError, $"Cannot update user '{data.Login}'"); 
            }
        }


        private string CurrentLogin()
        {
            return ((ClaimsIdentity)User?.Identity)?.Claims.FirstOrDefault(c => c.Type == nameof(LoggedUser.Login))?.Value ?? "unknown";
        }
    }
}