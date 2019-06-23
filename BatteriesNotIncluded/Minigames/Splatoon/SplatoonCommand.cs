using BatteriesNotIncluded.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace BatteriesNotIncluded.Minigames.Splatoon {
    public class SplatoonCommand : ICommand {
        private static string InvalidSyntax = "Invalid Syntax. ";
        private static string Syntax = "/splatoon <start/join>";

        public IEnumerable<Command> GetCommands() {
            yield return new Command("bni.splatoon", Splatoon, "splatoon");
        }

        public static void Splatoon(CommandArgs args) {
            var player = args.Player;
            var input = args.Parameters;

            if (input.Count < 1) {
                player.SendErrorMessage(InvalidSyntax + Syntax);
                return;
            }

            switch (input[0].ToLower()) {
                case "join":
                    if (Main.ActiveVote != null && Main.ActiveVote.GetType() == typeof(Splatoon)) {
                        if (!Main.ActiveVote.Players.Contains(player)) {
                            player.SendSuccessMessage("You've been added to Splatoon!");
                            Main.ActiveVote.AddPlayer(player);
                        } else {
                            player.SendErrorMessage("You're already in Splatoon!");
                        }
                    } else {
                        player.SendErrorMessage("No active Splatoon vote.");
                    }
                    break;

                case "start":
                    if (Main.ActiveVote != null) {
                        player.SendErrorMessage($"There is already an active vote. ({Main.ActiveVote.GamemodeName})");
                        return;
                    }

                    IEnumerable<Arena> arenas;
                    if (input.Count >= 2) {
                        string arenaName = input[1];
                        arenas = Main.ArenaManager.GetAvailableArenas<SplatoonArena>(arenaName);
                        if (arenas.Count() == 0) {
                            args.Player.SendErrorMessage("Splatoon arena not found or not available.");
                            return;
                        }
                    } else {
                        arenas = Main.ArenaManager.GetAvailableArenas<SplatoonArena>();
                        if (arenas.Count() == 0) {
                            args.Player.SendErrorMessage("All Splatoon arenas are occupied.");
                            return;
                        }
                    }

                    Main.ActiveVote = new Splatoon(arenas.SelectRandom());
                    Main.ActiveVote.AddPlayer(player);
                    break;

                default:
                    player.SendErrorMessage(InvalidSyntax + Syntax);
                    break;
            }
        }
    }
}
