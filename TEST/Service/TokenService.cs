using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TEST.Entities;
using TEST.Interface;

namespace TEST.Service
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;  
        public TokenService(IConfiguration configuration)   //use constructor to inject
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["TokenKey"]));
        }

        public string CreateToken(AppUser user)
        {
            var claim = new List<Claim>   //what's going to be claimed
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Email)  //use nameId to store eMAIL
            };
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature); //how to sign token
            var tokenDescriptor = new SecurityTokenDescriptor   //describe how token looks like
            {
                Subject = new ClaimsIdentity(claim),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();   //an object actually generate token
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
