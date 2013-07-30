using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Api.Manager.Master
{
    class TCPClient
    {
        private SockSocket _socket;
        private bool _isConnected = false;

        public bool Connected
        {
            get
            {
                return _isConnected;
            }
        }

        protected delegate void DisconnectedSocketHandler();
        protected DisconnectedSocketHandler DisconnectedSocket;

        private void OnDisconnectedSocket()
        {
            var evnt = DisconnectedSocket;
            if (evnt != null)
                evnt();
        }

        protected delegate void ReceiveDatasHandler(string message);
        protected ReceiveDatasHandler ReceivedDatas;

        private void OnReceivedDatas(string message)
        {
            var evnt = ReceivedDatas;
            if (evnt != null)
                evnt(message);
        }

        protected delegate void ConnectFailedHandler(Exception exception);
        protected ConnectFailedHandler ConnectFailed;

        private void OnConnectFailed(Exception exception)
        {
            var evnt = ConnectFailed;
            if (evnt != null)
                evnt(exception);
        }

        public TCPClient(SockSocket socket)
        {
            _socket = socket;

            _socket.OnConnected += new SockEvents.Connected(this.OnConnected);
            _socket.OnSocketClosedEvent += new SockEvents.SocketClosed(this.OnDisconnected);
            _socket.OnDataArrivalEvent += new SockEvents.DataArrival(this.OnDatasArrival);
            _socket.OnFailedToConnect += new SockEvents.FailedToConnect(this.OnFailedToConnect);
        }

        #region Functions

        public void ConnectTo(string ip, int port)
        {
            _socket.ConnectTo(ip, port);
        }

        public string myIp()
        {
            return _socket.IP;
        }

        public void Disconnect()
        {
            _socket.CloseSocket();
        }

        protected void SendDatas(string message)
        {
            try
            {
                _socket.Send(message);
            }
            catch { }
        }

        #endregion

        #region Events

        private void OnDatasArrival(string datas)
        {
            OnReceivedDatas(datas);                
        }

        private void OnConnected()
        {
            _isConnected = true;
        }

        private void OnFailedToConnect(Exception e)
        {
            OnConnectFailed(e);
        }

        private void OnDisconnected()
        {
            _isConnected = false;

            OnDisconnectedSocket();
        }

        #endregion
    }
}
