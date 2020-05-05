using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Shared;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace PFW_OfficialHub.Controllers
{
    [Route("server")]
    [ApiController]
    public class PublicServerController : ControllerBase
    {
        [HttpPost("getall")]
        public ActionResult<string> GetAll([FromForm]Jwt jwt, [FromForm]LobbySearchFilter filter)
        {
            //discover servers async
            var servers = Db.GameLobbies.Find(x =>
                x.IsRunning == filter.RunningGames
                && (x.Description.Contains(filter.NameContains) || x.Name.Contains(filter.NameContains))
                && (filter.HasPassword
                    ? (!string.IsNullOrWhiteSpace(x.Password) && string.IsNullOrWhiteSpace(x.Password))
                    : string.IsNullOrWhiteSpace(x.Password))
                && filter.GameModes.Contains(x.GameMode)).ToListAsync();

            //verify token sync
            if (!jwt.Verify()) return BadRequest(412);

            return JsonConvert.SerializeObject(servers.Result);
        }

        [HttpPost("getinfo/{serverId}")]
        public ActionResult<string> Single([FromForm]Jwt jwt, string serverId)
        {
            //verify token sync
            if (!jwt.Verify()) return BadRequest(412);
            var srv = Db.GameLobbies.Find(x => x.Id == serverId).FirstOrDefault();
            return JsonConvert.SerializeObject(srv);
        }
        [HttpPost("join/{serverId}")]
        public ActionResult<string> Connect([FromForm]Jwt jwt, string serverId)
        {

            return "200";
        }
        [HttpPost("leave/{serverId}")]
        public ActionResult<string> Leave([FromForm]Jwt jwt, string serverId)
        {

            return "";
        }

        [HttpPost("votekick/{playerId}/{serverId}")]
        public ActionResult<string> VoteKick([FromForm]Jwt jwt, string playerId, string serverId)
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
