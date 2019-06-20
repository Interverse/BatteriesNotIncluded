using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Streams;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Network.Packets {
    public class ModifyTilesArgs : EventArgs {
        public TSPlayer Player;

        public byte Action;
        public int TileX;
        public int TileY;
        /// <summary>
        /// KillTile (Fail: Bool), PlaceTile (Type: Byte), 
        /// KillWall (Fail: Bool), PlaceWall (Type: Byte), 
        /// KillTileNoItem (Fail: Bool), SlopeTile (Slope: Byte)
        /// </summary>
        public int Var1;

        /// <summary>
        /// Var2: PlaceTile(Style: Byte)
        /// </summary>
        public int Var2;

        public ModifyTilesArgs(MemoryStream data, TSPlayer player) {
            Player = player;

            Action = (byte)data.ReadByte();
            TileX = data.ReadInt16();
            TileY = data.ReadInt16();
            Var1 = data.ReadInt16();
            Var2 = data.ReadByte();
        }
    }
}
