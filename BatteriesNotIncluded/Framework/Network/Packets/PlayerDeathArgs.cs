using System;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Network.Packets {
    public class PlayerDeathArgs : TerrariaPacket {
        public PlayerDeathArgs(TSPlayer player) {
            Player = player;
        }
    }
}
