using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Extensions {
    public static class TSPlayerExtension {
        public static void SetGamemode(this TSPlayer player, string mode) {
            player.SetData("Gamemode", mode);
        }

        public static string GetGamemode(this TSPlayer player) {
            return player.GetData<string>("Gamemode");
        }

        public static void SetGamemodeSpawnPoint(this TSPlayer player, Vector2 spawn) {
            player.SetData("GamemodeSpawnPoint", spawn - new Vector2(7, 32));
        }

        public static Vector2 GetGamemodeSpawnPoint(this TSPlayer player) {
            return player.GetData<Vector2>("GamemodeSpawnPoint");
        }

        public static void SetOldPosition(this TSPlayer player, Vector2 pos) {
            player.SetData("OldPosition", pos);
        }

        public static Vector2 GetOldPosition(this TSPlayer player) {
            return player.GetData<Vector2>("OldPosition");
        }

        public static void SpawnOnSpawnPoint(this TSPlayer player) {
            player.Teleport(player.GetGamemodeSpawnPoint().X, player.GetGamemodeSpawnPoint().Y);
        }

        public static void SpawnOnOldPosition(this TSPlayer player) {
            player.Teleport(player.GetOldPosition().X, player.GetOldPosition().Y);
        }
    }
}
