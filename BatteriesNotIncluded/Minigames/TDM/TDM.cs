using BatteriesNotIncluded.Framework;
using BatteriesNotIncluded.Framework.Extensions;
using BatteriesNotIncluded.Framework.MinigameTypes;
using BatteriesNotIncluded.Framework.Network.Packets;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace BatteriesNotIncluded.Minigames.TDM {
    public class TDM : EndgameMinigame {
        public override string GamemodeName => "TDM";

        private List<TSPlayer> RedTeam = new List<TSPlayer>();
        private List<TSPlayer> BlueTeam = new List<TSPlayer>();

        private int _redScore = 0;
        private int _blueScore = 0;

        private int _scoreboardTick = 0;
        private string _scoreText => $"(Red: {_redScore}, Blue: {_blueScore})";

        public TDM(Arena arena) : base(arena) { }

        public override void StartGame() {
            TDMArena arena = ActiveArena as TDMArena;

            foreach (var player in Players) {
                if (RedTeam.Count < BlueTeam.Count) {
                    RedTeam.Add(player);
                    player.SetGamemodeSpawnPoint(arena.RedSpawn);
                    player.SetTeam(1);
                } else {
                    BlueTeam.Add(player);
                    player.SetGamemodeSpawnPoint(arena.BlueSpawn);
                    player.SetTeam(3);
                }
            }
        }

        public override void OnRunning() {
            _scoreboardTick++;
            if (_scoreboardTick / 60 == 1) {
                string teamWinning = _redScore > _blueScore ? "Red Team is winning!" : "Blue Team is winning!";
                if (_redScore == _blueScore) teamWinning = "Red and Blue are equal!";
                string bodyMessage = $"{teamWinning}\n" +
                    $"Red Players: {RedTeam.Count}\n" +
                    $"Blue Players: {BlueTeam.Count}\n" +
                    $"Score: {_scoreText}";

                foreach (var player in Players) {
                    player.DisplayInterface("Team Deathmatch Score", bodyMessage);
                }
                _scoreboardTick = 0;
            }
        }

        public override void OnPlayerData(TerrariaPacket e) {
            TDMArena arena = ActiveArena as TDMArena;
            PlayerTeamArgs playerTeam = e as PlayerTeamArgs;

            if (playerTeam != null) {
                if (RedTeam.Contains(e.Player)) {
                    e.Player.SetTeam(1);
                } else if (BlueTeam.Contains(e.Player)) {
                    e.Player.SetTeam(3);
                }
            }

            PlayerDeathArgs playerDeath = e as PlayerDeathArgs;

            if (playerDeath != null) {
                if (RedTeam.Contains(e.Player)) {
                    _blueScore++;
                    SendMessageToAllPlayers($"A member of Blue Team got a kill! {_scoreText}", Color.Turquoise);
                } else if (BlueTeam.Contains(e.Player)) {
                    _redScore++;
                    SendMessageToAllPlayers($"A member of Red Team got a kill! {_scoreText}", Color.OrangeRed);
                }
            }
        }

        public override bool HasFinished() {
            return _redScore == Main.Config.TDMMaxScore || _blueScore == Main.Config.TDMMaxScore;
        }

        public override bool InsufficientPlayers() {
            return RedTeam.Count < 1 || BlueTeam.Count < 1;
        }

        public override void OnFinished() {
            string winText = "(TDM) ";
            List<string> winners = new List<string>();

            if (_redScore == 3) {
                winText = "Red team has won! ";

                foreach (var winner in RedTeam) {
                    winners.Add(winner.Name);
                }
            } else {
                winText = "Blue team has won! ";

                foreach (var winner in BlueTeam) {
                    winners.Add(winner.Name);
                }
            }

            winText += $"{_scoreText} Congrats to " + string.Join(", ", winners) + "!";

            TShock.Utils.Broadcast(winText, _redScore == 3 ? Color.OrangeRed : Color.Turquoise);
        }
    }
}
