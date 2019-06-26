using BatteriesNotIncluded.Framework.Extensions;
using BatteriesNotIncluded.Framework.Network;
using BatteriesNotIncluded.Framework.Network.Packets;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.MinigameTypes {
    public abstract class Minigame {
        public List<TSPlayer> Players = new List<TSPlayer>();
        public Arena ActiveArena;
        private bool ForcePvP = true;
        private GameState _state = GameState.Initializing;

        public abstract string GamemodeName { get; }

        public Minigame(Arena arena) {
            ActiveArena = arena;
            ActiveArena.Available = false;
            ServerApi.Hooks.GameUpdate.Register(Main.Instance, OnGameUpdate);
            DataHandler.OnPlayerGetData += HandlePlayerData;
        }

        public virtual void AddPlayer(TSPlayer player) {
            if (!Players.Contains(player)) {
                Players.Add(player);
            }
        }

        public virtual void OnGameUpdate(EventArgs args) {
            //Console.WriteLine(_state.ToString());
            switch (_state) {
                case GameState.Initializing:
                    Initialize();
                    _state = GameState.PreGame;
                    break;

                case GameState.PreGame:
                    if (!PreGame()) {
                        if (!HasVoteSucceeded()) {
                            FailStart();
                            _state = GameState.Cleanup;
                        } else {
                            foreach (var player in Players) {
                                player.SetGamemode(GamemodeName);
                            }
                            _state = GameState.StartGame;
                        }

                        foreach (var player in Players) {
                            player.SetOldPosition(player.LastNetPosition);
                        }

                        Main.ActiveVote = null;
                    }
                    break;

                case GameState.StartGame:
                    StartGame();
                    _state = GameState.Countdown;
                    break;

                case GameState.Countdown:
                    if (!Countdown())
                        _state = GameState.GameRunning;

                    break;

                case GameState.GameRunning:
                    OnRunning();
                    UpdatePlayers();
                    SetPvP(ForcePvP);

                    if (HasFinished() || InsufficientPlayers())
                        _state = GameState.Finished;

                    break;

                case GameState.Finished:
                    if (InsufficientPlayers()) {
                        OnFailedFinished();
                    } else {
                        OnFinished();
                    }
                    //SetPvP(false);
                    _state = GameState.Cleanup;
                    break;

                case GameState.Cleanup:
                    OnCleanup();

                    foreach (var player in Players) {
                        player.SetGamemode(default);
                        player.SetTeam(0);
                        player.SpawnOnOldPosition();
                        player.ClearInterface();
                    }
                    ServerApi.Hooks.GameUpdate.Deregister(Main.Instance, OnGameUpdate);
                    DataHandler.OnPlayerGetData -= HandlePlayerData;
                    ActiveArena.Available = true;

                    return;
            }
        }

        /// <summary>
        /// Runs once when a person starts the gamemode with command.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Runs before a game. Used to alert players to get them into the game.
        /// </summary>
        /// <returns>True if the game is still in pregame</returns>
        public abstract bool PreGame();

        /// <summary>
        /// Returns true if there are adequete players to start the gamemode.
        /// </summary>
        public abstract bool HasVoteSucceeded();

        /// <summary>
        /// Runs when a game has failed to start.
        /// </summary>
        public abstract void FailStart();

        /// <summary>
        /// Returns true if there are insufficient players for the minigame.
        /// </summary>
        /// <returns></returns>
        public abstract bool InsufficientPlayers();

        /// <summary>
        /// Runs when a game has started
        /// </summary>
        public abstract void StartGame();

        /// <summary>
        /// Starts a countdown before the game starts.
        /// </summary>
        /// <returns>True if the countdown is running</returns>
        public abstract bool Countdown();

        /// <summary>
        /// Called when the game is running.
        /// </summary>
        public abstract void OnRunning();

        /// <summary>
        /// Method to determine if a game has finished
        /// </summary>
        /// <returns>True if the game has finished</returns>
        public abstract bool HasFinished();

        /// <summary>
        /// Method is called after the game is finished.
        /// </summary>
        public abstract void OnFinished();

        /// <summary>
        /// Method is called when there are insufficient players to continue the game.
        /// </summary>
        public abstract void OnFailedFinished();

        /// <summary>
        /// Method is called after the game is finished.
        /// </summary>
        public virtual void OnCleanup() { }

        public virtual void OnPlayerData(TerrariaPacket e) { }

        private void HandlePlayerData(object sender, TerrariaPacket e) {
            if (!Players.Contains(e.Player)) return;

            OnPlayerData(e);

            var spawn = e as PlayerSpawnArgs;
            if (spawn != null && _state == GameState.GameRunning) {
                spawn.Player.SpawnOnSpawnPoint();
            }
        }

        /// <summary>
        /// Sets players's pvp status in the current minigame.
        /// </summary>
        public virtual void SetPvP(bool on) {
            for (int index = 0; index < Players.Count; index++) {
                TSPlayer player = Players[index];
                if (player.TPlayer.hostile != on) {
                    player.TPlayer.hostile = on;
                    NetMessage.SendData((int)PacketTypes.TogglePvp, -1, -1, null, player.Index);
                }
            }
        }

        public virtual void UpdatePlayers() {
            for (int index = 0; index < Players.Count; index++) {
                TSPlayer player = Players[index];
                if (player == null || !player.ConnectionAlive || !player.Active || player.GetGamemode() != GamemodeName) {
                    Players.RemoveAt(index);
                    index--;
                    continue;
                }
            }
        }

        internal void SetGameState(GameState state) {
            _state = state;
        }

        internal void SendMessageToAllPlayers(string message, Color color) {
            foreach (var player in Players) {
                player.SendMessage(message, color);
            }
        }

        internal void SendMessageToAllPlayers(string message) {
            foreach (var player in Players) {
                player.SendMessage(message, Color.Yellow);
            }
        }

        internal GameState GetGameState() {
            return _state;
        }

        public void SendPacketsToAllPlayers(byte[] packet) {
            foreach (var player in Players) {
                player.SendRawData(packet);
            }
        }
    }

    internal enum GameState {
        Initializing,
        PreGame,
        StartGame,
        Countdown,
        GameRunning,
        Finished,
        Cleanup
    }
}
