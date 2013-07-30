using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Timers;

namespace Api.Manager.Entities
{
    class DBManager
    {
        public static MySqlConnection Connection;
        public static object ConnectionLocker;

        private static Timer _timer;

        public static void InitializeConnection()
        {
            Connection = new MySqlConnection(string.Format("server={0};uid={1};pwd='{2}';database={3}",
                    Configuration.DB_Host,
                    Configuration.DB_User,
                    Configuration.DB_Pass,
                    Configuration.DB_Name));

            ConnectionLocker = new object();

            lock (ConnectionLocker)
                Connection.Open();

            _timer = new Timer();
            _timer.Interval = 300000;
            _timer.Elapsed += new ElapsedEventHandler(UpdateTables);
            _timer.Start();

            Console.WriteLine("Connected with the database !");
            ClientsManager.ReloadClients();
            TilesManager.LoadTiles();
        }

        public static void Disconnect()
        {
            lock (ConnectionLocker)
                Connection.Close();
        }

        private static void ReConnect()
        {
            lock (ConnectionLocker)
                Connection.Open();
        }

        private static void ReDisconnect()
        {
            lock (ConnectionLocker)
                Connection.Close();
        }

        private static void UpdateTables(object sender, EventArgs e)
        {
            ReConnect();

            ClientsManager.ReloadClients();

            ReDisconnect();
        }
    }
}
