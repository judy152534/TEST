using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TEST.Data;
using TEST.DTOs;
using TEST.Entities;
using TEST.Interface;

namespace TEST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogInController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public LogInController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Email))
                return BadRequest("Email Already Exist");
            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                Email = registerDto.Email,
                PasswordHash = Encoding.ASCII.GetString( hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password))),
                PassswordSalt = Encoding.ASCII.GetString(hmac.Key)
            };
            _context.Users.Add(user);  //tracking this in EF
            await _context.SaveChangesAsync();     //call the database and save these 
            return new UserDto
            {
                Username = user.Email,
                Token = _tokenService.CreateToken(user)
            };  
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
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
                Token = _tokenService.CreateToken(user)
            };
        }
        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.Email == username);
        }
    }
}
