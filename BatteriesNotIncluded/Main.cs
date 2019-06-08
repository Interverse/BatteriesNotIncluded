using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatteriesNotIncluded.Framework;
using MySql.Data.MySqlClient;
using Terraria.GameContent.Generation;
using TerrariaApi.Server;
using TShockAPI.DB;

namespace BatteriesNotIncluded {
    [ApiVersion(2, 1)]
    public class Main : TerrariaPlugin {
        public static Main Instance;

        public override string Name => "BatteriesNotIncluded";
        public override string Author => "Johuan";
        public override string Description => "Adds minigames to Terraria";
        public override Version Version => new Version(0, 1, 0, 0);

        public Main(Terraria.Main game) : base(game) { }

        public override void Initialize() {
            Instance = this;
        }

        public void RegisterGame(Minigame mg) {
            ServerApi.Hooks.NetGetData.Register(this, mg.OnGameRunning);
        }

        public void DeregisterGame(Minigame mg) {
            ServerApi.Hooks.NetGetData.Deregister(this, mg.OnGameRunning);
        }
    }
}
