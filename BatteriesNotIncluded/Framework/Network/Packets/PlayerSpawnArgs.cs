using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Streams;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Network.Packets {
    public class PlayerSpawnArgs : TerrariaPacket {
        public int SpawnX;
        public int SpawnY;

        public PlayerSpawnArgs(MemoryStream data, TSPlayer player) : base(player) {
            SpawnX = data.ReadInt16();
            SpawnY = data.ReadInt16();
        }
    }
}
