using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace Updater
{
    class Program
    {
        static WebClient myWebClient;

        static void Main(string[] args)
        {
            try
            {
                AddToLog("Téléchargement de la mise à jour...");

                myWebClient = new WebClient();
                myWebClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
                myWebClient.Headers.Add("Cache-Control", "no-cache");

                myWebClient.DownloadFileAsync(new Uri("http://mapeditor.npdev.eu/downloads/DWE Core.zip"), "update.zip");

                myWebClient.DownloadFileCompleted += (s, e) =>
                {
                    AddToLog("Téléchargement de la mise à jour terminée !");
                    AddToLog("Décompression de la mise à jour...");
                    StartUnpressUpdate();
                };

                Console.ReadLine();

            }
            catch(Exception e)
            {
                AddToLog(e.ToString());
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        static void StartUnpressUpdate()
        {
            try
            {
                ZipInputStream zis = null;
                FileStream fos = null;

                zis = new ZipInputStream(new FileStream("update.zip", FileMode.Open, FileAccess.Read));
                ZipEntry ze;

                while ((ze = zis.GetNextEntry()) != null)
                {
                    if (ze.IsDirectory)
                        Directory.CreateDirectory(ze.Name);
                    else
                    {
                        fos = new FileStream(ze.Name, FileMode.Create, FileAccess.Write);

                        int count;
                        byte[] buffer = new byte[4096];

                        while ((count = zis.Read(buffer, 0, 4096)) > 0)
                            fos.Write(buffer, 0, count);
                    }
                }

                if (zis != null)
                    zis.Close();

                if (fos != null)
                    fos.Close();

                if (System.IO.File.Exists("update.zip"))
                    System.IO.File.Delete("update.zip");

                AddToLog("Décompression de la mise à jour terminée !");
                AddToLog("Redémarrage du programe ...");
                myWebClient.Dispose();

                System.Diagnostics.Process.Start("DofusWorldEditor.exe");
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                AddToLog(e.ToString());
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        static void AddToLog(string text)
        {
            Console.WriteLine(text);
        }
    }
}
