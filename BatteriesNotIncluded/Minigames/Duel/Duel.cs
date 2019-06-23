using BatteriesNotIncluded.Framework;
using BatteriesNotIncluded.Framework.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using TShockAPI;

namespace BatteriesNotIncluded.Minigames.Duel {
    public class Duel : Minigame {
        private TSPlayer challenger;
        private TSPlayer opponent;

        private string _scoreText => $"({challenger.Name}: {_challengerScore}, {opponent.Name}: {_opponentScore})";

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

        public override string GamemodeName => "Duel";

        public Duel(TSPlayer challenger, TSPlayer opponent, DuelArena arena) : base(arena) {
            AddPlayer(challenger);
            AddPlayer(opponent);

            this.challenger = Players[0];
            this.opponent = Players[1];

            _lastAlert = DateTime.Now;
        }

        public override void Initialize() {
            challenger.SendMessage($"Challenged {opponent.Name} to a duel on {ActiveArena.Name}", Color.Cyan);
            challenger.SendMessage($"Waiting for {opponent.Name} to accept...", Color.Aquamarine);
            opponent.SendMessage($"{challenger.Name} has challenged you to a duel (Arena: {ActiveArena.Name})! Type '/duel accept' to accept.", Color.Cyan);
        }

        public override bool PreGame() {
            if ((DateTime.Now - _lastAlert).TotalMilliseconds >= _alertDelay) {
                _lastAlert = DateTime.Now;
                _alertCounter += 1;
                challenger.SendMessage($"Waiting for {opponent.Name} to accept...", Color.Cyan);
                opponent.SendMessage(challenger.Name + " has challenged you to a duel! Type '/duel accept' to accept.", Color.Cyan);
            }
            
            return _alertCounter < 3 && !opponent.GetDuelAccept();
        }

        public override bool HasVoteSucceeded() {
            _respawnTimer = DateTime.Now;
            return opponent.GetDuelAccept();
        }

        public override void FailStart() {
            challenger.SendMessage(opponent.Name + " did not accept your challenge in time.", Color.Yellow);
            opponent.SendMessage("You failed to accept " + challenger.Name + "'s duel request.", Color.Yellow);
        }
        public override void StartGame() {
            DuelArena arena = ActiveArena as DuelArena;

            challenger.SetGamemodeSpawnPoint(arena.Player1Spawn);
            opponent.SetGamemodeSpawnPoint(arena.Player2Spawn);

            challenger.SendMessage(opponent.Name + " has accepted your challenge.", Color.Cyan);
            opponent.SendMessage($"You have accepted {challenger.Name}'s duel.", Color.Cyan);
        }

        public override bool Countdown() {
            if ((DateTime.Now - _respawnTimer).TotalMilliseconds >= _respawnDelay) {
                challenger.SpawnOnSpawnPoint();
                opponent.SpawnOnSpawnPoint();

                challenger.Heal();
                opponent.Heal();

                _respawnTimer = DateTime.Now;
                _respawnCounter -= 1;

                if (_respawnCounter > 0) {
                    SendMessageToAllPlayers($"Duel beginning in {_respawnCounter}...");
                } else {
                    SendMessageToAllPlayers("Go!");
                }

                string personWinning = _challengerScore > _opponentScore ? challenger.Name + " is winning!" : opponent.Name + " is winning!";
                if (_challengerScore == _opponentScore) personWinning = "Both people are equal in score!";
                string bodyMessage = $"{personWinning}\n" +
                    $"Challenger: {challenger.Name}\n" +
                    $"Opponent: {opponent.Name}\n" +
                    $"Score: {_scoreText}";

                foreach (var player in Players) {
                    player.DisplayInterface("Duel Score", bodyMessage);
                }
                _scoreboardTick = 0;
            }

            return _respawnCounter > 0;
        }

        public override void OnRunning() {
            if (challenger.Dead || opponent.Dead) {
                if (challenger.Dead) {
                    _opponentScore += 1;
                } else if (opponent.Dead) {
                    _challengerScore += 1;
                }

                string winner = challenger.Dead ? opponent.Name : challenger.Name;
                string message = $"{winner} scored! {_scoreText}";
                SendMessageToAllPlayers(message);

                if (!HasFinished()) {
                    SetGameState(GameState.Countdown);
                    _respawnTimer = DateTime.Now;
                    _respawnCounter = 6;
                }
            }

            _scoreboardTick++;
            if (_scoreboardTick / 60 == 1) {
                string personWinning = _challengerScore > _opponentScore ? challenger.Name + " is winning!" : opponent.Name + " is winning!";
                if (_challengerScore == _opponentScore) personWinning = "Both people are equal in score!";
                string bodyMessage = $"{personWinning}\n" +
                    $"Challenger: {challenger.Name}\n" +
                    $"Opponent: {opponent.Name}\n" +
                    $"Score: {_scoreText}";

                foreach (var player in Players) {
                    player.DisplayInterface("Duel Score", bodyMessage);
                }
                _scoreboardTick = 0;
            }
        }
        public override bool InsufficientPlayers() {
            return Players.Count < 2;
        }

        public override bool HasFinished() {
            return _challengerScore == 5 || _opponentScore == 5;
        }

        public override void OnFinished() {
            string winner = _challengerScore == 5 ? challenger.Name : opponent.Name;

            string scoreMessage = $"{winner} has won the duel! {_scoreText}";

            TShock.Utils.Broadcast(scoreMessage, Color.Cyan);
        }

        public override void OnFailedFinished() {
            SendMessageToAllPlayers("Duel ended due to opponent leaving.", Color.Cyan);
        }

        public override void OnCleanup() {
            opponent.SetDuelAccept(false);
            opponent.SetPendingDuel(default);
        }
    }
}
