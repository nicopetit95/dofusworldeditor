using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Api.Manager
{
    public class SockServer
    {
        private object _flagLock = new object();
        private string _listeningAdress;
        private int _port;
        private int _maxPossibleConnection;
        private Socket _socket;

        private SockEvents.Listening _OnListeningEvent;
        private SockEvents.ListeningFailed _OnListeningFailedEvent;
        private SockEvents.AcceptSocket _OnAcceptSocketEvent;

        public event SockEvents.Listening OnListeningEvent
        {
            add
            {
                SockEvents.Listening listening = this._OnListeningEvent;
                SockEvents.Listening comparand;
                do
                {
                    comparand = listening;
                    listening = Interlocked.CompareExchange<SockEvents.Listening>(ref this._OnListeningEvent, comparand + value, comparand);
                }
                while (listening != comparand);
            }
            remove
            {
                SockEvents.Listening listening = this._OnListeningEvent;
                SockEvents.Listening comparand;
                do
                {
                    comparand = listening;
                    listening = Interlocked.CompareExchange<SockEvents.Listening>(ref this._OnListeningEvent, comparand - value, comparand);
                }
                while (listening != comparand);
            }
        }

        public event SockEvents.ListeningFailed OnListeningFailedEvent
        {
            add
            {
                SockEvents.ListeningFailed listeningFailed = this._OnListeningFailedEvent;
                SockEvents.ListeningFailed comparand;
                do
                {
                    comparand = listeningFailed;
                    listeningFailed = Interlocked.CompareExchange<SockEvents.ListeningFailed>(ref this._OnListeningFailedEvent, comparand + value, comparand);
                }
                while (listeningFailed != comparand);
            }
            remove
            {
                SockEvents.ListeningFailed listeningFailed = this._OnListeningFailedEvent;
                SockEvents.ListeningFailed comparand;
                do
                {
                    comparand = listeningFailed;
                    listeningFailed = Interlocked.CompareExchange<SockEvents.ListeningFailed>(ref this._OnListeningFailedEvent, comparand - value, comparand);
                }
                while (listeningFailed != comparand);
            }
        }

        public event SockEvents.AcceptSocket OnAcceptSocketEvent
        {
            add
            {
                SockEvents.AcceptSocket acceptSocket = this._OnAcceptSocketEvent;
                SockEvents.AcceptSocket comparand;
                do
                {
                    comparand = acceptSocket;
                    acceptSocket = Interlocked.CompareExchange<SockEvents.AcceptSocket>(ref this._OnAcceptSocketEvent, comparand + value, comparand);
                }
                while (acceptSocket != comparand);
            }
            remove
            {
                SockEvents.AcceptSocket acceptSocket = this._OnAcceptSocketEvent;
                SockEvents.AcceptSocket comparand;
                do
                {
                    comparand = acceptSocket;
                    acceptSocket = Interlocked.CompareExchange<SockEvents.AcceptSocket>(ref this._OnAcceptSocketEvent, comparand - value, comparand);
                }
                while (acceptSocket != comparand);
            }
        }

        public SockServer(string listeningAdress, int port, int bufferSize = 3004)
        {
            this._listeningAdress = listeningAdress;
            this._port = port;
            this._maxPossibleConnection = bufferSize;
        }

        public void WaitConnection()
        {
            try
            {
                this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this._socket.Bind((EndPoint)new IPEndPoint(IPAddress.Parse(this._listeningAdress), this._port));
                this._socket.Listen(10);
                new Thread(new ThreadStart(this.AcceptThread)).Start();
                this.NotifyListening();
            }
            catch (Exception ex)
            {
                this.NotifyListeningFailed(ex);
            }
        }

        public void Close()
        {
            lock (this._flagLock)
            {
                try
                {
                    this._socket.Close();
                }
                catch
                {
                }
            }
        }

        private void AcceptThread()
        {
            lock (this._flagLock)
            {
                try
                {
                    this._socket.BeginAccept(new AsyncCallback(this.AcceptCallBack), (object)this._socket);
                }
                catch
                {
                    this.AcceptThread();
                }
            }
        }

        public void NotifyListening()
        {
            if (this._OnListeningEvent == null)
                return;
            this._OnListeningEvent();
        }

        public void NotifyListeningFailed(Exception ex)
        {
            if (this._OnListeningFailedEvent == null)
                return;
            this._OnListeningFailedEvent(ex);
        }

        public void NotifyAcceptSocket(SockSocket socket)
        {
            if (this._OnAcceptSocketEvent == null)
                return;
            this._OnAcceptSocketEvent(socket);
        }

        private void AcceptCallBack(IAsyncResult ar)
        {
            lock (this._flagLock)
            {
                try
                {
                    this.NotifyAcceptSocket(new SockSocket(this._socket.EndAccept(ar), 5000));
                    this.AcceptThread();
                }
                catch
                {
                    this.AcceptThread();
                }
            }
        }
    }
}
