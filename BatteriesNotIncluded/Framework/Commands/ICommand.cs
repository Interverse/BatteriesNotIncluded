using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Commands {
    interface ICommand {
        IEnumerable<Command> GetCommands();
    }
}
