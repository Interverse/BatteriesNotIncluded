using Microsoft.Xna.Framework;
using System;
using System.Text;
using Terraria;
using Terraria.Localization;
using TShockAPI;

namespace BatteriesNotIncluded.Framework {
    public static class SidebarInterface {
        /// <summary>
        /// Displays a string to the side the player's screen
        /// </summary>
        /// <param Name="player"></param>
        public static void DisplayInterface(this TSPlayer player, string header = "", string body = "") {
            if (!Main.Config.EnableScorebar) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(MiscUtils.LineBreaks(8));
            sb.AppendLine(header);
            sb.AppendLine(new string('-', 40));
            sb.AppendLine(body);
            sb.AppendLine(new string('-', 40));
            sb.AppendLine(MiscUtils.LineBreaks(50));

            player.SendData(PacketTypes.Status, sb.ToString());
        }

        /// <summary>
        /// Sends a empty string to clear the player interface on the right side of the screen.
        /// </summary>
        /// <param Name="player"></param>
        public static void ClearInterface(this TSPlayer player) {
            player.SendData(PacketTypes.Status, String.Empty);
        }

        /// <summary>
        /// Brings a brief text pop-up above a person displaying a message.
        /// </summary>
        /// <param Name="player"></param>
        /// <param Name="message"></param>
        /// <param Name="color"></param>
        public static void PlayerTextPopup(this TSPlayer player, string message, Color color) {
            NetMessage.SendData(119, player.Index, -1, NetworkText.FromLiteral(message), (int)color.PackedValue, player.X, player.Y + 10);
        }
    }
}
