using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace DWE.MapEditor.Frames
{
    public partial class LoadingFrame : Form
    {
        bool isTimed = false;
        delegate void ChangeHideValue(bool t);
        delegate void MainFormShow();
        bool hasUpdate = false;
        bool initialized = false;

        public LoadingFrame()
        {
            InitializeComponent();
        }

        private void LoadDirectories()
        {
            try
            {
                if (!System.IO.Directory.Exists("./input"))
                    System.IO.Directory.CreateDirectory("./input/");

                if (!System.IO.Directory.Exists("./input/tiles"))
                    System.IO.Directory.CreateDirectory("./input/tiles/");

                foreach (var dir in System.IO.Directory.GetDirectories("./input/tiles").OrderByDescending(x => x))
                {
                    var dirName = dir.Replace(@"./input/tiles\", "");
                    Program.Directories.Add(dirName, new List<string>());

                    var toReplace = "./input/tiles\\" + dirName + "\\";

                    foreach (string direc in System.IO.Directory.GetDirectories(dir))
                        Program.Directories[dirName].Add(direc.Replace(toReplace, ""));
                }
            }
            catch { }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (isTimed)
            {
                timer1.Stop();
                return;
            }

            isTimed = true;

            CL.Configuration.LoadConfiguration();
            System.Threading.Thread T = new System.Threading.Thread(new System.Threading.ThreadStart(this.RefreshAll));
            T.Start();
        }

        private void RefreshAll()
        {
            if (CL.Web.HasUpdate())
            {
                Invoke((MainFormShow)InvokeUpdate);
                hasUpdate = true;
            }

            LoadDirectories();
            CL.Manager.LoadTiles();

            if (!hasUpdate)
            {
                Invoke((ChangeHideValue)(RefreshHide), true);
                Invoke((MainFormShow)InvokeMainForm);
            }
        }

        private void RefreshHide(bool newValue)
        {
            if (newValue)
                this.Hide();
        }

        private void InvokeMainForm()
        {
            try
            {
                Program.MainFrame.Show();
            }
            catch { Environment.Exit(0); }
        }

        private void InvokeUpdate()
        {
            try
            {
                var result = MessageBox.Show("Une nouvelle version de l'éditeur (" + CL.Web.NewVersion + ") est disponible, voulez vous la télécharger ?", "Mise à jour", MessageBoxButtons.YesNo);

                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    var frame = new UpdateFrame();
                    frame.Show();

                    this.Hide();
                }
                else
                {
                    LoadDirectories();
                    CL.Manager.LoadTiles();

                    this.Hide();
                    InvokeMainForm();
                }
            }
            catch { Environment.Exit(0); }
        }
    }
}
