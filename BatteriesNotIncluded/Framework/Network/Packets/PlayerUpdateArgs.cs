using System;
using System.IO;
using System.IO.Streams;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Network.Packets {
    public class PlayerUpdateArgs : EventArgs {
        public TSPlayer Player;

        public int PlayerAction;
        public int Pulley;
        public int SelectedSlot;
        public float PositionX;
        public float PositionY;
        public float VelocityX;
        public float VelocityY;

        public PlayerUpdateArgs(MemoryStream data, TSPlayer player) {
            data.ReadByte();

            Player = player;
            PlayerAction = data.ReadByte();
            Pulley = data.ReadByte();
            SelectedSlot = data.ReadByte();
            PositionX = data.ReadSingle();
            PositionY = data.ReadSingle();

            if ((Pulley & 4) == 4) {
                VelocityX = data.ReadSingle();
                VelocityY = data.ReadSingle();
            }
        }
    }
}
