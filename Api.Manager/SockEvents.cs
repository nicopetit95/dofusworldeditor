using System;

namespace Api.Manager
{
    public class SockEvents
    {
        public delegate void Listening();

        public delegate void ListeningFailed(Exception ex);

        public delegate void AcceptSocket(SockSocket socket);

        public delegate void DataArrival(string datas);

        public delegate void Connected();

        public delegate void FailedToConnect(Exception ex);

        public delegate void SocketClosed();
    }
}