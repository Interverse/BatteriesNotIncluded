using BatteriesNotIncluded.Framework.Extensions;
using BatteriesNotIncluded.Framework.Network;
using BatteriesNotIncluded.Framework.Network.Packets;
using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Commands {
    public class SetupArena : ICommand {
        public static string InvalidSyntax = "Invalid Syntax. ";
        public static string CreateHelp = "/arena create <arena type> <arena name>";
        public static string DeleteHelp = "/arena delete <arena name>";
        public static string ListHelp = "/arena list <arena type>";
        public static string ArenaHelp = InvalidSyntax + "\n" + CreateHelp + "\n" + DeleteHelp + "\n" + ListHelp;
        public static List<Arena> ArenaTypes = null;

        // Permissions: bni.arena and bni.arenacreate
        public IEnumerable<Command> GetCommands() {
            ArenaTypes = MiscUtils.InstantiateClassesOfAbstract<Arena>();
            DataHandler.OnPlayerGetData += GetBlockPoints;
            yield return new Command("bni.arena", CreateArena, "arena");
        }

        private void GetBlockPoints(object sender, EventArgs e) {
            ModifyTilesArgs tileUpdate = e as ModifyTilesArgs;
            if (tileUpdate == null) return;
            if (tileUpdate.Player.GetGamemode() != "Edit") return;

            var player = tileUpdate.Player;

            player.SetCurrentVector(new Vector2(tileUpdate.TileX * 16 - 7, tileUpdate.TileY * 16 - 32));
            NetMessage.SendTileSquare(player.Index, tileUpdate.TileX, tileUpdate.TileY, 1);

            string nextVector = player.GetNextPendingVector();

            if (nextVector != default) {
                player.SendInfoMessage($"Hit a block to set point '{nextVector}'");
            } else {
                player.SendSuccessMessage($"Successfully created a {player.GetArenaEdit().Type} arena with name '{player.GetArenaEdit().Name}'");
                player.GetArenaEdit().InsertArena();
                player.SetGamemode(default);
                player.SetArenaEdit(default);
                player.SetPendingVectors(default);
            }
        }

        private void CreateArena(CommandArgs args) {
            var player = args.Player;
            var input = args.Parameters;

            if (input.Count < 1) {
                player.SendErrorMessage(InvalidSyntax + ArenaHelp);
                return;
            }

            string arenaType;
            Arena arena;

            switch (input[0].ToLower()) {
                case "create":
                    if (!player.HasPermission("bni.arenacreate")) {
                        player.SendErrorMessage("You do not have permission to create an arena!");
                        return;
                    }

                    if (input.Count < 3) {
                        player.SendErrorMessage(InvalidSyntax + CreateHelp);
                        return;
                    }

                    arenaType = input[1];
                    arena = ArenaTypes.FirstOrDefault(c => c.Type.ToLower() == arenaType.ToLower());

                    if (arena == null) {
                        player.SendInfoMessage("Gamemode does not exist.");
                        player.SendInfoMessage("Possible arena types: ");
                        List<string> arenaTypes = new List<string>();
                        foreach (var types in ArenaTypes) {
                            arenaTypes.Add(types.Type);
                        }
                        player.SendInfoMessage(string.Join(", ", arenaTypes));
                        return;
                    }

                    string arenaName = input[2];

                    if (string.IsNullOrWhiteSpace(arenaName)) {
                        player.SendErrorMessage("Arena name cannot be empty.");
                        return;
                    }

                    if (Main.ArenaManager.GetArena(arenaName) != null) {
                        player.SendErrorMessage($"Arena aready exists for name {arenaName}");
                        return;
                    }

                    player.SendMessage($"Creating {arenaType.FirstCharToUpper()} arena with name '{arenaName}'", Color.Cyan);
                    player.SendMessage($"Type '/arena cancel' to cancel editing", Color.Cyan);
                    player.SetGamemode("Edit");
                    player.SetArenaEdit((Arena)Activator.CreateInstance(arena.GetType()));
                    player.GetArenaEdit().SetValue("Name", arenaName);
                    player.SetPendingVectors(arena.GetVariableNamesOfType<Vector2>());
                    player.SendInfoMessage($"Hit a block to set point '{player.GetNextPendingVector()}'");

                    break;

                case "delete":
                    if (!player.HasPermission("bni.arenacreate")) {
                        player.SendErrorMessage("You do not have permission to delete arenas!");
                        return;
                    }

                    if (input.Count < 2) {
                        player.SendErrorMessage(InvalidSyntax + DeleteHelp);
                        return;
                    }

                    arenaName = input[1];

                    arena = Main.ArenaManager.GetArena(arenaName);

                    if (arena == null) {
                        player.SendErrorMessage($"Arena {arenaName} not found. (Arena names are case sensitive)");
                        return;
                    }

                    Main.ArenaDatabase.DeleteRow(arena.Type, arena.Name);
                    Main.ArenaManager.LoadArenas();
                    player.SendSuccessMessage($"Delete {arena.Type} arena '{arena.Name}'");

                    break;

                case "list":
                    if (input.Count < 2) {
                        player.SendInfoMessage(InvalidSyntax + ListHelp);
                        player.SendInfoMessage("Possible arena types: ");

                        List<string> arenaTypes = new List<string>();
                        foreach (var types in ArenaTypes) {
                            arenaTypes.Add(types.Type);
                        }
                        player.SendInfoMessage(string.Join(", ", arenaTypes));
                    } else {
                        arenaType = input[1];
                        arena = ArenaTypes.FirstOrDefault(c => c.Type.ToLower() == arenaType);

                        if (arena == null) {
                            player.SendInfoMessage("Gamemode does not exist.");
                            player.SendInfoMessage("Possible arena types: ");

                            List<string> arenaTypes = new List<string>();
                            foreach (var types in ArenaTypes) {
                                arenaTypes.Add(types.Type);
                            }
                            player.SendInfoMessage(string.Join(", ", arenaTypes));
                            return;
                        }

                        var arenasFound = Main.ArenaManager.GetArenas(arena.GetType());

                        if (arenasFound.Count() > 0) {
                            player.SendInfoMessage($"{arenaType.FirstCharToUpper()} Arenas: ");

                            List<string> arenas = new List<string>();
                            foreach (var a in arenasFound) {
                                arenas.Add(a.Name);
                            }
                            player.SendInfoMessage(string.Join(", ", arenas));
                        } else {
                            player.SendInfoMessage($"No arenas exist for {arenaType.FirstCharToUpper()}");
                        }
                    }
                    break;

                case "cancel":
                    if (player.GetArenaEdit() != null) {
                        player.SendInfoMessage("Cancelled arena creation.");
                        player.SetGamemode(default);
                        player.SetArenaEdit(default);
                        player.SetPendingVectors(default);
                    } else {
                        player.SendErrorMessage("You are not currently creating any arenas.");
                    }
                    
                    break;

                default:
                    player.SendErrorMessage(ArenaHelp);
                    break;
            }
        }
    }

    internal static class TSPlayerEditExtensions {
        internal static void SetArenaEdit(this TSPlayer player, Arena arena) {
            player.SetData("Arena", arena);
        }

        internal static Arena GetArenaEdit(this TSPlayer player) {
            return player.GetData<Arena>("Arena");
        }

        internal static void SetPendingVectors(this TSPlayer player, IEnumerable<string> vectors) {
            Queue vectorQueue = new Queue();
            if (vectors != null) {
                foreach (string vectorName in vectors) {
                    vectorQueue.Enqueue(vectorName);
                }
            }
            player.SetData("PendingVectors", vectorQueue);
        }

        internal static void SetCurrentVector(this TSPlayer player, Vector2 vector) {
            player.GetArenaEdit().SetValue(player.GetCurrentVector(), vector);
        }

        internal static string GetCurrentVector(this TSPlayer player) {
            return player.GetData<string>("CurrentVector");
        }

        internal static string GetNextPendingVector(this TSPlayer player) {
            if (player.GetData<Queue>("PendingVectors").Count < 1) {
                player.SetData<string>("CurrentVector", default);
                return default;
            }
            string currentVector = (string)player.GetData<Queue>("PendingVectors").Dequeue();
            player.SetData("CurrentVector", currentVector);
            return currentVector;
        }
    }
}
