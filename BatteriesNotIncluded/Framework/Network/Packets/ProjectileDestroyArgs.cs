using System;
using System.IO;
using System.IO.Streams;

namespace BatteriesNotIncluded.Framework.Network.Packets {
    public class ProjectileDestroyArgs : TerrariaPacket {
        public int ProjectileIndex;
        public int Owner;

        public ProjectileDestroyArgs(MemoryStream data) {
            ProjectileIndex = data.ReadInt16();
            Owner = data.ReadByte();
        }
    }
}
