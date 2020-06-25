using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PFW_OfficialHub.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PFW_OfficialHub.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public ActionResult<dynamic> Login([FromForm] string username, [FromForm] string password)
        {
            var user = Db.Users.Find(x => x.Username == username).FirstOrDefault();
            if (user.Password != password) return 400;
            
        }

        [HttpPost("logout")]
        public ActionResult<dynamic> Logout([FromForm] Jwt jwt, [FromForm] bool all = false)
        {

        }

        [HttpPost("verify")]
        public ActionResult<dynamic> Verify([FromForm] Jwt jwt)
        {

        }


    }
}
