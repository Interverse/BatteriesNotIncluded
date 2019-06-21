using System;
using System.IO;
using System.IO.Streams;
using Terraria;
using Terraria.DataStructures;
using TerrariaApi.Server;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Network.Packets {
    public class PlayerHurtArgs : TerrariaPacket {
        public GetDataEventArgs Args;

        public int PlayerID;
        public PlayerDeathReason PlayerHitReason;
        public int Damage;
        public byte HitDirection;
        public byte Flags;
        public sbyte CooldownCounter;

        public PlayerHurtArgs(GetDataEventArgs args, MemoryStream data) {
            Args = args;
            PlayerID = data.ReadByte();
            PlayerHitReason = PlayerDeathReason.FromReader(new BinaryReader(data));
            Damage = data.ReadInt16();
            HitDirection = (byte)data.ReadByte();
            Flags = (byte)data.ReadByte();
            CooldownCounter = (sbyte)data.ReadByte();
        }
    }
}
