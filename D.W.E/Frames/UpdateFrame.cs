using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DWE.MapEditor.Frames
{
    public partial class UpdateFrame : Form
    {
        public UpdateFrame()
        {
            InitializeComponent();
        }

        private void willClose(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void ticktick(object sender, EventArgs e)
        {
            CL.Web.myWebClient = new System.Net.WebClient();
            CL.Web.myWebClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
            CL.Web.myWebClient.Headers.Add("Cache-Control", "no-cache");

            CL.Web.myWebClient.DownloadFileAsync(new Uri("http://mapeditor.npdev.eu/downloads/Updater.exe"), "Updater.exe");

            CL.Web.myWebClient.DownloadProgressChanged += (s, ex) =>
            {
                this.progressBar1.Invoke(
                new Action(
                delegate()
                {
                    progressBar1.Value = ex.ProgressPercentage;
                }));
            };

            CL.Web.myWebClient.DownloadFileCompleted += (s, ex) =>
            {
                System.Diagnostics.Process.Start("Updater.exe");
                this.Close();
            };

            CL.Web.myWebClient.Dispose();
        }
    }
}
