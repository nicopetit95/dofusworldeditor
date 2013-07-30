using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace DWE.MapEditor.CL
{
    class Database
    {
        private static MySqlConnection connection;
        private static object connectionLocker = new object();

        public static void Connect()
        {
            lock(connectionLocker)
            {
                connection = new MySqlConnection("server=" + Program.OptionsFrame.HostBDD + ";uid=" + Program.OptionsFrame.UserBDD +
                    ";pwd='" + Program.OptionsFrame.PwdBDD + "';database=" + Program.OptionsFrame.DatabaseBDD + ";");
                connection.Open();
            }
        }

        public static void Disconnect()
        {
            lock (connectionLocker)
                connection.Close();
        }

        public static void SaveMap(Frames.MapFrame frame)
        {
            try
            {
                lock (connectionLocker)
                {
                    Connect();

                    var text = "DELETE FROM maps WHERE id=" + frame.ID;
                    var command = new MySqlCommand(text, connection);

                    command.ExecuteNonQuery();

                    text = "INSERT INTO maps (id, date, width, heigth, places, mapData, monsters, capabilities, mappos, numgroup) VALUES ( '" +
                            frame.ID + "', '" + frame.signature + "', '" + frame.width + "', '" + frame.height + "', '" + frame.GetCellsFight(true) + "', '" +
                            frame.compressMap() + "', '', '" + frame.getCapabilities() + "', '0,0,0', '5');";
                    command = new MySqlCommand(text, connection);

                    command.ExecuteNonQuery();

                    Disconnect();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }
    }
}
