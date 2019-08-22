using BatteriesNotIncluded.Framework;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatteriesNotIncluded.Minigames.TDM {
    public class TDMArena : Arena {
        public override string Type => "TDM";

        public Vector2 RedSpawn;
        public Vector2 BlueSpawn;
    }
}
