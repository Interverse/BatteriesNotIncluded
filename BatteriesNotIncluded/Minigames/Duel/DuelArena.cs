using BatteriesNotIncluded.Framework;
using Microsoft.Xna.Framework;

namespace BatteriesNotIncluded.Minigames.Duel {
    public class DuelArena : Arena {
        public override string Type => "Duel";

        public Vector2 Player1Spawn = default;
        public Vector2 Player2Spawn = default;
    }
}
