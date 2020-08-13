using BatteriesNotIncluded.Framework;
using BatteriesNotIncluded.Framework.Commands;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;

namespace BatteriesNotIncluded.Minigames.Doubles {
    public class DoublesCommand : ICommand {
        public static string InvalidSyntax = "Invalid Syntax. ";
        public static string Syntax = "/doubles <teammate> <opponent1> <opponent2>\n" +
            "If with teammate: /doubles <opponent1> <opponent2>\n" +
            "If opponent has teammate: /doubles <opponent1>";

        public static void Doubles(CommandArgs args) {
            var player = args.Player;
            TSPlayer teammate, opponent1, opponent2;
            var input = args.Parameters;

            if (input.Count == 0) {
                player.SendErrorMessage(InvalidSyntax + Syntax);
                return;
            }

            // If accepting doubles
            if (input[0].ToLower() == "accept") {
                if (player.IsPendingDoubles()) {
                    player.SetDoublesAccept(true);
                    player.SendInfoMessage("You have accepted the pending doubles duel.");
                } else {
                    player.SendMessage("No pending doubles.", Color.Yellow);
                }
                return;
            }

            // If the player calling the command has a teammate
            if (player.HasDoublesTeammate()) {
                // Set player teammate
                teammate = player.GetDoublesTeammate();

                // If the server can't find the player from a given name, send error message
                if (!MiscUtils.FindPlayer(player, input[0], out opponent1)) {
                    player.SendErrorMessage($"Cannot find player of {input[0]}. Syntax: " +
                        $"If with teammate: /doubles <opponent1> <opponent2>\n" +
                        "If opponent has teammate: /doubles <opponent1>");

                    return;
                } else {
                    // If opponent has a teammate
                    if (opponent1.HasDoublesTeammate()) {
                        // Automatically set their teammate as second opponent
                        opponent2 = opponent1.GetDoublesTeammate();
                    } else {
                        // If opponent does not have a teammate, find player from second parameter
                        if (!MiscUtils.FindPlayer(player, input[1], out opponent2)) {
                            player.SendErrorMessage($"Cannot find second player of {input[1]}. Syntax: " +
                                $"/doubles <opponent1> <opponent2>\n");

                            return;
                        }
                    }
                }
            } else { // If player does not have a teammate

                if (!MiscUtils.FindPlayer(player, input[0], out teammate)) {
                    player.SendErrorMessage($"Cannot find player of {input[0]}. " + Syntax);
                    return;
                }

                if (!MiscUtils.FindPlayer(player, input[1], out opponent1)) {
                    player.SendErrorMessage($"Cannot find opponent of {input[1]}. " + Syntax);
                    return;
                }

                if (opponent1.HasDoublesTeammate()) {
                    opponent2 = opponent1.GetDoublesTeammate();
                } else {
                    if (!MiscUtils.FindPlayer(player, input[2], out opponent2)) {
                        player.SendErrorMessage($"Cannot find second opponent of {input[2]}. " + Syntax);
                        return;
                    }
                }
            }

            IEnumerable<Arena> arenas;
            arenas = Main.ArenaManager.GetAvailableArenas("Doubles");
            if (arenas.Count() == 0) {
                args.Player.SendErrorMessage("All doubles arenas are occupied.");
                return;
            }

            teammate.SetPendingDoubles(player);
            opponent1.SetPendingDoubles(player);
            opponent2.SetPendingDoubles(player);
            new Doubles(player, teammate, opponent1, opponent2, arenas.SelectRandom() as DoublesArena);
        }

        private void Team(CommandArgs args) {
            var input = args.Parameters;

            if (input.Count < 1) {
                args.Player.SendErrorMessage(InvalidSyntax + "/team <teammate>. To leave a team: /team leave");
                return;
            }

            // Leaving a team
            if (input[0].ToLower() == "leave") {
                if (args.Player.HasDoublesTeammate()) {
                    var teammate = args.Player.GetDoublesTeammate();
                    args.Player.RemoveTeammate();

                    args.Player.SendInfoMessage($"{teammate.Name} was removed as a doubles teammate.");
                    teammate.SendInfoMessage($"{args.Player.Name} disbanded the doubles team.");
                } else {
                    args.Player.SendMessage("No doubles team to leave.", Color.Yellow);
                }
                return;
            }

            // If accepting teammate request
            if (input[0].ToLower() == "accept") {
                if (args.Player.IsPendingTeamInvite()) {
                    args.Player.SetDoublesTeammate(args.Player.GetPendingDoublesPlayer());
                    args.Player.SendSuccessMessage($"You have teamed up with {args.Player.GetDoublesTeammate().Name}.");
                } else {
                    args.Player.SendMessage("No pending doubles teammate invite.", Color.Yellow);
                }
                return;
            }

            if (!MiscUtils.FindPlayer(args.Player, input[0], out var opponent)) return;

            args.Player.SendMessage($"You have teamed up with {opponent.Name}.", Color.Yellow);
            opponent.SendMessage($"You have received a doubles team request from {args.Player.Name}. Type '/team accept' to accept.", Color.Yellow);
            opponent.SetPendingTeamInvite(args.Player);
        }

        public IEnumerable<Command> GetCommands() {
            yield return new Command("bni.doubles", Doubles, "doubles", "duos");
            yield return new Command("bni.doubles", Team, "team");
        }
    }
}
