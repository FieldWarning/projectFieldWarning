/*
 * Copyright (c) 2017-present, PFW Contributors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in
 * compliance with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software distributed under the License is
 * distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See
 * the License for the specific language governing permissions and limitations under the License.
*/

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
using Shared;
using Shared.Models;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using Jwt = PFW_OfficialHub.Models.Jwt;

namespace PFW_OfficialHub.Controllers
{
    [Route("game")]
    [ApiController]
    public class PublicServerController : ControllerBase
    {
        [HttpPost("create")]
        public ActionResult<dynamic> CreateLobby([FromForm] GameLobby lobby, [FromForm] Jwt jwt)
        {
            if (!jwt.Verify()) return BadRequest(406);

            lobby.Id = null;
            lobby.IsRunning = false;
            lobby.HostUserId = jwt.UserId;
            lobby.Name = lobby.Name ?? $"{jwt.Username}'s game";
            lobby.LastHeartbeat = DateTime.UtcNow;
            Db.GameLobbies.InsertOne(lobby);
            return JsonConvert.SerializeObject(lobby);
        }

        [HttpPost("close/{id}")]
        public ActionResult<dynamic> CreateLobby(string id, [FromForm] Jwt jwt)
        {
            var lobby = Db.GameLobbies.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (!jwt.Verify()) return BadRequest(406);
            if (lobby.Result.HostUserId != jwt.UserId) return BadRequest(400);

            Db.GameLobbies.DeleteOneAsync(x => x.Id == id);
            return 200;
        }

        [HttpPost("serverhb")]
        public ActionResult<short> SHeartbeat([FromForm] GameLobby lobby, [FromForm] Jwt jwt)
        {
            var elob = Db.GameLobbies.Find(x => x.Id == lobby.Id).FirstOrDefaultAsync();
            if (!jwt.Verify()) return BadRequest("Bad token");
            if (elob.Result is null) return 404;
            if (elob.Result.HostUserId != jwt.UserId) return BadRequest("You do not own this server");
            var upd = new UpdateDefinitionBuilder<GameLobby>()
                .Set("LastHeartbeat", DateTime.UtcNow);
            _ = Db.GameLobbies.UpdateOneAsync(x => x.Id == elob.Result.Id, upd);
            
            return 200;
        }

        [HttpPost("playerhb")]
        public async Task<ActionResult<short>> PHeartbeat([FromForm] Jwt jwt)
        {
            var p = await Utils.GetPlayer(jwt);
            if (!jwt.Verify()) return BadRequest("Bad token");
            var upd = new UpdateDefinitionBuilder<Player>()
                .Set("LastSeen", DateTime.UtcNow);
            _ = Db.Players.UpdateOneAsync(x => x.Id == p.Id, upd);

            return 200;
        }

        [HttpPost("get")]
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
            //verify
            if (!jwt.Verify()) return BadRequest(403);

            return JsonConvert.SerializeObject(servers.Result);
        }

        [HttpPost("info/{serverId}")]
        public ActionResult<dynamic> Single([FromForm]Jwt jwt, string serverId)
        {
            //verify token sync
            if (!jwt.Verify()) return BadRequest(412);
            var srv = Db.GameLobbies.Find(x => x.Id == serverId).FirstOrDefault();
            if (srv is null) return 404;
            srv.Password = String.Empty;
            return JsonConvert.SerializeObject(srv);
        }
        [HttpPost("join/{serverId}")]
        public ActionResult<dynamic> Connect([FromForm]Jwt jwt, string serverId)
        {
            var srv = Db.GameLobbies.Find(x => x.Id == serverId).FirstOrDefaultAsync();
            var player = Db.Players.Find(x => jwt.UserId == x.UserId).FirstOrDefaultAsync();
            if (!jwt.Verify()) return BadRequest();
            if (srv.Result is null) return 404;
            
            var upd = new UpdateDefinitionBuilder<Player>()
                .Set("LastSeen", DateTime.UtcNow)
                .Set("CurrentLobbyId", serverId);

            _ = Db.Players.UpdateOneAsync(x => x.Id == player.Result.Id, upd);
            return 200;
        }
        [HttpPost("leave/{serverId}")]
        public ActionResult<dynamic> Leave([FromForm]Jwt jwt, string serverId)
        {
            var srv = Db.GameLobbies.Find(x => x.Id == serverId).FirstOrDefaultAsync();
            var player = Db.Players.Find(x => jwt.UserId == x.UserId).FirstOrDefaultAsync();
            if (!jwt.Verify()) return BadRequest();
            if (srv.Result is null) return 404;

            var upd = new UpdateDefinitionBuilder<Player>()
                .Set("LastSeen", DateTime.UtcNow)
                .Set("CurrentLobbyId", string.Empty);

            _ = Db.Players.UpdateOneAsync(x => x.Id == player.Result.Id, upd);
            return 200;
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

    }
}
