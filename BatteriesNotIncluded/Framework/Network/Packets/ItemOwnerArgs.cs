using System;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Network.Packets {
    public class ItemOwnerArgs : EventArgs {
        public TSPlayer Player;

        public ItemOwnerArgs(TSPlayer player) {
            Player = player;
        }
    }
}
