using System;
using System.IO;
using System.IO.Streams;
using Microsoft.Xna.Framework;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Network.Packets {
    public class ProjectileNewArgs : EventArgs {
        
        public GetDataEventArgs Args;

        public int Identity;
        public Vector2 Position;
        public Vector2 Velocity;
        public Single Knockback;
        public int Damage;
        public int Owner;
        public int Type;
        public BitsByte AiFlags;
        public float Ai0;
        public float Ai1;
        public float[] Ai;

        public ProjectileNewArgs(GetDataEventArgs args, MemoryStream data) {
            Args = args;

            Identity = data.ReadInt16();
            Position = new Vector2(data.ReadSingle(), data.ReadSingle());
            Velocity = new Vector2(data.ReadSingle(), data.ReadSingle());
            Knockback = data.ReadSingle();
            Damage = data.ReadInt16();
            Owner = data.ReadByte();
            Type = data.ReadInt16();
            AiFlags = (BitsByte)data.ReadByte();
            Ai0 = AiFlags[0] ? Ai0 = data.ReadSingle() : 0;
            Ai1 = AiFlags[1] ? data.ReadSingle() : 0;

            Ai = new float[Projectile.maxAI];
        }
    }
}
