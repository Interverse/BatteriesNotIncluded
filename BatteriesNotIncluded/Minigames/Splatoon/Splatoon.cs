using BatteriesNotIncluded.Framework;
using BatteriesNotIncluded.Framework.Extensions;
using BatteriesNotIncluded.Framework.Network.Packets;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace BatteriesNotIncluded.Minigames.Splatoon {
    public class Splatoon : Minigame {
        public override string GamemodeName => "Splatoon";

        private List<TSPlayer> RedTeam = new List<TSPlayer>();
        private List<TSPlayer> BlueTeam = new List<TSPlayer>();

        // Variables for timers for announcing Splatoon
        private int _alertCounter = 0;
        private DateTime _lastAlert;
        private int _alertDelay = 10000;

        // Variables for countdown delay
        private int _respawnCounter = 4;
        private DateTime _respawnTimer;
        private int _respawnDelay = 1000;

        private DateTime _gameStart;
        private DateTime _lastGameTimeAnnounce;
        private DateTime _gameDuration = new DateTime().AddMinutes(3); // 3 minutes
        private int _minutes = 3;
        private int _matchTimeCounter = 3;

        private int _redScore = 0;
        private int _blueScore = 0;
        private float _totalPaintSpots = 0;

        private int _deepRedPaintID = 13;
        private int _deepBluePaintID = 21;

        private int _scoreboardTick = 0;

        private string _score => $"(Red: {(_redScore / _totalPaintSpots * 100).ToString("0.##")}%, Blue: {(_blueScore / _totalPaintSpots * 100).ToString("0.##")}%)";

        public Splatoon(Arena arena) : base(arena) {
            _lastAlert = DateTime.Now;
            ServerApi.Hooks.ProjectileAIUpdate.Register(Main.Instance, OnProjectileUpdate);
        }

        public override void Initialize() {
            TShock.Utils.Broadcast($"Splatoon vote has started. Type '/splatoon join' to join. (Arena: {ActiveArena.Name}) ({(3 - _alertCounter) * 10}s left)", Color.Cyan);
            Players[0].SendSuccessMessage("You've been added to Splatoon.");
        }

        public override bool PreGame() {
            if ((DateTime.Now - _lastAlert).TotalMilliseconds >= _alertDelay) {
                _lastAlert = DateTime.Now;
                _alertCounter += 1;

                if (_alertCounter < 3) {
                    TShock.Utils.Broadcast($"Splatoon vote currently running. Type '/splatoon join' to join. (Arena: {ActiveArena.Name}) ({(3 - _alertCounter) * 10}s left)", Color.Cyan);
                } else {
                    TShock.Utils.Broadcast($"Current Splatoon vote has ended.", Color.Cyan);
                }
            }

            return _alertCounter < 3;
        }

        public override bool HasVoteSucceeded() {
            return Players.Count > 1;
        }

        public override void FailStart() {
            SendMessageToAllPlayers("Splatoon vote failed. Not enough players.");
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
                    SendMessageToAllPlayers($"Splatoon beginning in {_respawnCounter}...");
                } else {
                    SendMessageToAllPlayers("Go!");
                }

                SetPvP(true);
            }

            return _respawnCounter > 0;
        }

        public override void StartGame() {
            SplatoonArena arena = ActiveArena as SplatoonArena;

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

            _gameStart = DateTime.Now;
            _lastGameTimeAnnounce = DateTime.Now;
            ResetArena();
            CountScore();
        }

        public override void OnRunning() {
            if ((DateTime.Now - _lastGameTimeAnnounce).TotalSeconds >= 60) {
                _lastGameTimeAnnounce = DateTime.Now;
                _matchTimeCounter--;

                if (_matchTimeCounter > 0) {
                    SendMessageToAllPlayers($"(Splatoon) {_matchTimeCounter} minutes remain!");
                }
            }

            for (int index = 0; index < RedTeam.Count; index++) {
                if (!Players.Contains(RedTeam[index])) {
                    RedTeam.Remove(RedTeam[index]);
                    index--;
                }
            }

            for (int index = 0; index < BlueTeam.Count; index++) {
                if (!Players.Contains(BlueTeam[index])) {
                    BlueTeam.Remove(BlueTeam[index]);
                    index--;
                }
            }

            _scoreboardTick++;
            if (_scoreboardTick / 60 == 1) {
                DateTime timeRemaining = _gameDuration - (DateTime.Now - _gameStart);
                string bodyMessage = $"Time Remaining: {timeRemaining.Minute}:{timeRemaining.Second.ToString("D2")}\n" +
                    $"Red Players: {RedTeam.Count()}\n" +
                    $"Blue Players: {BlueTeam.Count()}\n" +
                    $"Score: {_score}";

                foreach (var player in Players) {
                    player.DisplayInterface("Splatoon Score", bodyMessage);
                }
                _scoreboardTick = 0;
            }
        }

        public override void OnPlayerData(TerrariaPacket e) {
            if (GetGameState() != GameState.GameRunning) return;

            PlayerDeathArgs deathPacket = e as PlayerDeathArgs;

            if (deathPacket != null) {
                if (RedTeam.Contains(e.Player)) {
                    PaintCircle((int)e.Player.TPlayer.Center.X, (int)e.Player.TPlayer.Center.Y, (int)(3 + deathPacket.Damage / 20f), _deepBluePaintID);
                } else if (BlueTeam.Contains(e.Player)) {
                    PaintCircle((int)e.Player.TPlayer.Center.X, (int)e.Player.TPlayer.Center.Y, (int)(3 + deathPacket.Damage / 20f), _deepRedPaintID);
                }
            }

            ProjectileDestroyArgs projectileDied = e as ProjectileDestroyArgs;

            if (projectileDied != null) {
                if (RedTeam.Contains(e.Player)) {
                    PaintCircle((int)projectileDied.ProjectileX, (int)projectileDied.ProjectileY, 3 + projectileDied.ProjectileDestroyed.damage / 150, _deepRedPaintID);
                } else if (BlueTeam.Contains(e.Player)) {
                    PaintCircle((int)projectileDied.ProjectileX, (int)projectileDied.ProjectileY, 3 + projectileDied.ProjectileDestroyed.damage / 150, _deepBluePaintID);
                }
            }
        }

        private void OnProjectileUpdate(ProjectileAiUpdateEventArgs args) {
            if (GetGameState() != GameState.GameRunning) return;
            var proj = args.Projectile;
            if (proj.owner >= TShock.Players.Length || proj.owner < 0) return;
            var player = TShock.Players[proj.owner];

            if (RedTeam.Contains(player)) {
                PaintTile((int)proj.Center.X, (int)proj.Center.Y, _deepRedPaintID);
            } else if (BlueTeam.Contains(player)) {
                PaintTile((int)proj.Center.X, (int)proj.Center.Y, _deepBluePaintID);
            }
        }

        public override bool HasFinished() {
            return (DateTime.Now - _gameStart).TotalMinutes >= _minutes;
        }

        public override bool InsufficientPlayers() {
            return RedTeam.Count < 1 || BlueTeam.Count < 1;
        }

        public override void OnFinished() {
            SendMessageToAllPlayers("Splatoon has ended!", Color.Cyan);

            CountScore();

            List<string> winners = new List<string>();
            string winText = "(Splatoon) ";
            if (_redScore > _blueScore) {
                winText += "Red team wins! ";
                foreach (var winner in RedTeam) {
                    winners.Add(winner.Name);
                }
            } else if (_blueScore > _redScore) {
                winText += "Blue team wins!";
                foreach (var winner in BlueTeam) {
                    winners.Add(winner.Name);
                }
            } else {
                winText += "It was a tie!";
            }

            winText += $" {_score}";

            if (winners.Count > 0) {
                winText += " Congrats to " + string.Join(", ", winners) + "!";
            }

            TShock.Utils.Broadcast(winText, Color.Cyan);
        }

        public override void OnFailedFinished() {
            SendMessageToAllPlayers("Insufficient players to continue Splatoon.");
        }

        public override void OnCleanup() {
            ServerApi.Hooks.ProjectileAIUpdate.Deregister(Main.Instance, OnProjectileUpdate);
            ResetArena();
        }

        private void PaintTile(int x, int y, int color) {
            var arena = ActiveArena as SplatoonArena;
            if (x < arena.ArenaTopLeft.X || x > arena.ArenaBottomRight.X || y < arena.ArenaTopLeft.Y || y > arena.ArenaBottomRight.Y) return;

            var tile = Terraria.Main.tile[x / 16, y / 16];

            if (tile.type <= 0 && tile.wall <= 0) return;

            if (tile.color() != color) {
                if (color == _deepBluePaintID) _blueScore++;
                else if (color == _deepRedPaintID) _redScore++;

                if (tile.color() == _deepBluePaintID) _blueScore--;
                else if (tile.color() == _deepRedPaintID) _redScore--;

                tile.color((byte)color);
                NetMessage.SendData((int)PacketTypes.PaintTile, -1, -1, null, x / 16, y / 16, color);
            }

            if (tile.wallColor() != color) {
                if (color == _deepBluePaintID) _blueScore++;
                else if (color == _deepRedPaintID) _redScore++;

                if (tile.wallColor() == _deepBluePaintID) _blueScore--;
                else if (tile.wallColor() == _deepRedPaintID) _redScore--;

                tile.wallColor((byte)color);
                NetMessage.SendData((int)PacketTypes.PaintWall, -1, -1, null, x / 16, y / 16, color);
            }
        }

        private void PaintCircle(int x, int y, int radius, int color) {
            for (int col = -radius; col < radius; col++) {
                for (int row = -radius; row < radius; row++) {
                    double distanceSquared = col * col + row * row;

                    if (distanceSquared <= radius * radius) {
                        PaintTile(x + row * 16, y + col * 16, color);
                    }
                }
            }
        }

        private void CountScore() {
            SplatoonArena arena = ActiveArena as SplatoonArena;

            _totalPaintSpots = 0;
            _redScore = 0;
            _blueScore = 0;

            for (int x = (int)arena.ArenaTopLeft.X / 16; x <= (int)arena.ArenaBottomRight.X / 16; x++) {
                for (int y = (int)arena.ArenaTopLeft.Y / 16; y <= (int)arena.ArenaBottomRight.Y / 16; y++) {
                    var tile = Terraria.Main.tile[x, y];
                    if (tile.type > 0) _totalPaintSpots++;
                    if (tile.wall > 0) _totalPaintSpots++;

                    if (tile.color() == _deepRedPaintID) _redScore++;
                    if (tile.wallColor() == _deepRedPaintID) _redScore++;
                    if (tile.color() == _deepBluePaintID) _blueScore++;
                    if (tile.wallColor() == _deepBluePaintID) _blueScore++;
                }
            }
        }

        private void ResetArena() {
            SplatoonArena arena = ActiveArena as SplatoonArena;

            for (int x = (int)arena.ArenaTopLeft.X / 16; x <= (int)arena.ArenaBottomRight.X / 16; x++) {
                for (int y = (int)arena.ArenaTopLeft.Y / 16; y <= (int)arena.ArenaBottomRight.Y / 16; y++) {
                    var coord = new Vector2(x, y);

                    int tileColor = 0;
                    int wallColor = 0;
                    if (arena.PaintSpotTiles.ContainsKey(coord)) tileColor = arena.PaintSpotTiles[coord];
                    if (arena.PaintSpotWalls.ContainsKey(coord)) wallColor = arena.PaintSpotWalls[coord];

                    if (Terraria.Main.tile[x, y].color() != tileColor) {
                        Terraria.Main.tile[x, y].color((byte)tileColor);
                        NetMessage.SendData((int)PacketTypes.PaintTile, -1, -1, null, x, y, tileColor);
                    }

                    if (Terraria.Main.tile[x, y].wallColor() != wallColor) {
                        Terraria.Main.tile[x, y].wallColor((byte)wallColor);
                        NetMessage.SendData((int)PacketTypes.PaintWall, -1, -1, null, x, y, wallColor);
                    }
                }
            }
        }
    }
}
