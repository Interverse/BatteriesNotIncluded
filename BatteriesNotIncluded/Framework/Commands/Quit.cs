using BatteriesNotIncluded.Framework.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Commands {
    public class Quit : ICommand {
        public IEnumerable<Command> GetCommands() {
            yield return new Command(QuitGamemode, "quit");
        }

        private void QuitGamemode(CommandArgs args) {
            TSPlayer player = args.Player;

            if (player.GetGamemode() == default || player.GetGamemode() == "Edit") {
                player.SendMessage("You are currently not in a game.", Color.Cyan);
                return;
            }

            player.SendMessage($"You have quit {player.GetGamemode()}", Color.Cyan);
            player.SetGamemode(default);

            player.SpawnOnOldPosition();
            player.ClearInterface();
        }
    }
}
