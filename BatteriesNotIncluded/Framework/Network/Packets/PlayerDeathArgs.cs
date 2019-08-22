using System;
using System.IO;
using System.IO.Streams;
using Terraria.DataStructures;
using TerrariaApi.Server;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Network.Packets {
    public class PlayerDeathArgs : TerrariaPacket {

        public int PlayerID;
        public PlayerDeathReason PlayerDeathReason;
        public int Damage;
        public byte HitDirection;
        public byte Flags;
        public sbyte CooldownCounter;

        public PlayerDeathArgs(GetDataEventArgs args, MemoryStream data, TSPlayer player) : base(player) {
            data.ReadByte(); // Player ID
            PlayerDeathReason = PlayerDeathReason.FromReader(new BinaryReader(data));
            Damage = data.ReadInt16();
            HitDirection = (byte)data.ReadByte();
            Flags = (byte)data.ReadByte();
        }
    }
}
