using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using PFW_OfficialHub.Lib;
using PFW_OfficialHub.Models;
using Shared;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace PFW_OfficialHub.Controllers
{
    [Route("gameapi")]
    [ApiController]
    public class PublicServerController : ControllerBase
    {
        [HttpPost("serverhb")]
        public ActionResult<dynamic> Hearbeat([FromForm] GameLobby lobby, [FromForm] Jwt jwt)
        {
            if (!jwt.Verify()) return BadRequest("Bad token");
            var elob = Db.GameLobbies.Find(x => x.Id == lobby.Id).FirstOrDefaultAsync();
            
            return StatusCode(200);
        }

        [HttpPost("playerhb")]
        public async Task<ActionResult<string>> PHeartbeat([FromForm] Jwt jwt)
        {
            if (!jwt.Verify()) return BadRequest("Bad token");
            var p = await Utils.GetPlayer(jwt);
            var upd = new UpdateDefinitionBuilder<Player>().Set("LastSeen", DateTime.UtcNow);
            _ = Db.Players.UpdateOneAsync(x => x.Id == p.Id, upd);

            return StatusCode(200);
        }

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
            if (!jwt.Verify()) return BadRequest(403);

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
            var srv = Db.GameLobbies.Find(x => x.Id == serverId).FirstOrDefaultAsync();
            if (!jwt.Verify()) return BadRequest();
            var player = Db.Players.Find(x => jwt.Username == x.Username).FirstOrDefault();
            if (srv.Result == null) return BadRequest(404);
            
            var upd = new UpdateDefinitionBuilder<Player>().Set("LastSeen", DateTime.UtcNow).Set("CurrentLobbyId", serverId);
            _ = Db.Players.UpdateOneAsync(x => x.Id == player.Id, upd);


            return StatusCode(200);
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

    [Route("player")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        [HttpPost("friend")]
        public ActionResult<string> AddFriend()
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
