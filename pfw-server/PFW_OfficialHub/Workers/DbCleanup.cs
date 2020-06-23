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
