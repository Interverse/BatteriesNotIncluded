using BatteriesNotIncluded.Framework.Network.Packets;
using System;
using System.IO;
using TerrariaApi.Server;
using TShockAPI;

namespace BatteriesNotIncluded.Framework.Network {
    /// <summary>
    /// Creates hooks for plugins to use.
    /// </summary>
    public class DataHandler {
        public static event EventHandler<TerrariaPacket> OnPlayerGetData;

        public static void HandleData(GetDataEventArgs args, MemoryStream data, TSPlayer player) {
            switch (args.MsgID) {
                case PacketTypes.PlayerHurtV2:
                    OnPlayerGetData?.Invoke(typeof(DataHandler), new PlayerHurtArgs(args, data)); 
                    return;

                case PacketTypes.TogglePvp:
                    OnPlayerGetData?.Invoke(typeof(DataHandler), new TogglePvPArgs(data, player));
                    return;

                case PacketTypes.PlayerSlot:
                    OnPlayerGetData?.Invoke(typeof(DataHandler), new PlayerSlotArgs(data, player));
                    return;

                case PacketTypes.PlayerDeathV2:
                    OnPlayerGetData?.Invoke(typeof(DataHandler), new PlayerDeathArgs(player));
                    return;

                case PacketTypes.ProjectileNew:
                    OnPlayerGetData?.Invoke(typeof(DataHandler), new ProjectileNewArgs(args, data));
                    return;

                case PacketTypes.ProjectileDestroy:
                    OnPlayerGetData?.Invoke(typeof(DataHandler), new ProjectileDestroyArgs(data));
                    return;

                case PacketTypes.PlayerUpdate:
                    OnPlayerGetData?.Invoke(typeof(DataHandler), new PlayerUpdateArgs(data, player));
                    return;

                case PacketTypes.ItemOwner:
                    OnPlayerGetData?.Invoke(typeof(DataHandler), new ItemOwnerArgs(player));
                    return;

                case PacketTypes.Tile:
                    OnPlayerGetData?.Invoke(typeof(DataHandler), new ModifyTilesArgs(data, player));
                    return;

                case PacketTypes.PlayerSpawn:
                    OnPlayerGetData?.Invoke(typeof(DataHandler), new PlayerSpawnArgs(data, player));
                    return;

                case PacketTypes.PlayerTeam:
                    OnPlayerGetData?.Invoke(typeof(DataHandler), new PlayerTeamArgs(data, player));
                    return;
            }
        }
    }
}
