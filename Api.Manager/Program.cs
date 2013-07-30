using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Manager
{
    class Program
    {
        public static List<SockClient> Clients;

        static void Main(string[] args)
        {
            Console.WriteLine("Start of the LicenceMananger...");
            Clients = new List<SockClient>();

            try
            {
                SockServer server = new SockServer("178.170.95.136", 485);

                server.OnListeningEvent += new SockEvents.Listening(Listen);
                server.OnListeningFailedEvent += new SockEvents.ListeningFailed(FailedToListen);
                server.OnAcceptSocketEvent += new SockEvents.AcceptSocket(NewClient);

                server.WaitConnection();

                Entities.DBManager.InitializeConnection();
                Entities.DBManager.Disconnect();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.ReadKey();
        }

        static void NewClient(SockSocket client)
        {
            if (client != null)
            {
                lock(Clients)
                    Clients.Add(new SockClient(client));
            }
        }

        static void Listen()
        {
            Console.WriteLine("Start successfully !");
        }

        static void FailedToListen(Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}
