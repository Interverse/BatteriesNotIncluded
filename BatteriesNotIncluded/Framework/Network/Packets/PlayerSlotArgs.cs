using System;
using System.IO;
using System.IO.Streams;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Network.Packets {
    public class PlayerSlotArgs : EventArgs {
        public TSPlayer Player;
        public byte SlotId;
        public short Stack;
        public byte Prefix;
        public short NetID;

        public PlayerSlotArgs(MemoryStream data, TSPlayer player) {
            data.ReadByte(); //Passes through the PlayerID data

            Player = player;
            SlotId = (byte)data.ReadByte();
            Stack = data.ReadInt16();
            Prefix = (byte)data.ReadByte();
            NetID = data.ReadInt16();
        }
    }
}
