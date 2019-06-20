using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using TShockAPI;
using TShockAPI.DB;

namespace BatteriesNotIncluded.Framework {
    public class Database {
        public bool IsMySql => db.GetSqlType() == SqlType.Mysql;
        private readonly List<SqlTable> _tables = new List<SqlTable>();
        private readonly string _name;

        public IDbConnection db;

        public Database(string name) {
            _name = name;
        }

        /// <summary>
        /// Connects the mysql/sqlite database for the plugin, creating one if the database doesn't already exist.
        /// </summary>
        public Database ConnectDB() {
            if (TShock.Config.StorageType.ToLower() == "sqlite")
                db = new SqliteConnection(string.Format("uri=file://{0},Version=3",
                    Path.Combine(TShock.SavePath, $"{_name}.sqlite")));
            else if (TShock.Config.StorageType.ToLower() == "mysql") {
                try {
                    var host = TShock.Config.MySqlHost.Split(':');
                    db = new MySqlConnection {
                        ConnectionString = string.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4}",
                            host[0],
                            host.Length == 1 ? "3306" : host[1],
                            TShock.Config.MySqlDbName,
                            TShock.Config.MySqlUsername,
                            TShock.Config.MySqlPassword)
                    };
                } catch (MySqlException x) {
                    TShock.Log.Error(x.ToString());
                    throw new Exception("MySQL not setup correctly.");
                }
            } else
                throw new Exception("Invalid storage type.");

            return this;
        }

        public Database CreateTable(SqlTable table) {
            var sqlCreator = new SqlTableCreator(db,
                IsMySql
                    ? (IQueryBuilder)new MysqlQueryCreator()
                    : (IQueryBuilder)new SqliteQueryCreator());

            sqlCreator.EnsureTableStructure(table);
            return this;
        }

        public QueryResult QueryReader(string query, params object[] args) {
            return db.QueryReader(query, args);
        }

        /// <summary>
        /// Performs an SQL query
        /// </summary>
        /// <param name="query">The SQL statement</param>
        /// <returns>
        /// Returns true if the statement was successful.
        /// Returns false if the statement failed.
        /// </returns>
        public bool Query(string query) {
            bool success = true;
            db.Open();
            try {
                using (var conn = db.CreateCommand()) {
                    conn.CommandText = query;
                    conn.ExecuteNonQuery();
                }
            } catch (Exception e) {
                TShock.Log.Write(e.ToString(), TraceLevel.Error);
                success = false;
            }

            db.Close();
            return success;
        }

        /// <summary>
        /// Deletes the contents of an entire row.
        /// </summary>
        /// <param name="table">The table to delete from</param>
        /// <param name="id">The ID of the data being deleted</param>
        public void DeleteRow(string table, string name) {
            Query("DELETE FROM {0} WHERE Name = {1}".SFormat(table, name.SqlString()));
        }

        /// <summary>
        /// Performs a series of sql statements in a transaction.
        /// This allows for fast mass querying as opposed to querying
        /// one statement at a time.
        /// </summary>
        /// <param name="queries"></param>
        public void PerformTransaction(string[] queries) {
            var conn = IsMySql
                ? (DbConnection)new MySqlConnection(db.ConnectionString)
                : new SqliteConnection(db.ConnectionString);

            conn.Open();

            using (var cmd = conn.CreateCommand()) {
                using (var transaction = conn.BeginTransaction()) {
                    foreach (string query in queries) {
                        cmd.CommandText = query;
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }

            conn.Close();
        }

        /// <summary>
        /// Writes the changed attribute of an item to the sql database.
        /// </summary>
        public bool Update<T>(string table, int index, string column, T value) {
            bool selectAll = index < 0;

            if (value is string) value = (T)Convert.ChangeType(value.ToString().SqlString(), typeof(T));

            string sourceId = !selectAll ? " WHERE ID = {0}".SFormat(index) : "";
            return Query(string.Format("UPDATE {0} SET {1} = {2}{3}", table, column, value, sourceId));
        }

        /// <summary>
        /// Gets the value of an item, projectile, or buff based off id.
        /// </summary>
        public T GetData<T>(string table, int id, string column) {
            using (var reader = QueryReader(string.Format("SELECT {0} FROM {1} WHERE ID = {2}", column, table, id.ToString()))) {
                while (reader.Read()) {
                    return reader.Get<T>(column);
                }
            }

            return default(T);
        }

        /// <summary>
        /// Gets the value of an item, projectile, or buff based off id.
        /// </summary>
        public object GetDataWithType(string table, int id, string column, Type type) {
            MethodInfo getDataMethod = typeof(Database).GetMethod("GetData")?.MakeGenericMethod(type);

            return getDataMethod?.Invoke(null, new object[] { table, id, column });
        }

        /// <summary>
        /// Gets the type of the sql column.
        /// </summary>
        public Type GetType(string table, string column) {
            try {
                using (var reader = QueryReader(string.Format("SELECT {0} FROM {1}", column, table))) {
                    while (reader.Read()) {
                        return reader.Reader.GetFieldType(0);
                    }
                }
            } catch (Exception e) {
                TShock.Log.Write(e.ToString(), TraceLevel.Error);
            }

            return default(Type);
        }
    }
}