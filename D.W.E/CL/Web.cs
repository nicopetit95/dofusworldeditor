using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Net;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Management;

namespace DWE.MapEditor.CL
{
    class Web
    {
        public static string NewVersion;
        public static string ActVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().FullName.Split(',')[1].Replace("Version=", "").Trim();
            }
        }

        public static WebClient myWebClient;

        public static bool HasUpdate()
        {
            try
            {
                if (System.IO.File.Exists("Updater.exe"))
                    System.IO.File.Delete("Updater.exe");

                myWebClient = new WebClient();
                myWebClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
                myWebClient.Headers.Add("Cache-Control", "no-cache");

                NewVersion = myWebClient.DownloadString("http://mapeditor.npdev.eu/updates/core_version.txt");

                myWebClient.Dispose();

                if (NewVersion != ActVersion)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
