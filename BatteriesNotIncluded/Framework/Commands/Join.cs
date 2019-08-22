using System.Collections.Generic;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Commands {
    public class Join : ICommand {
        public IEnumerable<Command> GetCommands() {
            yield return new Command("bni.join", JoinCommand, "join");
        }

        private void JoinCommand(CommandArgs args) {
            var player = args.Player;

            if (Main.ActiveVote != null) {
                if (!Main.ActiveVote.Players.Contains(player)) {
                    player.SendSuccessMessage($"You've been added to {Main.ActiveVote.GamemodeName}!");
                    Main.ActiveVote.AddPlayer(player);
                } else {
                    player.SendErrorMessage($"You're already in {Main.ActiveVote.GamemodeName}!");
                }
            } else {
                player.SendErrorMessage("No active gamemode vote.");
            }
        }
    }
}
