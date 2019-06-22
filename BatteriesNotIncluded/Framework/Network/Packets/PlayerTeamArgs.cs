using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Network.Packets {
    public class PlayerTeamArgs : TerrariaPacket {
        public int Team;

        public PlayerTeamArgs(MemoryStream data, TSPlayer player) : base(player) {
            Team = data.ReadByte();
        }
    }
}
