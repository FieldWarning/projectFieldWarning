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
using MongoDB.Driver;

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
        public ActionResult Send(WarchatMsg msg, Jwt jwt)
        {
            if (!jwt.Verify()) return StatusCode(406);

            msg.Time = DateTime.UtcNow;
            Db.Warchat.InsertOne(msg);
            return StatusCode(200);
        }

        [HttpPost("get/{since}")]
        public ActionResult<string> Get(DateTime since)
        {
            if (since < DateTime.UtcNow - TimeSpan.FromMinutes(60))
                since = DateTime.UtcNow - TimeSpan.FromMinutes(60);

            var msgs = Db.Warchat.Find(x => x.Time >= since);

            return "";
        }

        [HttpPost("pm/{playerId}")]
        public ActionResult<string> Pm(string playerId, Jwt jwt, PrivateMessage msg)
        {
            if (!jwt.Verify()) return BadRequest(412);
            msg.Time = DateTime.UtcNow;
            var pt = Db.OnlinePlayers.Find(x => x.UserId == playerId).FirstOrDefaultAsync();
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



    [BsonNoId]
    public class PrivateMessage
    {
        public string SenderId { get; set; }
        public string TargetId { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }
    }


    [BsonNoId]
    public class WarchatMsg {
        public string Username { get; set; }
        public DateTime Time { get; set; }
        public string Content {
            get {
                return Content;
            } 
            set {
                if (Content.Length > 240) Content = Content[0..240]; 
            } 
        }

    }
}
