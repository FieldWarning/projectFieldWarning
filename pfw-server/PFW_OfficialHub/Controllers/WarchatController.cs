using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization.Attributes;
using Shared;

namespace PFW_OfficialHub.Controllers
{
    [Route("warchat")]
    [ApiController]
    public class WarchatController : ControllerBase
    {
        public WarchatController()
        {
            Task.Factory.StartNew(delegate
            {
                while (true)
                {
                    Task.Delay(1000 * 60).Wait();
                    //Messages.
                }
            });
        }

        public static ConcurrentDictionary<string, WarchatMsg> Messages = new System.Collections.Concurrent.ConcurrentDictionary<string, WarchatMsg>();

        [HttpPost("send")]
        public ActionResult Send(WarchatMsg msg, Jwt jwt)
        {
            if (!jwt.Verify()) return StatusCode(406);

            msg.Time = DateTime.UtcNow;
            if (Messages.TryAdd(null, msg))
                return StatusCode(200);
            else return StatusCode(501);
        }

        [HttpPost("get/{since}")]
        public ActionResult<string> Get(DateTime since)
        {
            return "";
        }
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
