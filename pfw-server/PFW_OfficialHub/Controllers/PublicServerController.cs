using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PFW_OfficialHub.Controllers
{
    [Route("server")]
    [ApiController]
    public class PublicServerController : ControllerBase
    {
        [HttpPost("getall")]
        public ActionResult<string> GetAll()
        {
            return "";
        }

        [HttpPost("getinfo/{serverId}")]
        public ActionResult<string> GetAll(string serverId)
        {

            return "";
        }
        [HttpPost("connect/{serverId}")]
        public ActionResult<string> Connect(string serverId)
        {

            return "";
        }
        [HttpPost("leave/{serverId}")]
        public ActionResult<string> Leave(string serverId)
        {

            return "";
        }

        [HttpPost("votekick/{playerId}/{serverId}/{time}")]
        public ActionResult<string> VoteKick(string playerId, string serverId, int time)
        {

            return "";
        }
    }

    [Route("status")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        [HttpPost("info")]
        public ActionResult<string> GetServerStatus()
        {

            return "";
        }

        [HttpPost("heartbeat")]
        public ActionResult<string> SendHeartBeat()
        {

            return "";
        }
    }
}
