using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Api.Manager
{
    public class SockSocket
    {
        private object _flagLock = new object();
        private byte[] buffer = new byte[3004];
        private Socket _socket;

        private SockEvents.DataArrival _OnDataArrivalEvent;
        private SockEvents.SocketClosed _OnSocketClosedEvent;
        private SockEvents.Connected _OnConnected;
        private SockEvents.FailedToConnect _OnFailedToConnect;

        public string IP
        {
            get
            {
                return this._socket.RemoteEndPoint.ToString();
            }
        }

        public event SockEvents.DataArrival OnDataArrivalEvent
        {
            add
            {
                SockEvents.DataArrival dataArrival = this._OnDataArrivalEvent;
                SockEvents.DataArrival comparand;
                do
                {
                    comparand = dataArrival;
                    dataArrival = Interlocked.CompareExchange<SockEvents.DataArrival>(ref this._OnDataArrivalEvent, comparand + value, comparand);
                }
                while (dataArrival != comparand);
            }
            remove
            {
                SockEvents.DataArrival dataArrival = this._OnDataArrivalEvent;
                SockEvents.DataArrival comparand;
                do
                {
                    comparand = dataArrival;
                    dataArrival = Interlocked.CompareExchange<SockEvents.DataArrival>(ref this._OnDataArrivalEvent, comparand - value, comparand);
                }
                while (dataArrival != comparand);
            }
        }

        public event SockEvents.SocketClosed OnSocketClosedEvent
        {
            add
            {
                SockEvents.SocketClosed socketClosed = this._OnSocketClosedEvent;
                SockEvents.SocketClosed comparand;
                do
                {
                    comparand = socketClosed;
                    socketClosed = Interlocked.CompareExchange<SockEvents.SocketClosed>(ref this._OnSocketClosedEvent, comparand + value, comparand);
                }
                while (socketClosed != comparand);
            }
            remove
            {
                SockEvents.SocketClosed socketClosed = this._OnSocketClosedEvent;
                SockEvents.SocketClosed comparand;
                do
                {
                    comparand = socketClosed;
                    socketClosed = Interlocked.CompareExchange<SockEvents.SocketClosed>(ref this._OnSocketClosedEvent, comparand - value, comparand);
                }
                while (socketClosed != comparand);
            }
        }

        public event SockEvents.Connected OnConnected
        {
            add
            {
                SockEvents.Connected connected = this._OnConnected;
                SockEvents.Connected comparand;
                do
                {
                    comparand = connected;
                    connected = Interlocked.CompareExchange<SockEvents.Connected>(ref this._OnConnected, comparand + value, comparand);
                }
                while (connected != comparand);
            }
            remove
            {
                SockEvents.Connected connected = this._OnConnected;
                SockEvents.Connected comparand;
                do
                {
                    comparand = connected;
                    connected = Interlocked.CompareExchange<SockEvents.Connected>(ref this._OnConnected, comparand - value, comparand);
                }
                while (connected != comparand);
            }
        }

        public event SockEvents.FailedToConnect OnFailedToConnect
        {
            add
            {
                SockEvents.FailedToConnect failedToConnect = this._OnFailedToConnect;
                SockEvents.FailedToConnect comparand;
                do
                {
                    comparand = failedToConnect;
                    failedToConnect = Interlocked.CompareExchange<SockEvents.FailedToConnect>(ref this._OnFailedToConnect, comparand + value, comparand);
                }
                while (failedToConnect != comparand);
            }
            remove
            {
                SockEvents.FailedToConnect failedToConnect = this._OnFailedToConnect;
                SockEvents.FailedToConnect comparand;
                do
                {
                    comparand = failedToConnect;
                    failedToConnect = Interlocked.CompareExchange<SockEvents.FailedToConnect>(ref this._OnFailedToConnect, comparand - value, comparand);
                }
                while (failedToConnect != comparand);
            }
        }

        public SockSocket() { }

        public SockSocket(Socket connection, int bufferSize)
        {
            try
            {
                this.buffer = new byte[bufferSize];
                this._socket = connection;
                this.ThreadReceived();
            }
            catch { }
        }

        public void ConnectTo(string adress, int port)
        {
            try
            {
                this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this._socket.BeginConnect(IPAddress.Parse(adress), port, new AsyncCallback(this.ConnectCallBack), (object)this._socket);
            }
            catch (Exception ex)
            {
                this.NotifyFailedToConnect(ex);
            }
        }

        public void ThreadReceived()
        {
            try
            {
                this._socket.BeginReceive(this.buffer, 0, this.buffer.Length, SocketFlags.None, new AsyncCallback(this.ReceiveCallBack), (object)this._socket);
            }
            catch { }
        }

        private void ConnectCallBack(IAsyncResult ar)
        {
            try
            {
                if (this._socket.Connected)
                {
                    this.NotifyConnected();
                    this.ThreadReceived();
                }
                else
                    this.NotifyFailedToConnect(new Exception("Not Connect"));
            }
            catch { }
        }

        public void CloseSocket()
        {
            lock (this._flagLock)
            {
                try
                {
                    this._socket.Close();
                }
                catch { }
            }
        }

        private void ReceiveCallBack(IAsyncResult ar)
        {
            lock (this._flagLock)
            {
                try
                {
                    int local_0 = this._socket.EndReceive(ar);
                    if (local_0 > 0)
                    {
                        byte[] local_1 = new byte[local_0];
                        for (int local_2 = 0; local_2 <= local_0 - 1; ++local_2)
                            local_1[local_2] = this.buffer[local_2];
                        this.NotifyDataArrival(local_1);
                        this.buffer = new byte[5000];
                        this.ThreadReceived();
                    }
                    else
                        this.NotifySocketClosed();
                }
                catch
                {
                    this.NotifySocketClosed();
                }
            }
        }

        public void Send(string text)
        {
            try
            {
                if (Configuration.Debug)
                    Console.WriteLine("Sent >> " + text);

                Send(Encoding.UTF8.GetBytes(string.Format("{0}\x00", text)));
            }
            catch { }
        }

        public void Send(string format, params object[] objects)
        {
            try
            {
                Send(Encoding.UTF8.GetBytes(string.Format("{0}\x00", string.Format(format, objects))));
            }
            catch { }
        }

        private void Send(byte[] data)
        {
            lock (this._flagLock)
            {
                try
                {
                    this._socket.Send(data);
                }
                catch { }
            }
        }

        private void NotifyDataArrival(byte[] datas)
        {
            if (this._OnDataArrivalEvent == null)
                return;

            var notParsed = Encoding.UTF8.GetString(datas);

            foreach (var Packet in notParsed.Replace("\x0a", "").Split('\x00'))
            {
                if (Packet == "")
                    continue;

                this._OnDataArrivalEvent(Packet);
            }
        }

        private void NotifySocketClosed()
        {
            if (this._OnSocketClosedEvent == null)
                return;
            this._OnSocketClosedEvent();
        }

        private void NotifyConnected()
        {
            if (this._OnConnected == null)
                return;
            this._OnConnected();
        }

        private void NotifyFailedToConnect(Exception ex)
        {
            if (this._OnFailedToConnect == null)
                return;
            this._OnFailedToConnect(ex);
        }
    }
}
