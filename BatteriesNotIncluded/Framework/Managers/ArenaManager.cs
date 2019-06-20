using BatteriesNotIncluded.Framework;
using BatteriesNotIncluded.Minigames.Duel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatteriesNotIncluded.Framework.Managers {
    public class ArenaManager {
        public List<Arena> Arenas = new List<Arena>();

        public ArenaManager() {
            LoadArenas();
        }

        public void LoadArenas() {
            Arenas = new List<Arena>();
            var arenaTypes = MiscUtils.InstantiateClassesOfAbstract<Arena>();

            foreach (var arena in arenaTypes) {
                Arenas.AddRange(arena.GetArenas());
            }
        }

        public IEnumerable<Arena> GetAvailableArenas<T>(string name = "") {
            if (name != "") {
                Arenas.Find(c => c.GetType() == typeof(T) && c.Available && c.Name == name);
            }
            return Arenas.FindAll(c => c.GetType() == typeof(T) && c.Available);
        }

        public Arena GetArena(string name) {
            return Arenas.Find(c => c.Name == name);
        }

        public IEnumerable<Arena> GetArenas(Type type) {
            return Arenas.FindAll(c => c.GetType() == type);
        }
    }
}
