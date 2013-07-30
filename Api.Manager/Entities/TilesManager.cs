using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace Api.Manager.Entities
{
    class TilesManager
    {
        public static List<Tile> Tiles = new List<Tile>();

        public static void LoadTiles()
        {
            lock (DBManager.ConnectionLocker)
            {
                var sqlText = "SELECT * FROM tiles";
                var sqlCommand = new MySqlCommand(sqlText, DBManager.Connection);

                var sqlReader = sqlCommand.ExecuteReader();

                while (sqlReader.Read())
                {
                    var tile = new Tile()
                    {
                        ID = sqlReader.GetInt32("id_tile"),
                        X = sqlReader.GetInt32("x"),
                        Y = sqlReader.GetInt32("y"),
                        Type = sqlReader.GetInt32("type")
                    };

                    lock (Tiles)
                        Tiles.Add(tile);
                }

                sqlReader.Close();
            }

            Console.WriteLine("Loaded '{0}' tiles", Tiles.Count);
        }

        public class Tile
        {
            public int ID;
            public int X;
            public int Y;
            public int Type;

            public override string ToString()
            {
                return SecurityManager.EncodeTo64(ID.ToString()) + ";" + Y.ToString() + ";" + X.ToString();
            }
        }
    }
}
