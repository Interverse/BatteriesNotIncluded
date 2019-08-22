using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace BatteriesNotIncluded {
    public class Config {
        public static string ConfigPath = Path.Combine(TShock.SavePath, "batteriesnotincluded.json");

        public bool AutoJoinVotes = true;
        public bool EnableScorebar = true;
        public int CTFMaxScore = 3;
        public int TDMMaxScore = 10;
        public int DuelMaxScore = 5;
        public int SplatoonTimerInSeconds = 180;

        /// <summary>
        /// Writes the current internal server config to the external .json file
        /// </summary>
        /// <param Name="path"></param>
        public void Write(string path) {
            File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        /// <summary>
        /// Reads the .json file and stores its contents into the plugin
        /// </summary>
        public static Config Read(string path) {
            if (!File.Exists(path))
                return new Config();
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
        }
    }
}
