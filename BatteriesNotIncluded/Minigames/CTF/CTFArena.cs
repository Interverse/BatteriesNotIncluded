using BatteriesNotIncluded.Framework;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatteriesNotIncluded.Minigames.CTF {
    public class CTFArena : Arena {
        public override string Type => "CTF";

        public Vector2 RedSpawn = default;
        public Vector2 RedFlag = default;
        public Vector2 BlueSpawn = default;
        public Vector2 BlueFlag = default;
    }
}
