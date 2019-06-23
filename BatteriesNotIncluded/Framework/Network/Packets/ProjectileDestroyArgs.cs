using System;
using System.IO;
using System.IO.Streams;
using Terraria;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Network.Packets {
    public class ProjectileDestroyArgs : TerrariaPacket {
        public int ProjectileIndex;
        public int Owner;
        public Projectile ProjectileDestroyed;
        public float ProjectileX;
        public float ProjectileY;

        public ProjectileDestroyArgs(MemoryStream data, TSPlayer player) : base(player) {
            lock (Terraria.Main.projectile) {
                ProjectileIndex = data.ReadInt16();
                Owner = data.ReadByte();

                ProjectileDestroyed = Terraria.Main.projectile[ProjectileIndex];
                ProjectileX = ProjectileDestroyed.Center.X;
                ProjectileY = ProjectileDestroyed.Center.Y;
            }
        }
    }
}
