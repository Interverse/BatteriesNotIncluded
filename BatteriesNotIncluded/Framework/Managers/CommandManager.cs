using BatteriesNotIncluded.Framework.Commands;

namespace BatteriesNotIncluded.Framework.Managers {
    public class CommandManager {
        public CommandManager() {
            var commandList = MiscUtils.InstantiateClassesOfInterface<ICommand>();

            foreach (var commands in commandList) {
                foreach (var command in commands.GetCommands()) {
                    TShockAPI.Commands.ChatCommands.Add(command);
                }
            }
        }
    }
}
