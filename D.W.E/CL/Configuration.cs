using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Drawing.Drawing2D;
using System.IO;
using System.Xml;

namespace DWE.MapEditor.CL
{
    class Configuration
    {
        public static Dictionary<string, List<DecalTile>> decalTiles;
        public static string Key = "";
        public static string Username = "";

        public class DecalTile
        {
            public int ID;
            public Point Decalages;
        }

        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = System.Convert.FromBase64String(encodedData);
            string returnValue = System.Text.Encoding.Unicode.GetString(encodedDataAsBytes);

            return returnValue;
        }

        public static void LoadConfiguration()
        {
            decalTiles = new Dictionary<string, List<DecalTile>>();

            if (!Directory.Exists("./input"))
                Directory.CreateDirectory("./input");

            if (!decalTiles.ContainsKey("Sols"))
                decalTiles.Add("Sols", new List<DecalTile>());

            if (!decalTiles.ContainsKey("Objets"))
                decalTiles.Add("Objets", new List<DecalTile>());

            if (!File.Exists("config"))
            {
                var writer = new StreamWriter("config", false, Encoding.Default);

                try
                {
                    writer.WriteLine("InsertInDBAuto=false");
                    writer.WriteLine("GenerateSWF=true");
                    writer.WriteLine("GenerateSWFServer=false");
                    writer.WriteLine("GenerateSQL=true");
                    writer.WriteLine("GenerateDM=true");
                    writer.WriteLine("DbHost=localhost");
                    writer.WriteLine("DbUser=root");
                    writer.WriteLine("DbPwd=");
                    writer.WriteLine("DbName=editor");
                    writer.WriteLine(@"SwfHost=.\output");
                    writer.WriteLine(@"ServerSwfLink=c:\wamp\www");
                    writer.WriteLine(@"SqlHost=.\output");
                    writer.WriteLine(@"DmHost=.\output");
                    writer.WriteLine("Key=");
                    writer.WriteLine("Username=");
                    writer.WriteLine("RightClicShowMenu=true");
                    writer.WriteLine("RightClicRemoveTile=true");

                    writer.Close();
                }
                catch { writer.Close(); }

                Application.Restart();
            }

            var reader = new StreamReader("config", Encoding.Default);

            try
            {

                Program.OptionsFrame.InsertAutoBDD = bool.Parse(reader.ReadLine().Split('=')[1].Trim());
                Program.OptionsFrame.mustCopyAutoSWF = bool.Parse(reader.ReadLine().Split('=')[1].Trim());
                Program.OptionsFrame.mustCopyServerSWF = bool.Parse(reader.ReadLine().Split('=')[1].Trim());
                Program.OptionsFrame.mustCopyAutoSQL = bool.Parse(reader.ReadLine().Split('=')[1].Trim());
                Program.OptionsFrame.mustCopyAutoDM = bool.Parse(reader.ReadLine().Split('=')[1].Trim());
                Program.OptionsFrame.HostBDD = reader.ReadLine().Split('=')[1].Trim();
                Program.OptionsFrame.UserBDD = reader.ReadLine().Split('=')[1].Trim();
                Program.OptionsFrame.PwdBDD = reader.ReadLine().Split('=')[1].Trim();
                Program.OptionsFrame.DatabaseBDD = reader.ReadLine().Split('=')[1].Trim();
                Program.OptionsFrame.LinkSWF = reader.ReadLine().Split('=')[1].Trim();
                Program.OptionsFrame.LinkCopySWF = reader.ReadLine().Split('=')[1].Trim();
                Program.OptionsFrame.LinkSQL = reader.ReadLine().Split('=')[1].Trim();
                Program.OptionsFrame.LinkDM = reader.ReadLine().Split('=')[1].Trim();
                Key = reader.ReadLine().Split('=')[1].Trim();
                Username = reader.ReadLine().Split('=')[1].Trim();
                Program.OptionsFrame.mustRightClicShowMenu = bool.Parse(reader.ReadLine().Split('=')[1].Trim());
                Program.OptionsFrame.mustRightClicRemoveTile = bool.Parse(reader.ReadLine().Split('=')[1].Trim());

                reader.Close();
            }
            catch
            {
                reader.Close();

                var writer = new StreamWriter("config", false, Encoding.Default);

                writer.WriteLine("InsertInDBAuto=false");
                writer.WriteLine("GenerateSWF=true");
                writer.WriteLine("GenerateSWFServer=false");
                writer.WriteLine("GenerateSQL=true");
                writer.WriteLine("GenerateDM=true");
                writer.WriteLine("DbHost=localhost");
                writer.WriteLine("DbUser=root");
                writer.WriteLine("DbPwd=");
                writer.WriteLine("DbName=editor");
                writer.WriteLine(@"SwfHost=.\output");
                writer.WriteLine(@"ServerSwfLink=c:\wamp\www");
                writer.WriteLine(@"SqlHost=.\output");
                writer.WriteLine(@"DmHost=.\output");
                writer.WriteLine("Key=");
                writer.WriteLine("Username=");
                writer.WriteLine("RightClicShowMenu=true");
                writer.WriteLine("RightClicRemoveTile=true");

                writer.Close();

                Application.Restart();
            }
        }

        public static void SaveConfiguration()
        {
            var writer = new StreamWriter("config", false, Encoding.Default);

            try
            {
                writer.WriteLine("InsertInDBAuto=" + Program.OptionsFrame.InsertAutoBDD.ToString());
                writer.WriteLine("GenerateSWF=" + Program.OptionsFrame.mustCopyAutoSWF.ToString());
                writer.WriteLine("GenerateSWFServer=" + Program.OptionsFrame.mustCopyServerSWF.ToString());
                writer.WriteLine("GenerateSQL=" + Program.OptionsFrame.mustCopyAutoSQL.ToString());
                writer.WriteLine("GenerateDM=" + Program.OptionsFrame.mustCopyAutoDM.ToString());
                writer.WriteLine("DbHost=" + Program.OptionsFrame.HostBDD.ToString());
                writer.WriteLine("DbUser=" + Program.OptionsFrame.UserBDD.ToString());
                writer.WriteLine("DbPwd=" + Program.OptionsFrame.PwdBDD.ToString());
                writer.WriteLine("DbName=" + Program.OptionsFrame.DatabaseBDD.ToString());
                writer.WriteLine("SwfHost=" + Program.OptionsFrame.LinkSWF.ToString());
                writer.WriteLine("ServerSwfLink=" + Program.OptionsFrame.LinkCopySWF.ToString());
                writer.WriteLine("SqlHost=" + Program.OptionsFrame.LinkSQL.ToString());
                writer.WriteLine("DmHost=" + Program.OptionsFrame.LinkDM.ToString());
                writer.WriteLine("Key=" + Key);
                writer.WriteLine("Username=" + Username);
                writer.WriteLine("RightClicShowMenu=" + Program.OptionsFrame.mustRightClicShowMenu.ToString());
                writer.WriteLine("RightClicRemoveTile=" + Program.OptionsFrame.mustRightClicRemoveTile.ToString());

                writer.Close();
            }
            catch { writer.Close(); }
        }
    }
}
