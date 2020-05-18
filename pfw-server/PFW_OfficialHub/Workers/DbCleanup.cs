using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using MongoDB.Driver;
namespace PFW_OfficialHub.Workers
{
    public class DbCleanup {
        private Task playerCleanupTask;
        private Task lobbyCleanupTask;
        public void StartPlayerCleanup() {
            playerCleanupTask = Task.Run(async delegate {
                while (true) {
                    await Task.Delay(1000 * 10);
                    Db.Players.DeleteMany(x => x.LastSeen < DateTime.UtcNow - TimeSpan.FromSeconds(15));
                }
            });
        }

        public void StopPlayerCleanup() {
            playerCleanupTask.Dispose();
        }

    }
}
