using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace BatteriesNotIncluded.Minigames.Duel {
    public static class TSPlayerDuelExtension {
        public static void SetPendingDuel(this TSPlayer player, TSPlayer opponent) {
            player.SetData("DuelPlayer", opponent);
        }

        public static bool IsPendingDuel(this TSPlayer player) {
            return player.GetData<TSPlayer>("DuelPlayer") != default(TSPlayer);
        }

        public static TSPlayer GetPendingDuelPlayer(this TSPlayer player) {
            return player.GetData<TSPlayer>("DuelPlayer");
        }

        public static void SetDuelAccept(this TSPlayer player, bool accepted) {
            player.SetData("DuelAccept", accepted);
        }

        public static bool GetDuelAccept(this TSPlayer player) {
            return player.GetData<bool>("DuelAccept");
        }
    }
}
