using System;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Network.Packets {
    public class PlayerDeathArgs : EventArgs {
        public TSPlayer Player;

        public PlayerDeathArgs(TSPlayer player) {
            Player = player;
        }
    }
}
