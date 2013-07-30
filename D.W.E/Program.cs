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

namespace DWE.MapEditor
{
    static class Program
    {
        public static Dictionary<string, List<string>> Directories;
        public static Dictionary<string, List<string>> Tiles;
        public const string Hash = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";

        public static Frames.MainFrame MainFrame;
        public static Frames.Options OptionsFrame;
        public static Frames.LoadingFrame LoadingFrame;

        [STAThread]
        static void Main()
        {
            Directories = new Dictionary<string, List<string>>();
            Tiles = new Dictionary<string, List<string>>();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainFrame = new Frames.MainFrame();
            OptionsFrame = new Frames.Options();
            LoadingFrame = new Frames.LoadingFrame();

            Application.Run(LoadingFrame);
        }
    }
}