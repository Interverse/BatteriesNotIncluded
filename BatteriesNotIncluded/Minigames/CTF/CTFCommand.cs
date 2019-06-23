using BatteriesNotIncluded.Framework;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace BatteriesNotIncluded.Minigames.CTF {
    public class CTFCommand : ICommand {
        private static string InvalidSyntax = "Invalid Syntax. ";
        private static string Syntax = "/ctf <start/join>";

        public IEnumerable<Command> GetCommands() {
            yield return new Command("bni.ctf", CTF, "ctf");
        }

        public static void CTF(CommandArgs args) {
            var player = args.Player;
            var input = args.Parameters;

            if (input.Count < 1) {
                player.SendErrorMessage(InvalidSyntax + Syntax);
                return;
            }

            switch (input[0].ToLower()) {
                case "join":
                    if (Main.ActiveVote != null && Main.ActiveVote.GetType() == typeof(CTF)) {
                        if (!Main.ActiveVote.Players.Contains(player)) {
                            player.SendSuccessMessage("You've been added to CTF!");
                            Main.ActiveVote.AddPlayer(player);
                        } else {
                            player.SendErrorMessage("You're already in CTF!");
                        }
                        
                    } else {
                        player.SendErrorMessage("No active CTF vote.");
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
                        arenas = Main.ArenaManager.GetAvailableArenas<CTFArena>(arenaName);
                        if (arenas.Count() == 0) {
                            args.Player.SendErrorMessage("CTF arena not found or not available.");
                            return;
                        }
                    } else {
                        arenas = Main.ArenaManager.GetAvailableArenas<CTFArena>();
                        if (arenas.Count() == 0) {
                            args.Player.SendErrorMessage("All CTF arenas are occupied.");
                            return;
                        }
                    }

                    Main.ActiveVote = new CTF(arenas.SelectRandom());
                    Main.ActiveVote.AddPlayer(player);
                    break;

                default:
                    player.SendErrorMessage(InvalidSyntax + Syntax);
                    break;
            }
        }
    }
}
