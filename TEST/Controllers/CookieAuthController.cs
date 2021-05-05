using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TEST.Data;
using TEST.DTOs;

namespace TEST.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CookieAuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ILogger _logger;

        public CookieAuthController(DataContext context, ILogger<CookieAuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CheckLogin(LoginDto loginDto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Email == loginDto.Email);  //call database to check if Email exists
            if (user == null) return StatusCode(401);
            byte[] bytes = Encoding.ASCII.GetBytes(user.PassswordSalt);
            using var hmac = new HMACSHA512(bytes);  //if username exist, use passwordsalt to check passwordhash identical
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            string computed_Hash = Encoding.ASCII.GetString(computedHash);
            if (computed_Hash != user.PasswordHash)
                return StatusCode(401);

            return new UserDto
            {
                Username = user.Email,
            };
        }
        public async Task<IActionResult> LogIn(LoginDto loginDto)   //set cookie
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Email == loginDto.Email);  //call database to check if Email exists
            if (user == null) return StatusCode(401);
            byte[] bytes = Encoding.ASCII.GetBytes(user.PassswordSalt);
            using var hmac = new HMACSHA512(bytes);  //if username exist, use passwordsalt to check passwordhash identical
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            string computed_Hash = Encoding.ASCII.GetString(computedHash);
            if (computed_Hash != user.PasswordHash)
                    return StatusCode(401);
          

            #region ***** 不使用ASP.NET Core Identity的 cookie 驗證 *****
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                //new Claim(ClaimTypes.Role, "Administrator"),            // // 如果要有「群組、角色、權限」
            };

            // 底下的 ** 登入 Login ** 需要下面兩個參數 (1) claimsIdentity  (2) authProperties
            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                //AllowRefresh = <bool>,
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

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
            #endregion
            return OK("登入成功");
        }

        private IActionResult OK(string v)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("User {Name} logged out at {Time}.",
                User.Identity.Name, DateTime.UtcNow);

            #region snippet1
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);  //若要登出目前的使用者並刪除其 cookie
            #endregion

            return Ok("/Account/SignedOut");
        }
    }
}
