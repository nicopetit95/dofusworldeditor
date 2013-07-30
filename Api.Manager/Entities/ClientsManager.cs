using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace Api.Manager.Entities
{
    class ClientsManager
    {
        public static bool isOk(string user, string licence, string macadress)
        {
            try
            {
                if (Clients.Any(x => x.Username == user && licence == string.Join("|", x.Licence) && x.MacAdds.Contains(macadress)))
                    return true;
                else
                    return false;
            }
            catch { return false; }
        }

        static List<DBClient> Clients = new List<DBClient>();

        class DBClient
        {
            public int ID;
            public string Username;
            public string[] Licence;
            public List<string> MacAdds = new List<string>();

            public DBClient(string licence)
            {
                Licence = new string[3];

                var licenceInfos = licence.Split('|');
                Licence[0] = SecurityManager.EncodeTo64(licenceInfos[0]);
                Licence[1] = SecurityManager.EncodeTo64(licenceInfos[1]);
                Licence[2] = SecurityManager.EncodeTo64(licenceInfos[2]);
            }
        }

        public static void ReloadClients()
        {
            lock (DBManager.ConnectionLocker)
            {
                var sqlText = "SELECT * FROM users";
                var sqlCommand = new MySqlCommand(sqlText, DBManager.Connection);

                var sqlReader = sqlCommand.ExecuteReader();

                while (sqlReader.Read())
                {
                    var client = new DBClient(sqlReader.GetString("Licence"))
                    {
                        ID = sqlReader.GetInt16("ID"),
                        Username = SecurityManager.EncodeTo64(sqlReader.GetString("Username"))
                    };

                    client.Username = client.Username.Substring(0, client.Username.Length - 2);

                    if (Clients.Any(x => x.ID == client.ID))
                        continue;

                    foreach (var to in sqlReader.GetString("MacAdds").Split('|'))
                        client.MacAdds.Add(SecurityManager.CryptMD5(to).Replace("-", ""));

                    lock (Clients)
                        Clients.Add(client);
                }

                sqlReader.Close();
            }

            Console.WriteLine("Reloaded Users");
        }
    }
}
