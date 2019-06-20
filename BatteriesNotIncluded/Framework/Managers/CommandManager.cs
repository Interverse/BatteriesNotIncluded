using BatteriesNotIncluded.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

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
