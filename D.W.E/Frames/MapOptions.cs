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
    public partial class MapOptions : Form
    {
        public MapFrame FormOwner;
        private string groundPath;

        public MapOptions(MapFrame owner)
        {
            InitializeComponent();

            FormOwner = owner;

            try
            {
                textBox1.Text = FormOwner.ID.ToString();

                textBox2.Text = FormOwner.continent;
                textBox5.Text = FormOwner.signature;

                textBox3.Text = FormOwner.height.ToString();
                textBox3.Enabled = false;

                textBox4.Text = FormOwner.width.ToString();
                textBox4.Enabled = false;

                if (FormOwner.background != null)
                {
                    Bitmap last = new Bitmap(Image.FromFile(FormOwner.background));
                    Size newSize = new System.Drawing.Size(100, 100);
                    Bitmap result = new Bitmap(newSize.Width, newSize.Height);

                    using (Graphics g = Graphics.FromImage((Image)result))
                        g.DrawImage(last, 0, 0, newSize.Width, newSize.Height);

                    pictureBox1.Image = result;
                    groundPath = FormOwner.background;
                }

                checkBox1.Checked = FormOwner.challengeAuto;
                checkBox2.Checked = FormOwner.agroAuto;
                checkBox3.Checked = FormOwner.saveAuto;
                checkBox4.Checked = FormOwner.telepAuto;
                checkBox5.Checked = FormOwner.externMap;
            }
            catch { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                int newID;
                if (!int.TryParse(textBox1.Text.Trim(), out newID))
                    throw new Exception("Merci de bien vouloir rentrer un nombre valide !");

                FormOwner.ID = newID;
                FormOwner.Text = newID.ToString();
                FormOwner.continent = (textBox2.Text == "" ? "Aucun" : textBox2.Text);
                FormOwner.ChangeGround(groundPath);

                FormOwner.challengeAuto = checkBox1.Checked;
                FormOwner.agroAuto = checkBox2.Checked;
                FormOwner.saveAuto = checkBox3.Checked;
                FormOwner.telepAuto = checkBox4.Checked;
                FormOwner.externMap = checkBox5.Checked;
                FormOwner.signature = textBox5.Text;

                this.Close();
            }
            catch (Exception te)
            {
                MessageBox.Show(te.ToString());
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog path = new OpenFileDialog();
                path.Multiselect = false;

                path.InitialDirectory = Environment.CurrentDirectory + @"\input\backgrounds";
                path.RestoreDirectory = true;
                path.Filter = "Images (*.png or *.jpg or *.bmp)|*.png;*.jpg;*.bmp";

                path.ShowDialog();
                var selectedFile = path.FileName;

                if (selectedFile == string.Empty)
                    return;

                Bitmap last = new Bitmap(Image.FromFile(selectedFile));
                Size newSize = new System.Drawing.Size(100, 100);
                Bitmap result = new Bitmap(newSize.Width, newSize.Height);

                using (Graphics g = Graphics.FromImage((Image)result))
                    g.DrawImage(last, 0, 0, newSize.Width, newSize.Height);

                pictureBox1.Image = result;
                groundPath = selectedFile;
            }
            catch { }
        }

        private void MapOptions_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            groundPath = null;
            pictureBox1.Image = null;
        }
    }
}
