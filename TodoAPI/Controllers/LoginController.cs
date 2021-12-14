using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TodoAPI.DTOs;
using TodoAPI.Models;

namespace TodoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class LoginController : ControllerBase
    {
        private readonly TodoContext _todoContext;

        public LoginController(TodoContext todoContext)
        {
            _todoContext = todoContext;
        }

        [HttpPost]
        public string login(LoginPost value)
        {
            var user = (from person in _todoContext.Employees
                        where person.Account == value.Account
                        && person.Password == value.Password
                        select person).SingleOrDefault();

            if (user == null)
                return "帳號密碼錯誤";
            else
            {
                //驗證
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Account),
                    new Claim("FullName", user.Name),
                    new Claim("EmployeeId", user.EmployeeId.ToString())
                };

                var role = from person in _todoContext.Roles
                           where person.EmployeeId == user.EmployeeId
                           select person;

                foreach(var tmp in role)
                {
                    claims.Add(new Claim(ClaimTypes.Role, tmp.Name));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                return "OK";
            }
        }
        
        [HttpDelete]
        public void logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        
        [HttpGet("NoLogin")]
        public string noLogin()
        {
            return "未登入";
        }

        [HttpGet("NoAccess")]
        public string noAccess()
        {
            return "沒有權限";
        }

    }
}
