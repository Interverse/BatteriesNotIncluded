using BatteriesNotIncluded.Framework;
using BatteriesNotIncluded.Framework.Extensions;
using BatteriesNotIncluded.Framework.MinigameTypes;
using BatteriesNotIncluded.Framework.Network.Packets;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using TShockAPI;

namespace BatteriesNotIncluded.Minigames.Doubles {
    public class Doubles : EndgameMinigame {
        private bool _challengerWasTeamed = false;
        private bool _opponentWasTeamed = false;

        private TSPlayer _challenger;
        private TSPlayer _teammate;
        private TSPlayer _opponent1;
        private TSPlayer _opponent2;

        private string _scoreText => $"(Challenger: {_challengerScore}, Opponent: {_opponentScore})";

        private int _challengerScore = 0;
        private int _opponentScore = 0;

        // Variables for alerting duels
        private int _alertCounter = 0;
        private DateTime _lastAlert;
        private int _alertDelay = 10000;

        // Variables for respawn delay
        private int _respawnCounter = 6;
        private DateTime _respawnTimer;
        private int _respawnDelay = 1000;

        private int _scoreboardTick = 0;

        public override string GamemodeName => "Doubles";

        public Doubles(TSPlayer challenger, TSPlayer teammate, TSPlayer opponent1, TSPlayer opponent2, DoublesArena arena) : base(arena) {
            AddPlayer(challenger);
            AddPlayer(teammate);
            AddPlayer(opponent1);
            AddPlayer(opponent2);

            this._challenger = Players[0];
            this._teammate = Players[1];
            this._opponent1 = Players[2];
            this._opponent2 = Players[3];

            if (_challenger.HasDoublesTeammate()) _challengerWasTeamed = true;
            if (_opponent1.HasDoublesTeammate()) _opponentWasTeamed = true;

            _lastAlert = DateTime.Now;
        }

        public override void Initialize() {
            _challenger.SendMessage($"Initialized a doubles match. Teammate: {_teammate.Name}. Opponents: {_opponent1.Name}, {_opponent2.Name}. Arena: {ActiveArena.Name}", Color.Cyan);
            _challenger.SendMessage($"Waiting for everyone to accept...", Color.Aquamarine);
            _teammate.SendMessage($"{_challenger.Name} has initialized a doubles match (Arena: {ActiveArena.Name})! Type '/doubles accept' to accept.", Color.Cyan);
            _opponent1.SendMessage($"{_challenger.Name} (Teammate: {_teammate.Name}) has challenged you to doubles. Your teammate is {_opponent2.Name}. (Arena: {ActiveArena.Name})! Type '/doubles accept' to accept.", Color.Cyan);
            _opponent2.SendMessage($"{_challenger.Name} (Teammate: {_teammate.Name}) has challenged you to doubles. Your teammate is {_opponent1.Name}. (Arena: {ActiveArena.Name})! Type '/doubles accept' to accept.", Color.Cyan);
        }

        public override bool PreGame() {
            if ((DateTime.Now - _lastAlert).TotalMilliseconds >= _alertDelay) {
                _lastAlert = DateTime.Now;
                _alertCounter += 1;
                _challenger.SendMessage($"Waiting for everyone to accept...", Color.Cyan);
                _teammate.SendMessage($"{_challenger.Name} has initialized a doubles match (Arena: {ActiveArena.Name})! Type '/doubles accept' to accept.", Color.Cyan);
                _opponent1.SendMessage($"{_challenger.Name} (Teammate: {_teammate.Name}) has challenged you to doubles. Your teammate is {_opponent2.Name}. (Arena: {ActiveArena.Name})! Type '/doubles accept' to accept.", Color.Cyan);
                _opponent2.SendMessage($"{_challenger.Name} (Teammate: {_teammate.Name}) has challenged you to doubles. Your teammate is {_opponent1.Name}. (Arena: {ActiveArena.Name})! Type '/doubles accept' to accept.", Color.Cyan);
            }
            
            return _alertCounter < 3 && !(_teammate.GetDoublesAccept() && _opponent1.GetDoublesAccept() && _opponent2.GetDoublesAccept());
        }

        public override bool HasVoteSucceeded() {
            _respawnTimer = DateTime.Now;
            return _teammate.GetDoublesAccept() && _opponent1.GetDoublesAccept() && _opponent2.GetDoublesAccept();
        }

        public override void FailStart() {
            _challenger.SendMessage("Everyone did not accept your challenge in time.", Color.Yellow);
            _teammate.SendMessage("Not everyone accepted the doubles challenge in time.", Color.Yellow);
            _opponent1.SendMessage("Not everyone accepted the doubles challenge in time.", Color.Yellow);
            _opponent2.SendMessage("Not everyone accepted the doubles challenge in time.", Color.Yellow);
        }
        public override void StartGame() {
            DoublesArena arena = ActiveArena as DoublesArena;

            _challenger.SetGamemodeSpawnPoint(arena.Team1Spawn1);
            _teammate.SetGamemodeSpawnPoint(arena.Team1Spawn2);
            _opponent1.SetGamemodeSpawnPoint(arena.Team2Spawn1);
            _opponent2.SetGamemodeSpawnPoint(arena.Team2Spawn2);

            _challenger.SetDoublesTeammate(_teammate);
            _teammate.SetDoublesTeammate(_challenger);
            _opponent1.SetDoublesTeammate(_opponent2);
            _opponent2.SetDoublesTeammate(_opponent1);

            _challenger.SetTeam(1);
            _teammate.SetTeam(1);
            _opponent1.SetTeam(3);
            _opponent2.SetTeam(3);

            _challenger.SendMessage("Everyone has accepted your challenge.", Color.Cyan);
            _teammate.SendMessage($"You have accepted {_challenger.Name}'s duel.", Color.Cyan);
            _opponent1.SendMessage($"You have accepted {_challenger.Name}'s duel.", Color.Cyan);
            _opponent2.SendMessage($"You have accepted {_challenger.Name}'s duel.", Color.Cyan);
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
                    SendMessageToAllPlayers($"Doubles beginning in {_respawnCounter}...");
                } else {
                    SendMessageToAllPlayers("Go!");
                }

                string personWinning = _challengerScore > _opponentScore ? "Challenegers are is winning!" : "Opponents are winning!";
                if (_challengerScore == _opponentScore) personWinning = "Both people are equal in score!";
                string bodyMessage = $"{personWinning}\n" +
                    $"Challenger: {_challenger.Name} + {_teammate.Name}\n" +
                    $"Opponent: {_opponent1.Name} + {_opponent2.Name}\n" +
                    $"Score: {_scoreText}";

                foreach (var player in Players) {
                    player.DisplayInterface("Doubles Score", bodyMessage);
                }
                _scoreboardTick = 0;
            }

            return _respawnCounter > 0;
        }

        public override void OnRunning() {
            if (_challenger.IsTeamDead() || _opponent1.IsTeamDead()) {
                if (_challenger.IsTeamDead()) {
                    _opponentScore += 1;
                } else if (_opponent1.IsTeamDead()) {
                    _challengerScore += 1;
                }

                DoublesArena arena = ActiveArena as DoublesArena;

                _challenger.SetGamemodeSpawnPoint(arena.Team1Spawn1);
                _teammate.SetGamemodeSpawnPoint(arena.Team1Spawn2);
                _opponent1.SetGamemodeSpawnPoint(arena.Team2Spawn1);
                _opponent2.SetGamemodeSpawnPoint(arena.Team2Spawn2);

                string winner = _challenger.IsTeamDead() ? $"{_opponent1.Name} and {_opponent2.Name}" : $"{_challenger.Name} and {_teammate.Name}";
                string message = $"{winner} scored! {_scoreText}";
                SendMessageToAllPlayers(message);

                _challenger.ResetTeamDead();
                _opponent1.ResetTeamDead();

                if (!HasFinished()) {
                    SetGameState(GameState.Countdown);
                    _respawnTimer = DateTime.Now;
                    _respawnCounter = 6;
                }
            }
            
            _scoreboardTick++;
            if (_scoreboardTick / 60 == 1) {
                string personWinning = _challengerScore > _opponentScore ? "Challenegers are is winning!" : "Opponents are winning!";
                if (_challengerScore == _opponentScore) personWinning = "Both people are equal in score!";
                string bodyMessage = $"{personWinning}\n" +
                    $"Challenger: {_challenger.Name} + {_teammate.Name}\n" +
                    $"Opponent: {_opponent1.Name} + {_opponent2.Name}\n" +
                    $"Score: {_scoreText}";

                foreach (var player in Players) {
                    player.DisplayInterface("Doubles Score", bodyMessage);
                }
                _scoreboardTick = 0;
            }
        }

        public override void OnPlayerData(TerrariaPacket e) {
            if (GetGameState() != GameState.GameRunning) return;

            PlayerDeathArgs deathPacket = e as PlayerDeathArgs;

            if (deathPacket != null) {
                e.Player.SetDoublesDead(true);
                e.Player.SetGamemodeSpawnPoint(((DoublesArena)ActiveArena).SpectateArea);
            }

            PlayerTeamArgs playerTeam = e as PlayerTeamArgs;

            if (playerTeam != null) {
                if (e.Player == _challenger || e.Player == _teammate) {
                    e.Player.SetTeam(1);
                } else if (e.Player == _opponent1 || e.Player == _opponent2) {
                    e.Player.SetTeam(3);
                }
            }

            PlayerHurtArgs hurtPacket = e as PlayerHurtArgs;

            if (hurtPacket != null) {
                if (e.Player.GetDoublesDead()) {
                    hurtPacket.Args.Handled = true;
                    e.Player.SendErrorMessage("You cannot hurt people while dead.");
                }
            }
        }

        public override bool InsufficientPlayers() {
            return Players.Count < 4;
        }

        public override bool HasFinished() {
            return _challengerScore == Main.Config.DuelMaxScore || _opponentScore == Main.Config.DuelMaxScore;
        }

        public override void OnFinished() {
            string winner = _challengerScore == Main.Config.DuelMaxScore ? $"{_challenger.Name} and {_teammate.Name}" : $"{_opponent1.Name} and {_opponent2.Name}";

            string scoreMessage = $"{winner} has won the doubles match! {_scoreText}";

            TShock.Utils.Broadcast(scoreMessage, Color.Cyan);
        }

        public override void OnFailedFinished() {
            SendMessageToAllPlayers("Doubles ended due to opponent leaving.", Color.Cyan);
        }

        public override void OnCleanup() {
            foreach (var player in Players) {
                player.SetDoublesAccept(false);
                player.SetPendingDoubles(default);
            }

            if (!_challengerWasTeamed) {
                _challenger.RemoveTeammate();
            }

            if (!_opponentWasTeamed) {
                _opponent1.RemoveTeammate();
            }
        }
    }
}
