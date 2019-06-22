using BatteriesNotIncluded.Framework;
using Microsoft.Xna.Framework;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using TShockAPI.DB;

namespace BatteriesNotIncluded.Minigames.Splatoon {
    public class SplatoonArena : Arena {
        public override string Type => "Splatoon";

        public Vector2 ArenaTopLeft;
        public Vector2 ArenaBottomRight;

        public Vector2 RedSpawn;
        public Vector2 BlueSpawn;

        public Dictionary<Vector2, int> PaintSpotTiles;
        public Dictionary<Vector2, int> PaintSpotWalls;

        public override IEnumerable<Arena> GetArenas() {
            Type arenaType = GetType();

            List<SqlColumn> sqlColumns = new List<SqlColumn>();
            sqlColumns.Add(new SqlColumn("Name", MySqlDbType.Text) { Primary = true, Length = 255 });

            IEnumerable<string> vectors = this.GetVariableNamesOfType<Vector2>();

            foreach (string vector in vectors) {
                sqlColumns.Add(new SqlColumn(vector + "X", MySqlDbType.Int32));
                sqlColumns.Add(new SqlColumn(vector + "Y", MySqlDbType.Int32));
            }

            sqlColumns.Add(new SqlColumn("PaintSpotTiles", MySqlDbType.Text));
            sqlColumns.Add(new SqlColumn("PaintSpotWalls", MySqlDbType.Text));

            SqlTable sqlTable = new SqlTable(Type, sqlColumns);

            Main.ArenaDatabase.CreateTable(sqlTable);

            using (var reader = Main.ArenaDatabase.QueryReader($"SELECT * FROM {Type}")) {
                while (reader.Read()) {
                    Arena arena = (Arena)Activator.CreateInstance(arenaType);
                    arena.SetValue("Name", reader.Get<string>("Name"));

                    foreach (string vector in vectors) {
                        arena.SetValue(vector, new Vector2(reader.Get<int>(vector + "X"), reader.Get<int>(vector + "Y")));
                    }

                    // Initializes all the tile paint spots
                    Dictionary<Vector2, int> paintSpotsTile = new Dictionary<Vector2, int>();
                    string paintSpotList = reader.Get<string>("PaintSpotTiles");
                    var paintSpots = paintSpotList.Split('|');
                    foreach (var spots in paintSpots) {
                        if (string.IsNullOrEmpty(spots)) continue;

                        var spot = spots.Split('-');
                        var tile = spot[0].Split(',');
                        var paintColor = spot[1];

                        paintSpotsTile[new Vector2(int.Parse(tile[0]), int.Parse(tile[1]))] = int.Parse(paintColor);
                    }

                    arena.SetValue("PaintSpotTiles", paintSpotsTile);

                    // Initializes all the wall paint spots
                    Dictionary<Vector2, int> paintSpotsWall = new Dictionary<Vector2, int>();
                    paintSpotList = reader.Get<string>("PaintSpotWalls");
                    paintSpots = paintSpotList.Split('|');
                    foreach (var spots in paintSpots) {
                        if (string.IsNullOrEmpty(spots)) continue;

                        var spot = spots.Split('-');
                        var wall = spot[0].Split(',');
                        var paintColor = spot[1];

                        paintSpotsWall[new Vector2(int.Parse(wall[0]), int.Parse(wall[1]))] = int.Parse(paintColor);
                    }

                    arena.SetValue("PaintSpotWalls", paintSpotsWall);

                    yield return arena;
                }
            }
        }

        public override void InsertArena() {
            StringBuilder sb = new StringBuilder();

            sb.Append($"INSERT INTO {Type} VALUES (");

            List<string> values = new List<string>();
            values.Add(Name.SqlString());

            IEnumerable<string> vectors = this.GetVariableNamesOfType<Vector2>();

            foreach (string vector in vectors) {
                Vector2 vect = this.GetValue<Vector2>(vector);
                values.Add(vect.X.ToString());
                values.Add(vect.Y.ToString());
            }

            // Stores all the tile paints in the arena region
            List<string> paintSpotTiles = new List<string>();

            for (int x = (int)ArenaTopLeft.X / 16; x <= (int)ArenaBottomRight.X / 16; x++) {
                for (int y = (int)ArenaTopLeft.Y / 16; y <= (int)ArenaBottomRight.Y / 16; y++) {
                    if (Terraria.Main.tile[x, y].color() == 0) continue;
                    paintSpotTiles.Add($"{x},{y}-{Terraria.Main.tile[x, y].color()}");
                }
            }

            values.Add(string.Join("|", paintSpotTiles).SqlString());

            // Stores all the wall paints in the arena region
            List<string> paintSpotWalls = new List<string>();

            for (int x = (int)ArenaTopLeft.X / 16; x <= (int)ArenaBottomRight.X / 16; x++) {
                for (int y = (int)ArenaTopLeft.Y / 16; y <= (int)ArenaBottomRight.Y / 16; y++) {
                    if (Terraria.Main.tile[x, y].wallColor() == 0) continue;
                    paintSpotWalls.Add($"{x},{y}-{Terraria.Main.tile[x, y].wallColor()}");
                }
            }

            values.Add(string.Join("|", paintSpotWalls).SqlString());

            sb.Append(string.Join(",", values));

            sb.Append(");");

            Main.ArenaDatabase.Query(sb.ToString());

            Main.ArenaManager.LoadArenas();
        }
    }
}
