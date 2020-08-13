using System;
using System.IO;
using System.IO.Streams;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Network.Packets {
    public class ProjectileNewArgs : TerrariaPacket {
        
        public GetDataEventArgs Args;

        public int Identity;
        public Vector2 Position;
        public Vector2 Velocity;
        public float Knockback;
        public int Damage;
        public int Owner;
        public int Type;
        public int ProjFlags;
        public float Ai0;
        public float Ai1;
        public float[] Ai;
        public int OriginalDamage;
        public int UUID;

        public ProjectileNewArgs(GetDataEventArgs args, MemoryStream data, TSPlayer player) : base(player) {
            Args = args;

            Identity = data.ReadInt16();
            Position = new Vector2(data.ReadSingle(), data.ReadSingle());
            Velocity = new Vector2(data.ReadSingle(), data.ReadSingle());
            Owner = data.ReadByte();
            Type = data.ReadInt16();
            ProjFlags = data.ReadByte();

            Ai0 = (ProjFlags & 1) == 1 ? data.ReadSingle() : 0;
            Ai1 = (ProjFlags & 2) == 2 ? data.ReadSingle() : 0;
            Damage = (ProjFlags & 16) == 16 ? data.ReadInt16() : 0;
            Knockback = (ProjFlags & 32) == 32 ? data.ReadSingle() : 0;
            OriginalDamage = (ProjFlags & 64) == 64 ? data.ReadInt16() : 0;
            UUID = (ProjFlags & 128) == 128 ? data.ReadInt16() : 0;

            Ai = new float[Projectile.maxAI];
        }
    }
}
