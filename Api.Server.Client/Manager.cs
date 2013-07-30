using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Api.Server.Client
{
    public class Manager
    {
        public delegate void OnInitialized();
        public delegate void OnInitializeFailed(Exception e);
        public delegate void OnDisconnected();
        public delegate void OnLoadedTile(TileType t);

        public static OnInitialized meInit;
        public static OnInitializeFailed meFailed;
        public static OnDisconnected meDisco;
        public static OnLoadedTile meLoaded;

        private static SockSocket socket;
        private static string[] licence;
        private static string user;
        private static string clearAdd;
        private static string noclearadd;
        private static State state = State.OnLicence;
        private static bool initialized = false;

        private static Dictionary<TileType, List<Tile>> Tiles = new Dictionary<TileType, List<Tile>>();
        private static Dictionary<TileType, List<int>> UnknowsTiles = new Dictionary<TileType, List<int>>();

        private static void OnInitLicence()
        {
            var evnt = meInit;
            if (evnt != null)
                evnt();
        }
        private static void OnInitLicenceFailed(Exception e)
        {
            var evnt = meFailed;
            if (evnt != null)
                evnt(e);
        }
        private static void OnDisco()
        {
            var evnt = meDisco;
            if (evnt != null)
                evnt();
        }
        private static void OnLoaded(TileType t)
        {
            var evnt = meLoaded;
            if (evnt != null)
                evnt(t);
        }

        public static void Initialise(string _ip, int _port, string _user, string[] _licence)
        {
            socket = new SockSocket();
            socket.OnDataArrivalEvent += new SockEvents.DataArrival(ReceiveDatas);
            socket.OnFailedToConnect += new SockEvents.FailedToConnect(FailedToConnected);
            socket.OnSocketClosedEvent += new SockEvents.SocketClosed(Disconnected);

            user = _user;
            licence = _licence;
            state = State.OnLicence;

            clearAdd = Utilities.GetMacAddress();
            if(clearAdd == null)
                OnInitLicenceFailed(new Exception("MacAdress cannot be null !"));

            if (!initialized)
            {
                Tiles.Add(TileType.grounds, new List<Tile>());
                Tiles.Add(TileType.objects, new List<Tile>());
                UnknowsTiles.Add(TileType.grounds, new List<int>());
                UnknowsTiles.Add(TileType.objects, new List<int>());
            }

            initialized = true;

            socket.ConnectTo(_ip, _port);
        }

        public static void Disconnect()
        {
            socket.CloseSocket();
        }

        public static string GetTile(TileType type, int tileID)
        {
            if (!Tiles[type].Any(x => x.ID == tileID) && !UnknowsTiles[type].Contains(tileID))
            {
                socket.Send(string.Concat(SecurityManager.CryptMD5(type.ToString()).Replace("-", ""), "~", SecurityManager.EncodeTo64(tileID.ToString())));
                return "";
            }
            else if (UnknowsTiles[type].Contains(tileID))
                return "";
            else if (!Tiles[type].Any(x => x.ID == tileID))
                return "";

            return Tiles[type].First(x => x.ID == tileID).ToString();
        }

        public static void UpdatesTiles(TileType type, List<int> tiles)
        {
            var packet = string.Concat(SecurityManager.CryptMD5(type.ToString() + "LIST").Replace("-", ""), "~");

            foreach(var tile in tiles)
                packet += tile.ToString() + "|";
            
            socket.Send(packet.Substring(0, packet.Length - 1));
        }

        private static void FailedToConnected(Exception e)
        {
            OnInitLicenceFailed(e);
        }

        private static void Disconnected()
        {
            OnDisco();
        }

        private static void ReceiveDatas(string datas)
        {
            switch (state)
            {
                case State.OnLicence:

                    if (licence[0] == "" || licence.Length < 2)
                        FailedToConnected(new Exception("Failed by authentication !"));
                    else
                    {
                        socket.Send(string.Join("|", from i in licence select SecurityManager.EncodeTo64(i)));
                        state = State.OnMacAdd;
                    }

                    break;

                case State.OnMacAdd:

                    noclearadd = SecurityManager.CryptMD5(Utilities.GetMacAddress()).Replace("-", "");
                    socket.Send(noclearadd);
                    state = State.OnUser;

                    break;

                case State.OnUser:

                    if (user == "")
                        FailedToConnected(new Exception("Failed by authentication !"));
                    else
                    {
                        var packet = SecurityManager.EncodeTo64(user);
                        socket.Send(packet.Substring(0, packet.Length - 2));
                        state = State.OnAuth;
                    }

                    break;

                case State.OnAuth:

                    var infos = datas.Split('|');

                    if (infos.Length < 3)
                    {
                        var key = SecurityManager.CryptWithKey(SecurityManager.DecodeFrom64(infos[1]), noclearadd);

                        if (SecurityManager.DecodeFrom64(infos[1]) == "YOUR KEYFL")
                        {
                            OnInitLicenceFailed(new Exception("Failed by authentication !"));
                            return;
                        }

                        if (SecurityManager.CryptWithKey(Utilities.GetPublicIP(), key) == infos[0])
                        {
                            OnInitLicence();
                            state = State.OnTiles;
                        }
                        else
                            OnInitLicenceFailed(new Exception("Failed by authentication !"));
                    }

                    break;

                case State.OnTiles:

                    var groundsListMD5 = SecurityManager.CryptMD5("groundsLIST").Replace("-", "");
                    var groundsMD5 = SecurityManager.CryptMD5("grounds").Replace("-", "");
                    var objectsListMD5 = SecurityManager.CryptMD5("objectsLIST").Replace("-", "");
                    var objectsMD5 = SecurityManager.CryptMD5("objects").Replace("-", "");
                    var failgrdMD5 = SecurityManager.CryptMD5("failgrd?").Replace("-", "");
                    var failobjMD5 = SecurityManager.CryptMD5("failobj?").Replace("-", "");

                    if (datas.StartsWith(groundsMD5))
                    {
                        var tileinfos = SecurityManager.DecodeFrom64(datas.Substring(groundsMD5.Length)).Split(';');
                        var tileid = int.Parse(SecurityManager.DecodeFrom64(tileinfos[0]));
                        var y = int.Parse(tileinfos[1]);
                        var x = int.Parse(tileinfos[2]);

                        if (!Tiles[TileType.grounds].Any(t => t.ID == tileid))
                            Tiles[TileType.grounds].Add(new Tile() { ID = tileid, Y = y, X = x });

                        OnLoaded(TileType.grounds);
                    }
                    else if (datas.StartsWith(objectsMD5))
                    {
                        var tileinfos = SecurityManager.DecodeFrom64(datas.Substring(objectsMD5.Length)).Split(';');
                        var tileid = int.Parse(SecurityManager.DecodeFrom64(tileinfos[0]));
                        var y = int.Parse(tileinfos[1]);
                        var x = int.Parse(tileinfos[2]);

                        if (!Tiles[TileType.objects].Any(t => t.ID == tileid))
                            Tiles[TileType.objects].Add(new Tile() { ID = tileid, Y = y, X = x });

                        OnLoaded(TileType.objects);
                    }
                    else if (datas.StartsWith(groundsListMD5))
                    {
                        foreach (var tile in datas.Substring(groundsListMD5.Length).Split('|'))
                        {
                            if (tile == "")
                                continue;

                            var tileinfos = tile.Split(';');
                            var tileid = int.Parse(SecurityManager.DecodeFrom64(tileinfos[0]));
                            var y = int.Parse(tileinfos[1]);
                            var x = int.Parse(tileinfos[2]);

                            if (!Tiles[TileType.grounds].Any(t => t.ID == tileid))
                                Tiles[TileType.grounds].Add(new Tile() { ID = tileid, Y = y, X = x });
                        }

                        OnLoaded(TileType.grounds);
                    }
                    else if (datas.StartsWith(objectsListMD5))
                    {
                        foreach (var tile in datas.Substring(objectsListMD5.Length).Split('|'))
                        {
                            if (tile == "")
                                continue;

                            var tileinfos = tile.Split(';');
                            var tileid = int.Parse(SecurityManager.DecodeFrom64(tileinfos[0]));
                            var y = int.Parse(tileinfos[1]);
                            var x = int.Parse(tileinfos[2]);

                            if(!Tiles[TileType.objects].Any(t => t.ID == tileid))
                                Tiles[TileType.objects].Add(new Tile() { ID = tileid, Y = y, X = x });
                        }

                        OnLoaded(TileType.objects);
                    }
                    else if (datas.StartsWith(failgrdMD5))
                    {
                        var tileid = int.Parse(SecurityManager.DecodeFrom64(datas.Substring(failgrdMD5.Length)));
                        UnknowsTiles[TileType.grounds].Add(tileid);
                    }
                    else if (datas.StartsWith(failobjMD5))
                    {
                        var tileid = int.Parse(SecurityManager.DecodeFrom64(datas.Substring(failobjMD5.Length)));
                        UnknowsTiles[TileType.objects].Add(tileid);
                    }

                    break;
            }
        }

        public enum TileType
        {
            grounds,
            objects,
        }

        private enum State
        {
            OnLicence,
            OnMacAdd,
            OnUser,
            OnAuth,
            OnTiles,
        }

        private class Tile
        {
            public int ID;
            public int X;
            public int Y;

            public override string ToString()
            {
                return string.Concat(SecurityManager.EncodeTo64(ID.ToString()), ";", X, ";", Y);
            }
        }
    }
}
