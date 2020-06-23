using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization.Attributes;
using Shared;
using System.Collections.Specialized;

using System.Collections;
using System.Net.Http;
using Database;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Shared.Models;
using Jwt = PFW_OfficialHub.Models.Jwt;

//using Shared.Models;

namespace PFW_OfficialHub.Controllers
{
    [Route("warchat")]
    [ApiController]
    public class WarchatController : ControllerBase
    {
        private static HttpClient httpClient = new HttpClient();

        public WarchatController()
        {
            Task.Factory.StartNew(delegate
            {
                while (true)
                {
                    Task.Delay(1000 * 60 * 5).Wait();
                    Db.Warchat.DeleteMany(x => x.Time < DateTime.UtcNow - TimeSpan.FromMinutes(60));
                }
            });
        }


        [HttpPost("send")]
        public ActionResult Send([FromForm]WarchatMsg msg, [FromForm]Jwt jwt)
        {
            if (!jwt.Verify()) return StatusCode(406);

            msg.Time = DateTime.UtcNow;
            Db.Warchat.InsertOne(msg);
            return StatusCode(200);
        }

        [HttpPost("get")]
        public ActionResult<string> Get([FromForm]string time, [FromForm]Jwt jwt)
        {
            if (!jwt.Verify()) return StatusCode(406);
            if (!DateTime.TryParse(time, out var since)) return StatusCode(400);
            if (since < DateTime.UtcNow - TimeSpan.FromMinutes(20))
                since = DateTime.UtcNow - TimeSpan.FromMinutes(20);

            var msgs = Db.Warchat.Find(x => x.Time >= since);

            return JsonConvert.SerializeObject(msgs);
        }

        [HttpPost("pm/{playerId}")]
        public ActionResult<string> Pm(string playerId, [FromForm]Jwt jwt, [FromForm]PrivateMessage msg)
        {
            if (!jwt.Verify()) return BadRequest(412);
            msg.Time = DateTime.UtcNow;
            var pt = Db.Players.Find(x => x.UserId == playerId).FirstOrDefaultAsync();
            if (msg.Content.Length > 240) msg.Content = msg.Content[0..240];

            //insert in db
            Db.Messages.InsertOne(msg);

            //notify client
            

            return "";
        }

        [HttpPost("pms")]
        public ActionResult<string> GetPms(Jwt jwt)
        {

            return "";
        }
    }
}
