using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace DWE.MapEditor.Frames
{
    public partial class OpenFrame : Form
    {
        public OpenFrame()
        {
            InitializeComponent();
        }

        private void OpenFrame_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();

                openFileDialog1.InitialDirectory = Environment.SpecialFolder.MyComputer.ToString();
                openFileDialog1.Filter = "carte DOFUS (*.swf or *.dm)|*.swf;*.dm";
                openFileDialog1.FilterIndex = 2;
                openFileDialog1.RestoreDirectory = true;
                openFileDialog1.Multiselect = false;

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    textBox1.Text = openFileDialog1.FileName;
            }
            catch { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            Program.MainFrame.AddMap(textBox1.Text.Trim(), textBox2.Text.Trim(), (textBox1.Text.Contains(".swf") ? true : false));

            this.Close();
        }
    }
}
