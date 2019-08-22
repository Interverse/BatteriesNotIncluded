using System;
using System.IO;
using System.IO.Streams;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Network.Packets {
    public class TogglePvPArgs : TerrariaPacket {
        public bool Hostile;

        public TogglePvPArgs(MemoryStream data, TSPlayer player) : base(player) {
            Hostile = data.ReadBoolean();
        }
    }
}
