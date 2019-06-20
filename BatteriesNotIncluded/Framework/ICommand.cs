using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace BatteriesNotIncluded.Framework {
    interface ICommand {
        IEnumerable<Command> GetCommands();
    }
}
