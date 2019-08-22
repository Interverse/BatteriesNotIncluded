using BatteriesNotIncluded.Framework;
using BatteriesNotIncluded.Framework.Commands;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;

namespace BatteriesNotIncluded.Minigames.Duel {
    public class DuelCommand : ICommand {
        public static string InvalidSyntax = "Invalid Syntax. ";
        public static string Syntax = "/duel <name> <optional: arena name>";

        public static void Duel(CommandArgs args) {
            var player = args.Player;
            var input = args.Parameters;

            if (input.Count < 1) {
                player.SendErrorMessage(InvalidSyntax + Syntax);
                return;
            }

            // If accepting duel
            if (input[0].ToLower() == "accept") {
                if (player.IsPendingDuel()) {
                    player.SetDuelAccept(true);
                } else {
                    player.SendMessage("No pending duels.", Color.Yellow);
                }
                return;
            }

            // If challenging someone
            var playersFound = TShock.Utils.FindPlayer(input[0]);

            if (playersFound.Count > 1) {
                args.Player.SendMessage("Multiple players found: ", Color.Yellow);
                foreach (var foundPlayer in playersFound) {
                    args.Player.SendMessage(foundPlayer.Name, Color.Yellow);
                }
                return;
            } else if (playersFound.Count == 0) {
                args.Player.SendErrorMessage("No players found!");
                return;
            }

            var opponent = TShock.Players[playersFound[0].Index];


            if (player.Name == opponent.Name) {
                player.SendErrorMessage("Cannot duel yourself!");
                return;
            }

            IEnumerable<Arena> arenas;
            if (input.Count >= 2) {
                string arenaName = input[1];
                arenas = Main.ArenaManager.GetAvailableArenas("Duel", arenaName);
                if (arenas.Count() == 0) {
                    args.Player.SendErrorMessage("Duel arena not found.");
                    return;
                }
            } else {
                arenas = Main.ArenaManager.GetAvailableArenas("Duel");
                if (arenas.Count() == 0) {
                    args.Player.SendErrorMessage("All duel arenas are occupied.");
                    return;
                }
            }

            opponent.SetPendingDuel(player);
            new Duel(player, opponent, arenas.SelectRandom() as DuelArena);
        }

        public IEnumerable<Command> GetCommands() {
            yield return new Command("bni.duel", Duel, "duel");
        }
    }
}
