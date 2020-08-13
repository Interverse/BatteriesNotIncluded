using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace BatteriesNotIncluded.Minigames.Doubles {
    public static class TSPlayerDoublesExtension {
        public static TSPlayer GetDoublesTeammate(this TSPlayer player) {
            return player.GetData<TSPlayer>("DoublesTeammate");
        } 

        public static bool HasDoublesTeammate(this TSPlayer player) {
            return player.GetData<TSPlayer>("DoublesTeammate") != default(TSPlayer);
        }

        public static void SetPendingDoubles(this TSPlayer player, TSPlayer opponent) {
            player.SetData("DoublesPlayer", opponent);
        }

        public static void SetPendingTeamInvite(this TSPlayer player, TSPlayer opponent) {
            player.SetData("PendingDoublesTeammate", opponent);
        }

        public static bool IsPendingTeamInvite(this TSPlayer player) {
            return player.GetData<TSPlayer>("PendingDoublesTeammate") != default(TSPlayer);
        }

        public static void RemoveTeammate(this TSPlayer player) {
            player.GetDoublesTeammate().SetData<TSPlayer>("DoublesTeammate", default);
            player.GetDoublesTeammate().SetData<TSPlayer>("PendingDoublesTeammate", default);

            player.SetData<TSPlayer>("DoublesTeammate", default);
            player.SetData<TSPlayer>("PendingDoublesTeammate", default);
        }

        public static void SetDoublesTeammate(this TSPlayer player, TSPlayer teammate) {
            player.SetData<TSPlayer>("PendingDoublesTeammate", default);
            teammate.SetData<TSPlayer>("PendingDoublesTeammate", default);

            player.SetData("DoublesTeammate", teammate);
            teammate.SetData("DoublesTeammate", player);
        }

        public static bool IsPendingDoubles(this TSPlayer player) {
            return player.GetData<TSPlayer>("DoublesPlayer") != default(TSPlayer);
        }

        public static TSPlayer GetPendingDoublesPlayer(this TSPlayer player) {
            return player.GetData<TSPlayer>("DoublesPlayer");
        }

        public static void SetDoublesAccept(this TSPlayer player, bool accepted) {
            player.SetData("DoublesAccept", accepted);
        }

        public static bool GetDoublesAccept(this TSPlayer player) {
            return player.GetData<bool>("DoublesAccept");
        }

        public static void SetDoublesDead(this TSPlayer player, bool dead) {
            player.SetData("DoublesDead", dead);
        }

        public static bool GetDoublesDead(this TSPlayer player) {
            return player.GetData<bool>("DoublesDead");
        }

        public static bool IsTeamDead(this TSPlayer player) {
            return player.GetDoublesDead() && player.GetDoublesTeammate().GetDoublesDead();
        }

        public static void ResetTeamDead(this TSPlayer player) {
            player.SetDoublesDead(false);
            player.GetDoublesTeammate().SetDoublesDead(false);
        }
    }
}
