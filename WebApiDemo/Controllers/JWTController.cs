using DataAccess.DataConnectContext;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WebApiDemo.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class JWTController : Controller
    {
        private IConfiguration _config { get; }
        private DataContext _dbContext { get; }
        public JWTController(IConfiguration configuration, DataContext dbContext)
        {
            this._config = configuration;
            this._dbContext = dbContext;

        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult CreateToken([FromBody]User login)
        {
            IActionResult response = Unauthorized(); //if not authen then return 401
            var user = Authenticate(login);
            if(user != null)
            {
                var tokenString = BuildToken(user);
                response = Ok(new { token = tokenString });
            }

            return response;
        }

        private string BuildToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            string exp = _config["Jwt:Expires"];
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(exp));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.Username),
                new Claim(JwtRegisteredClaimNames.Email,user.Email),
                new Claim("sutin1990","Learn JWT By sutin"),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims,
                expires: expires,
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);


        }
        private User Authenticate(User login)
        {
            User user = _dbContext.User.FirstOrDefault(x => x.Username.Equals(login.Username) && x.Password.Equals(login.Password));
            return user;
        }
    }
}
