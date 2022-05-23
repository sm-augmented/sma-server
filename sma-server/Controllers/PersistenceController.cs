using Microsoft.AspNetCore.Mvc;
using SMAServer.Managers;
using SMAServer.Models;
using System.Collections.Concurrent;

namespace SMAServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersistenceController : ControllerBase
    {
        private readonly PersistenceManager persistence;

        public PersistenceController(PersistenceManager persistence)
        {
            this.persistence = persistence;
        }

        [HttpGet("getUsersDetails")]
        public ConcurrentDictionary<string, UserPing> GetUsersDetails()
        {
            return persistence.Players;
        }

        [HttpPost("registerUserIngame")]
        public void RegisterUserIngame([FromQuery]string branch, [FromQuery]string uid)
        {
            var hasPlayer = persistence.Players.TryGetValue(uid, out UserPing ping);

            if (hasPlayer && ping != null)
            {
                ping.PingsCount++;
                ping.Pinged = DateTime.Now;
                ping.Branch = branch;
            }
            else
            {
                persistence.Players.TryAdd(uid, new UserPing()
                {
                    Branch = branch,
                    Created = DateTime.Now,
                    ID = uid,
                    Pinged = DateTime.Now,
                    PingsCount = 0,
                    Status = ""
                });
            }
        }

        [HttpPost("unregisterUserIngame")]
        public void UnregisterUserIngame([FromQuery] string uid)
        {
            persistence.Players.TryRemove(uid, out UserPing ping);
        }
    }
}