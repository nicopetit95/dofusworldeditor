using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Api.Manager
{
    class SockClient : Master.TCPClient
    {
        private State state;

        private string key;
        private string user;
        private string macadress;

        private bool isAuth = false;

        public SockClient(SockSocket socket) : base(socket)
        {
            this.DisconnectedSocket += new DisconnectedSocketHandler(this.Disconnected);
            this.ReceivedDatas += new ReceiveDatasHandler(this.ReceiveDatas);

            state = State.OnLicence;

            SendDatas(SecurityManager.CryptMD5("VUERHVOERUIV").Replace("-", ""));
        }

        private void Disconnected()
        {
            lock(Program.Clients)
                Program.Clients.Remove(this);
        }

        private void ReceiveDatas(string datas)
        {
            if(Configuration.Debug)
                Console.WriteLine("Receive >> " + datas);

            switch (state)
            {
                case State.OnLicence:

                    SendDatas(SecurityManager.CryptMD5("ZUERVJHRE9PUJVRE!!").Replace("-", ""));
                    key = datas;
                    state = State.OnMacAdd;

                    break;

                case State.OnMacAdd:

                    SendDatas(SecurityManager.CryptMD5("EZSHREPUSIOJGER!!").Replace("-", ""));
                    macadress = datas;
                    state = State.OnUser;

                    break;

                case State.OnUser:

                    user = datas;

                    if (Entities.ClientsManager.isOk(user, key, macadress))
                    {
                        var resultKey = Utilities.RandomString(10);
                        var resultPass = SecurityManager.CryptWithKey(this.myIp().Split(':')[0], SecurityManager.CryptWithKey(resultKey, macadress));
                        resultKey = SecurityManager.EncodeTo64(resultKey);

                        isAuth = true;

                        SendDatas(resultPass + "|" + resultKey);

                        state = State.OnTiles;
                    }
                    else
                    {
                        var resultKey = "YOUR KEYFL";
                        var resultPass = SecurityManager.CryptWithKey(this.myIp().Split(':')[0], SecurityManager.CryptWithKey(resultKey, macadress));
                        resultKey = SecurityManager.EncodeTo64(resultKey);

                        SendDatas(resultPass + "|" + resultKey);

                        Disconnect();
                    }

                    break;

                case State.OnTiles:

                    if (isAuth)
                    {
                        var packet = datas.Split('~');

                        if (SecurityManager.CryptMD5("grounds").Replace("-", "") == packet[0])
                        {
                            if (Entities.TilesManager.Tiles.Any(x => x.Type == 0 && x.ID == int.Parse(SecurityManager.DecodeFrom64(packet[1]))))
                                SendDatas(SecurityManager.CryptMD5("grounds").Replace("-", "") + SecurityManager.EncodeTo64(Entities.TilesManager.Tiles.First(x => x.Type == 0 && x.ID == int.Parse(SecurityManager.DecodeFrom64(packet[1]))).ToString()));
                            else
                                SendDatas(SecurityManager.CryptMD5("failgrd?").Replace("-", "") + packet[1]);
                        }
                        else if (SecurityManager.CryptMD5("objects").Replace("-", "") == packet[0])
                        {
                            if (Entities.TilesManager.Tiles.Any(x => x.Type == 1 && x.ID == int.Parse(SecurityManager.DecodeFrom64(packet[1]))))
                                SendDatas(SecurityManager.CryptMD5("objects").Replace("-", "") + SecurityManager.EncodeTo64(Entities.TilesManager.Tiles.First(x => x.Type == 1 && x.ID == int.Parse(SecurityManager.DecodeFrom64(packet[1]))).ToString()));
                            else
                                SendDatas(SecurityManager.CryptMD5("failobj?").Replace("-", "") + packet[1]);
                        }
                        else if (SecurityManager.CryptMD5("groundsLIST").Replace("-", "") == packet[0])
                        {
                            var datasGrounds = SecurityManager.CryptMD5("groundsLIST").Replace("-", "");

                            foreach (var tile in packet[1].Split('|'))
                            {
                                var tileID = int.Parse(tile);
                                var isOk = Entities.TilesManager.Tiles.Any(x => x.Type == 0 && x.ID == tileID);
                                datasGrounds += (isOk ? Entities.TilesManager.Tiles.First(x => x.Type == 0 && x.ID == tileID).ToString() + "|" : "|");
                            }

                            SendDatas(datasGrounds.Substring(0, datasGrounds.Length - 1));
                        }
                        else if (SecurityManager.CryptMD5("objectsLIST").Replace("-", "") == packet[0])
                        {
                            var datasObjects = SecurityManager.CryptMD5("objectsLIST").Replace("-", "");

                            foreach (var tile in packet[1].Split('|'))
                            {
                                var tileID = int.Parse(tile);
                                var isOk = Entities.TilesManager.Tiles.Any(x => x.Type == 1 && x.ID == tileID);
                                datasObjects += (isOk ? Entities.TilesManager.Tiles.First(x => x.Type == 1 && x.ID == tileID).ToString() + "|" : "|");
                            }

                            SendDatas(datasObjects.Substring(0, datasObjects.Length - 1));
                        }
                    }

                    break;
            }
        }

        private enum State
        {
            OnLicence,
            OnMacAdd,
            OnUser,
            OnTiles,
        }
    }
}
