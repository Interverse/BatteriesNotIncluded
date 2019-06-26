using BatteriesNotIncluded.Framework.MinigameTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Commands {
    public class Start : ICommand {
        private static string InvalidSyntax = "Invalid Syntax. ";
        private static string Syntax = "/start <gamemode> <optional: arena name>";
        private static List<Type> GamemodeTypes = MiscUtils.GetTypesThatInheritAbstract<EndgameMinigame>();

        public IEnumerable<Command> GetCommands() {
            yield return new Command("bni.start", StartGamemode, "start");
        }

        public static void StartGamemode(CommandArgs args) {
            var player = args.Player;
            var input = args.Parameters;

            if (input.Count < 1) {
                player.SendErrorMessage(InvalidSyntax + Syntax);
                player.SendErrorMessage("Possible Gamemodes: " + string.Join(", ", GamemodeTypes.Select(name => name.StripNamespace())));
                return;
            }

            if (Main.ActiveVote != null) {
                player.SendErrorMessage($"There is already an active vote. ({Main.ActiveVote.GamemodeName})");
                return;
            }

            Type gamemodeType = GamemodeTypes.FirstOrDefault(c => c.StripNamespace().ToLower() == input[0].ToLower());

            if (gamemodeType == default) {
                player.SendErrorMessage($"Could not find gamemode of type {input[0]}.");
                return;
            }

            string gamemodeName = gamemodeType.StripNamespace();

            IEnumerable<Arena> arenas;
            if (input.Count >= 2) {
                string arenaName = input[1];
                arenas = Main.ArenaManager.GetAvailableArenas(gamemodeName, arenaName);
                if (arenas.Count() == 0) {
                    args.Player.SendErrorMessage($"{gamemodeName} arena not found or not available.");
                    return;
                }
            } else {
                arenas = Main.ArenaManager.GetAvailableArenas(gamemodeName);
                if (arenas.Count() == 0) {
                    args.Player.SendErrorMessage($"All {gamemodeName} arenas are occupied.");
                    return;
                }
            }

            Main.ActiveVote = (Minigame)Activator.CreateInstance(gamemodeType, arenas.SelectRandom());
            Main.ActiveVote.AddPlayer(player);
        }
    }
}
