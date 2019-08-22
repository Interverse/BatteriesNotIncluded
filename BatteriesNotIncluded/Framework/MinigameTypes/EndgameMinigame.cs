using BatteriesNotIncluded.Framework.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.MinigameTypes {
    public abstract class EndgameMinigame : Minigame {
        // Variables for timers for announcing Splatoon
        private int _alertCounter = 0;
        private DateTime _lastAlert;
        private int _alertDelay = 10000;

        // Variables for countdown delay
        private int _respawnCounter = 4;
        private DateTime _respawnTimer;
        private int _respawnDelay = 1000;

        public EndgameMinigame(Arena arena) : base(arena) {
            _lastAlert = DateTime.Now;
        }

        public override void Initialize() {
            TShock.Utils.Broadcast($"{GamemodeName} vote has started. Type '/join' to join. (Arena: {ActiveArena.Name}) ({(3 - _alertCounter) * 10}s left)", Color.Cyan);
        }

        public override bool PreGame() {
            if ((DateTime.Now - _lastAlert).TotalMilliseconds >= _alertDelay) {
                _lastAlert = DateTime.Now;
                _alertCounter += 1;

                if (_alertCounter < 3) {
                    TShock.Utils.Broadcast($"{GamemodeName} vote currently running. Type '/join' to join. (Arena: {ActiveArena.Name}) ({(3 - _alertCounter) * 10}s left)", Color.Cyan);
                } else {
                    TShock.Utils.Broadcast($"Current {GamemodeName} vote has ended.", Color.Cyan);
                }
            }

            return _alertCounter < 3;
        }

        public override bool HasVoteSucceeded() {
            return Players.Count > 1;
        }

        public override void FailStart() {
            SendMessageToAllPlayers($"{GamemodeName} vote failed. Not enough players.");
        }

        public override bool Countdown() {
            if ((DateTime.Now - _respawnTimer).TotalMilliseconds >= _respawnDelay) {
                foreach (var player in Players) {
                    player.SpawnOnSpawnPoint();
                    player.Heal();
                }

                _respawnTimer = DateTime.Now;
                _respawnCounter -= 1;

                if (_respawnCounter > 0) {
                    SendMessageToAllPlayers($"{GamemodeName} beginning in {_respawnCounter}...");
                } else {
                    SendMessageToAllPlayers("Go!");
                }

                SetPvP(true);
            }

            return _respawnCounter > 0;
        }

        public override void OnFailedFinished() {
            SendMessageToAllPlayers($"Insufficient players to continue {GamemodeName}.");
        }
    }
}
