using SMAServer.Models;
using System.Collections.Concurrent;

namespace SMAServer.Managers
{
    public class PersistenceManager
    {
        public ConcurrentDictionary<string, UserPing> Players { get; private set; }

        public PersistenceManager()
        {
            Players = new ConcurrentDictionary<string, UserPing>();
        }
    }
}
