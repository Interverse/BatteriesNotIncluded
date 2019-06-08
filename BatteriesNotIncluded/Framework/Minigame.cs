using System;
using System.Collections.Generic;
using TShockAPI;

namespace BatteriesNotIncluded.Framework {
    public abstract class Minigame {
        public List<TSPlayer> Players = new List<TSPlayer>();
        private GameState _state = GameState.Initializing;

        public virtual void AddPlayer(TSPlayer player) {
            Players.Add(player);
        }

        public virtual void OnGameRunning(EventArgs e) {
            switch (_state) {
                case GameState.Initializing:
                    Initialize();
                    _state = GameState.PreGame;
                    return;

                case GameState.PreGame:
                    if (!PreGame()) {
                        if (!HasVoteSucceeded()) {
                            FailStart();
                            _state = GameState.Finished;
                            return;
                        }
                        _state = GameState.Countdown;
                    }
                    return;

                case GameState.Countdown:
                    if (!Countdown())
                        _state = GameState.GameRunning;

                    return;

                case GameState.GameRunning:
                    OnRunning();

                    if (HasFinished())
                        _state = GameState.Finished;

                    return;

                case GameState.Finished:
                    OnFinished();
                    Deregister();
                    return;
            }
        }

        public abstract void Initialize();
        public abstract bool PreGame();
        public abstract bool HasVoteSucceeded();
        public abstract void FailStart();
        public abstract bool Countdown();
        public abstract void OnRunning();
        public abstract void OnFinished();
        public abstract bool HasFinished();

        private void Deregister() {
            Main.Instance.DeregisterGame(this);
        }
    }

    internal enum GameState {
        Initializing,
        PreGame,
        Countdown,
        GameRunning,
        Finished
    }
}
