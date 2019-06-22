using Microsoft.Xna.Framework;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using TShockAPI.DB;

namespace BatteriesNotIncluded.Framework {
    public abstract class Arena {
        public abstract string Type { get; }
        public string Name;

        public bool Available = true;

        public virtual IEnumerable<Arena> GetArenas() {
            Type arenaType = GetType();

            List<SqlColumn> sqlColumns = new List<SqlColumn>();
            sqlColumns.Add(new SqlColumn("Name", MySqlDbType.Text) { Primary = true, Length = 255 });

            IEnumerable<string> vectors = this.GetVariableNamesOfType<Vector2>();

            foreach (string vector in vectors) {
                sqlColumns.Add(new SqlColumn(vector + "X", MySqlDbType.Int32));
                sqlColumns.Add(new SqlColumn(vector + "Y", MySqlDbType.Int32));
            }

            SqlTable sqlTable = new SqlTable(Type, sqlColumns);

            Main.ArenaDatabase.CreateTable(sqlTable);

            using (var reader = Main.ArenaDatabase.QueryReader($"SELECT * FROM {Type}")) {
                while (reader.Read()) {
                    Arena arena = (Arena)Activator.CreateInstance(arenaType);
                    arena.SetValue("Name", reader.Get<string>("Name"));

                    foreach (string vector in vectors) {
                        arena.SetValue(vector, new Vector2(reader.Get<int>(vector + "X"), reader.Get<int>(vector + "Y")));
                    }

                    yield return arena;
                }
            }
        }

        public virtual void InsertArena() {
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

            sb.Append(string.Join(",", values));

            sb.Append(");");

            Main.ArenaDatabase.Query(sb.ToString());

            Main.ArenaManager.LoadArenas();
        }
    }
}
