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
    public partial class Options : Form
    {
        public Options()
        {
            InitializeComponent();
        }

        public bool InsertAutoBDD = false;
        public bool mustCopyAutoSQL = false;
        public bool mustCopyAutoSWF = false;
        public bool mustCopyServerSWF = false;
        public bool mustCopyAutoDM = false;

        public bool mustRightClicShowMenu = true;
        public bool mustRightClicRemoveTile = false;

        public string HostBDD = "";
        public string UserBDD = "";
        public string PwdBDD = "";
        public string DatabaseBDD = "";

        public string LinkSWF = "";
        public string LinkSQL = "";
        public string LinkDM = "";
        public string LinkCopySWF = "";

        public void UpdateForm()
        {
            try
            {
                checkBox1.Checked = InsertAutoBDD;
                groupBox1.Enabled = InsertAutoBDD;

                checkBox3.Checked = mustCopyAutoSQL;
                groupBox3.Enabled = mustCopyAutoSQL;

                checkBox2.Checked = mustCopyAutoSWF;
                groupBox2.Enabled = mustCopyAutoSWF;

                checkBox4.Checked = mustCopyAutoDM;
                groupBox4.Enabled = mustCopyAutoDM;

                checkBox5.Checked = mustCopyServerSWF;
                button6.Enabled = mustCopyServerSWF;
                textBox8.Enabled = mustCopyServerSWF;

                textBox5.Text = LinkSWF;
                textBox6.Text = LinkSQL;
                textBox7.Text = LinkDM;

                textBox1.Text = HostBDD;
                textBox2.Text = UserBDD;
                textBox3.Text = PwdBDD;
                textBox4.Text = DatabaseBDD;

                textBox8.Text = LinkCopySWF;

                checkBox6.Checked = mustRightClicShowMenu;
                checkBox7.Checked = mustRightClicRemoveTile;
            }
            catch { }
        }

        private void willClose(object sender, FormClosingEventArgs e)
        {
            try
            {
                checkBox1.Checked = InsertAutoBDD;
                groupBox1.Enabled = InsertAutoBDD;

                checkBox3.Checked = mustCopyAutoSQL;
                groupBox3.Enabled = mustCopyAutoSQL;

                checkBox2.Checked = mustCopyAutoSWF;
                groupBox2.Enabled = mustCopyAutoSWF;

                checkBox4.Checked = mustCopyAutoDM;
                groupBox4.Enabled = mustCopyAutoDM;

                checkBox5.Checked = mustCopyServerSWF;
                button6.Enabled = mustCopyServerSWF;
                textBox8.Enabled = mustCopyServerSWF;

                textBox5.Text = LinkSWF;
                textBox6.Text = LinkSQL;
                textBox7.Text = LinkDM;

                textBox1.Text = HostBDD;
                textBox2.Text = UserBDD;
                textBox3.Text = PwdBDD;
                textBox4.Text = DatabaseBDD;

                textBox8.Text = LinkCopySWF;

                checkBox6.Checked = mustRightClicShowMenu;
                checkBox7.Checked = mustRightClicRemoveTile;
            }
            catch { }

            e.Cancel = true;
            this.Hide();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            groupBox1.Enabled = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            groupBox2.Enabled = checkBox2.Checked;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                InsertAutoBDD = checkBox1.Checked;
                mustCopyAutoDM = checkBox4.Checked;
                mustCopyAutoSWF = checkBox2.Checked;
                mustCopyAutoSQL = checkBox3.Checked;
                mustCopyServerSWF = checkBox5.Checked;

                HostBDD = textBox1.Text;
                UserBDD = textBox2.Text;
                PwdBDD = textBox3.Text;
                DatabaseBDD = textBox4.Text;

                LinkSWF = textBox5.Text;
                LinkSQL = textBox6.Text;
                LinkDM = textBox7.Text;
                LinkCopySWF = textBox8.Text;

                mustRightClicRemoveTile = checkBox7.Checked;
                mustRightClicShowMenu = checkBox6.Checked;

                CL.Configuration.SaveConfiguration();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Dossier de génération SWF";
                    dialog.ShowNewFolderButton = true;
                    dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        textBox5.Text = (dialog.SelectedPath);
                    }
                }
            }
            catch { }
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            groupBox3.Enabled = checkBox3.Checked;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            groupBox4.Enabled = checkBox4.Checked;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Dossier de génération DM";
                    dialog.ShowNewFolderButton = true;
                    dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        textBox7.Text = (dialog.SelectedPath);
                    }
                }
            }
            catch { }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Dossier de génération SQL";
                    dialog.ShowNewFolderButton = true;
                    dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        textBox6.Text = (dialog.SelectedPath);
                    }
                }
            }
            catch { }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                textBox8.Enabled = checkBox5.Checked;
                button6.Enabled = checkBox5.Checked;
            }
            catch { }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    dialog.Description = "Dossier de génération SWF";
                    dialog.ShowNewFolderButton = true;
                    dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        textBox8.Text = (dialog.SelectedPath);
                    }
                }
            }
            catch { }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                checkBox7.Checked = !checkBox6.Checked;
                mustRightClicRemoveTile = false;
                mustRightClicShowMenu = true;
            }
            catch { }
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                checkBox6.Checked = !checkBox7.Checked;
                mustRightClicRemoveTile = true;
                mustRightClicShowMenu = false;
            }
            catch { }
        }
    }
}
