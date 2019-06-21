using System;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Network.Packets {
    public class ItemOwnerArgs : TerrariaPacket {
        public ItemOwnerArgs(TSPlayer player) {
            Player = player;
        }
    }
}
