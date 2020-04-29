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

        }

        [HttpPost("getinfo/{serverId}")]
        public ActionResult<string> GetAll(string serverId)
        {

        }
        [HttpPost("connect/{serverId}")]
        public ActionResult<string> Connect(string serverId)
        {

        }
        [HttpPost("leave/{serverId}")]
        public ActionResult<string> Leave(string serverId)
        {

        }

        [HttpPost("votekick/{playerId}/{serverId}/{time}")]
        public ActionResult<string> VoteKick(string playerId, string serverId, int time)
        {

        }
    }

    [Route("status")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        [HttpPost("info")]
        public ActionResult<string> GetServerStatus()
        {

        }

        [HttpPost("heartbeat")]
        public ActionResult<string> SendHeartBeat()
        {

        }
    }
}
