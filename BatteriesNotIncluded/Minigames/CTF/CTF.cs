using BatteriesNotIncluded.Framework;
using BatteriesNotIncluded.Framework.Extensions;
using BatteriesNotIncluded.Framework.MinigameTypes;
using BatteriesNotIncluded.Framework.Network.Packets;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using TShockAPI;

namespace BatteriesNotIncluded.Minigames.CTF {
    public class CTF : EndgameMinigame {
        public override string GamemodeName => "CTF";

        private List<TSPlayer> RedTeam = new List<TSPlayer>();
        private List<TSPlayer> BlueTeam = new List<TSPlayer>();

        private int _redScore = 0;
        private int _blueScore = 0;

        private int _scoreboardTick = 0;
        private string _scoreText => $"(Red: {_redScore}, Blue: {_blueScore})";

        private TSPlayer _redFlagHolder;
        private TSPlayer _blueFlagHolder;

        public CTF(Arena arena) : base(arena) { }

        public override void StartGame() {
            CTFArena arena = ActiveArena as CTFArena;

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
            for (int index = 0; index < RedTeam.Count; index++) {
                if (!Players.Contains(RedTeam[index])) {
                    if (_blueFlagHolder == RedTeam[index]) {
                        _blueFlagHolder = null;
                        SendMessageToAllPlayers($"{RedTeam[index].Name}'s flag has dropped.");
                    }
                    RedTeam.Remove(RedTeam[index]);
                    index--;
                }
            }

            for(int index = 0; index < BlueTeam.Count; index++) {
                if (!Players.Contains(BlueTeam[index])) {
                    if (_redFlagHolder == BlueTeam[index]) {
                        _redFlagHolder = null;
                        SendMessageToAllPlayers($"{BlueTeam[index].Name}'s flag has dropped.");
                    }
                    BlueTeam.Remove(BlueTeam[index]);
                    index--;
                }
            }

            _scoreboardTick++;
            if (_scoreboardTick / 60 == 1) {
                string teamWinning = _redScore > _blueScore ? "Red Team is winning!" : "Blue Team is winning!";
                if (_redScore == _blueScore) teamWinning = "Red and Blue are equal!";
                string bodyMessage = $"{teamWinning}\n" +
                    $"Red Players: {RedTeam.Count}\n" +
                    $"Blue Players: {BlueTeam.Count}\n" +
                    $"Score: {_scoreText}";

                if (_redFlagHolder != null) {
                    bodyMessage += $"\n{_redFlagHolder.Name} has the red flag.";
                }

                if (_blueFlagHolder != null) {
                    bodyMessage += $"\n{_blueFlagHolder.Name} has the blue flag.";
                }

                foreach (var player in Players) {
                    player.DisplayInterface("Capture The Flag Score", bodyMessage);
                }
                _scoreboardTick = 0;
            }
        }

        public override void OnPlayerData(TerrariaPacket e) {
            CTFArena arena = ActiveArena as CTFArena;
            PlayerTeamArgs playerTeam = e as PlayerTeamArgs;

            if (playerTeam != null) {
                if (RedTeam.Contains(e.Player)) {
                    e.Player.SetTeam(1);
                } else if (BlueTeam.Contains(e.Player)) {
                    e.Player.SetTeam(3);
                }
            }

            PlayerUpdateArgs playerUpdate = e as PlayerUpdateArgs;

            if (playerUpdate != null) {
                // Picking up flags
                if (_redFlagHolder == null && Vector2.Distance(new Vector2(playerUpdate.PositionX, playerUpdate.PositionY), arena.RedFlag) < 2 * 16 && BlueTeam.Contains(e.Player)) {
                    _redFlagHolder = e.Player;
                    SendMessageToAllPlayers($"{_redFlagHolder.Name} has picked up the red flag!", Color.OrangeRed);
                } else if (_blueFlagHolder == null && Vector2.Distance(new Vector2(playerUpdate.PositionX, playerUpdate.PositionY), arena.BlueFlag) < 2 * 16 && RedTeam.Contains(e.Player)) {
                    _blueFlagHolder = e.Player;
                    SendMessageToAllPlayers($"{_blueFlagHolder.Name} has picked up the blue flag!", Color.Turquoise);
                }

                // Placing flags at base flag
                if (_redFlagHolder != null && _redFlagHolder == e.Player && Vector2.Distance(new Vector2(playerUpdate.PositionX, playerUpdate.PositionY), arena.BlueFlag) < 2 * 16 && BlueTeam.Contains(e.Player) && !e.Player.TPlayer.dead) {
                    SendMessageToAllPlayers($"{_redFlagHolder.Name} has scored for the blue team!", Color.Turquoise);
                    _redFlagHolder = null;
                    _blueScore += 1;
                    SendMessageToAllPlayers($"Score: {_scoreText}", Color.Cyan);
                } else if (_blueFlagHolder != null && _blueFlagHolder == e.Player && Vector2.Distance(new Vector2(playerUpdate.PositionX, playerUpdate.PositionY), arena.RedFlag) < 2 * 16 && RedTeam.Contains(e.Player) && !e.Player.TPlayer.dead) {
                    SendMessageToAllPlayers($"{_blueFlagHolder.Name} has scored for the red team!", Color.OrangeRed);
                    _blueFlagHolder = null;
                    _redScore += 1;
                    SendMessageToAllPlayers($"Score: {_scoreText}", Color.Cyan);
                }
            }

            PlayerDeathArgs playerDeath = e as PlayerDeathArgs;

            if (playerDeath != null) {
                if (e.Player == _redFlagHolder) {
                    _redFlagHolder = null;
                    SendMessageToAllPlayers($"{e.Player.Name} has dropped the red flag!", Color.OrangeRed);
                }

                if (e.Player == _blueFlagHolder) {
                    _blueFlagHolder = null;
                    SendMessageToAllPlayers($"{e.Player.Name} has dropped the blue flag!", Color.Turquoise);
                }
            }
        }

        public override bool HasFinished() {
            return _redScore == 3 || _blueScore == 3;
        }

        public override bool InsufficientPlayers() {
            return RedTeam.Count < 1 || BlueTeam.Count < 1;
        }

        public override void OnFinished() {
            string winText = "(CTF) ";
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
