using SMAServer.Managers;
using SMAServer.Models;

namespace SMAServer.Workers
{
    public class PersistenceWorker : BackgroundService
    {
        private readonly PersistenceManager persistence;

        public PersistenceWorker(PersistenceManager persistence)
        {
            this.persistence = persistence;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(600000, stoppingToken);

                var expiredPlayers = persistence.Players.Where(x => x.Value.Pinged.AddMinutes(18) < DateTime.Now).ToList();
                expiredPlayers.ForEach(x => persistence.Players.TryRemove(x.Key, out UserPing ping));
            }
        }
    }
}
