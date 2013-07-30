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
    public partial class TilesManager : Form
    {
        public TilesManager()
        {
            InitializeComponent();

            textBox1.Enabled = false;
        }

        private string directory = "";

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                listBox2.Items.Clear();

                if (listBox1.SelectedItems.Count < 1)
                    return;

                directory = (listBox1.SelectedIndex == 0 ? "Sols" : "Objets");

                if (!CL.Configuration.decalTiles.ContainsKey(directory))
                    return;

                foreach (CL.Configuration.DecalTile tile in CL.Configuration.decalTiles[directory].OrderBy(x => x.ID))
                    listBox2.Items.Add(tile.ID.ToString());
            }
            catch { }
        }

        private void TilesManager_Load(object sender, EventArgs e)
        {

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (listBox2.SelectedItems.Count < 1)
                    return;

                if (CL.Configuration.decalTiles[directory].Any(x => x.ID.ToString() == listBox2.SelectedItem.ToString()))
                {
                    var tile = CL.Configuration.decalTiles[directory].First(x => x.ID.ToString() == listBox2.SelectedItem.ToString());

                    textBox1.Text = tile.ID.ToString();
                    textBox2.Text = tile.Decalages.X.ToString();
                    textBox3.Text = tile.Decalages.Y.ToString();
                }
            }
            catch { }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                listBox1.Items.Clear();
                listBox1.Items.Add("Sols");
                listBox1.Items.Add("Objets");

                listBox2.Items.Clear();
            }
            catch { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                button2.Enabled = false;
                timer1.Enabled = true;
                timer1.Start();

                this.Close();
            }
            catch { }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            timer1.Stop();

            button2.Enabled = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBox2.SelectedItems.Count < 1)
                    return;

                if (CL.Configuration.decalTiles[directory].Any(x => x.ID.ToString() == listBox2.SelectedItem.ToString()))
                {
                    var tile = CL.Configuration.decalTiles[directory].First(x => x.ID.ToString() == listBox2.SelectedItem.ToString());

                    tile.Decalages.X = int.Parse(textBox2.Text);
                    tile.Decalages.Y = int.Parse(textBox3.Text);

                    Program.MainFrame.RefreshMaps(true);
                }
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBox2.SelectedItems.Count < 1)
                    return;

                if (CL.Configuration.decalTiles[directory].Any(x => x.ID.ToString() == listBox2.SelectedItem.ToString()))
                {
                    var tile = CL.Configuration.decalTiles[directory].First(x => x.ID.ToString() == listBox2.SelectedItem.ToString());
                    CL.Configuration.decalTiles[directory].Remove(tile);

                    listBox1.Items.Clear();
                    listBox1.Items.Add("Sols");
                    listBox1.Items.Add("Objets");

                    listBox2.Items.Clear();

                    textBox1.Text = "";
                    textBox2.Text = "";
                    textBox3.Text = "";

                    Program.MainFrame.RefreshMaps(true);
                }
            }
            catch { }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                if (!listBox2.Items.Contains(textBox4.Text) && textBox4.Text != "")
                {
                    var nID = 0;
                    var nX = 0;
                    var nY = 0;

                    if (int.TryParse(textBox4.Text, out nID))
                    {
                        if (int.TryParse(textBox5.Text, out nX))
                        {
                            if (int.TryParse(textBox6.Text, out nY))
                            {
                                listBox2.Items.Add(textBox4.Text);
                                CL.Configuration.decalTiles[directory].Add(new CL.Configuration.DecalTile()
                                {
                                    ID = nID,
                                    Decalages = new Point(nX, nY)
                                });
                            }
                        }
                    }

                    listBox1.Items.Clear();
                    listBox1.Items.Add("Sols");
                    listBox1.Items.Add("Objets");

                    listBox2.Items.Clear();

                    textBox4.Text = "";
                    textBox5.Text = "";
                    textBox6.Text = "";

                    Program.MainFrame.RefreshMaps(true);
                }
            }
            catch { }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                textBox2.Text = (int.Parse(textBox2.Text) - 10).ToString();
            }
            catch {}
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            try
            {
                textBox3.Text = (int.Parse(textBox3.Text) + 10).ToString();
            }
            catch { }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            try
            {
                textBox2.Text = (int.Parse(textBox2.Text) + 10).ToString();
            }
            catch { }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            try
            {
                textBox3.Text = (int.Parse(textBox3.Text) - 10).ToString();
            }
            catch { }
        }
    }
}
