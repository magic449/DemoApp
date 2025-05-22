using DemoAppAPI.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using DemoAppAPI.DAL;

namespace DemoAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login(string userName, string password){

            AuthDAL authDAL = new AuthDAL(_configuration);
            bool res = authDAL.login(userName, password);
            if (res == true)
            {   string token = authDAL.generateToken(userName, password);         
                return Ok(new {token = token});
            }
            return BadRequest("error logging in");
        }

        [HttpPost("register")]
        public IActionResult Register(string userName, string password)
        {
            AuthDAL authDAL = new AuthDAL(_configuration);
            string res = authDAL.register(userName, password);
            
            if(res == "success")
                return Ok(res);
            else
                return BadRequest(res);
        }

    }
}
