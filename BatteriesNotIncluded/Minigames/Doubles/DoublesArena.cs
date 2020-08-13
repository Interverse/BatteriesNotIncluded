using BatteriesNotIncluded.Framework;
using Microsoft.Xna.Framework;

namespace BatteriesNotIncluded.Minigames.Doubles {
    public class DoublesArena : Arena {
        public override string Type => "Doubles";

        public Vector2 Team1Spawn1;
        public Vector2 Team1Spawn2;

        public Vector2 Team2Spawn1;
        public Vector2 Team2Spawn2;

        public Vector2 SpectateArea;
    }
}
