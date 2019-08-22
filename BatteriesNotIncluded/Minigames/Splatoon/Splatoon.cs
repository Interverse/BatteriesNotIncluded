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
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace BatteriesNotIncluded.Minigames.Splatoon {
    public class Splatoon : EndgameMinigame {
        public override string GamemodeName => "Splatoon";

        private List<TSPlayer> RedTeam = new List<TSPlayer>();
        private List<TSPlayer> BlueTeam = new List<TSPlayer>();

        private DateTime _gameStart;
        private DateTime _lastGameTimeAnnounce;
        private DateTime _gameDuration = new DateTime().AddSeconds(Main.Config.SplatoonTimerInSeconds);
        private int _matchTimeCounter = Main.Config.SplatoonTimerInSeconds;

        private int _redScore = 0;
        private int _blueScore = 0;
        private float _totalPaintSpots = 0;

        private int _deepRedPaintID = 13;
        private int _deepBluePaintID = 21;

        private int _scoreboardTick = 0;

        private int _tilesPerUpdate = 100;
        private int _tilesUpdated = 0;

        private string _score => $"(Red: {(_redScore / _totalPaintSpots * 100).ToString("0.##")}%, Blue: {(_blueScore / _totalPaintSpots * 100).ToString("0.##")}%)";

        public Splatoon(Arena arena) : base(arena) {
            ServerApi.Hooks.ProjectileAIUpdate.Register(Main.Instance, OnProjectileUpdate);
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
                _matchTimeCounter -= 60;

                if (_matchTimeCounter > 0) {
                    SendMessageToAllPlayers($"(Splatoon) {_matchTimeCounter} seconds remain! {_score}");
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

            _tilesUpdated = 0;
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
                if (projectileDied.ProjectileDestroyed.owner == projectileDied.Owner) {
                    if (RedTeam.Contains(e.Player)) {
                        PaintCircle((int)projectileDied.ProjectileX, (int)projectileDied.ProjectileY, 3 + projectileDied.ProjectileDestroyed.damage / 150, _deepRedPaintID);
                    } else if (BlueTeam.Contains(e.Player)) {
                        PaintCircle((int)projectileDied.ProjectileX, (int)projectileDied.ProjectileY, 3 + projectileDied.ProjectileDestroyed.damage / 150, _deepBluePaintID);
                    }
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
            return (DateTime.Now - _gameStart).TotalSeconds >= Main.Config.SplatoonTimerInSeconds;
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

        public override void OnCleanup() {
            ServerApi.Hooks.ProjectileAIUpdate.Deregister(Main.Instance, OnProjectileUpdate);
            ResetArena();
        }

        private void PaintTile(int x, int y, int color) {
            var arena = ActiveArena as SplatoonArena;
            if (x < arena.ArenaTopLeft.X || x > arena.ArenaBottomRight.X || y < arena.ArenaTopLeft.Y || y > arena.ArenaBottomRight.Y) return;
            if (_tilesUpdated > _tilesPerUpdate) return;

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

            _tilesUpdated++;
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
                    }

                    if (Terraria.Main.tile[x, y].wallColor() != wallColor) {
                        Terraria.Main.tile[x, y].wallColor((byte)wallColor);
                    }
                }
            }

            ResetSection((int)arena.ArenaTopLeft.X / 16, (int)arena.ArenaBottomRight.X / 16, (int)arena.ArenaTopLeft.Y / 16, (int)arena.ArenaBottomRight.Y / 16);
        }

        public void ResetSection(int x, int x2, int y, int y2) {
            int lowX = Netplay.GetSectionX(x);
            int highX = Netplay.GetSectionX(x2);
            int lowY = Netplay.GetSectionY(y);
            int highY = Netplay.GetSectionY(y2);
            foreach (RemoteClient sock in Netplay.Clients.Where(s => s.IsActive)) {
                for (int i = lowX; i <= highX; i++) {
                    for (int j = lowY; j <= highY; j++)
                        sock.TileSections[i, j] = false;
                }
            }
        }
    }
}
