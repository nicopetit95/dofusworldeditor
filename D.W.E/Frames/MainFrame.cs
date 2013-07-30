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

namespace DWE.MapEditor.Frames
{
    public partial class MainFrame : Form
    {
        public List<MapFrame> maps = new List<MapFrame>();

        public bool isEditing = true;

        public bool modifyLevel1 = true;
        public bool modifyLevel2 = false;
        public bool modifyLevel3 = false;

        public bool viewLevel1 = true;
        public bool viewLevel2 = true;
        public bool viewLevel3 = true;

        public bool viewGrid = false;
        public bool viewCellID = false;
        public bool viewBackGround = true;

        public bool viewLoS = false;
        public bool viewWalkable = false;
        public bool viewCellFight1 = false;
        public bool viewCellFight2 = false;

        public bool mustTileFlip = false;
        public bool mustTileRot1 = false;
        public bool mustTileRot2 = false;
        public bool mustTileRot3 = false;
        public bool mustNextInterractif = false;

        public bool onInitialise = false;

        private string lastWay = string.Empty;

        public MainFrame()
        {
            InitializeComponent();
        }

        public int CurrentLevel
        {
            get
            {
                if (modifyLevel1 == true)
                    return 1;
                else if (modifyLevel2 == true)
                    return 2;
                else if (modifyLevel3 == true)
                    return 3;
                else
                    return 1;
            }
        }
        public void AddMap(string path, string key, bool swf = true)
        {
            try
            {
                MapFrame form;

                if (maps.Count > 0)
                    form = new MapFrame(maps.OrderByDescending(x => x.ID).ToArray()[0].ID + 1, path, key, swf);
                else
                    form = new MapFrame(10000, path, key, swf);

                form.Location = new Point((maps.Count + 1) * 5, (maps.Count + 1) * 5);

                form.TopLevel = false;
                maps.Add(form);
                panel3.Controls.Add(form);
                form.Visible = true;

                form.RefreshMap(true);
            }
            catch { }
        }
        public void SendMessage(string message, string titel)
        {
            this.Invoke(new Action(delegate()
                {
                    MessageBox.Show(message, titel);
                }));
        }
        private void MainFrame_Load(object sender, EventArgs e)
        {
            try
            {
                foreach (string dir in Program.Directories.Keys)
                {
                    treeView1.Nodes.Add(dir);

                    foreach (string item in Program.Directories[dir])
                        treeView1.Nodes[Program.Directories.Keys.ToList().IndexOf(dir)].Nodes.Add(item);
                }
                noneToolStripMenuItem.Checked = true;
                lastWay = "Sols";

                gridToolStripMenuItem.Checked = viewGrid;
                level1ToolStripMenuItem.Checked = viewLevel1;
                level2ToolStripMenuItem.Checked = viewLevel2;
                level3ToolStripMenuItem.Checked = viewLevel3;
                backgroundToolStripMenuItem.Checked = viewBackGround;

                toolStripButton1.Checked = true;
                toolStripButton3.Checked = true;

                listView1.MultiSelect = false;

                var frame = new EntryFrame();
                frame.TopLevel = false;
                panel3.Controls.Add(frame);
                frame.Location = new Point((panel3.Width - frame.Width) / 2, (panel3.Height - frame.Height) / 2);
                frame.Show();

                //CL.Web.GetDecalTilesPerThread();
                onInitialise = true;
            }
            catch { }
        }

        public void RefreshMaps(bool force = false)
        {
            maps.ForEach(x => x.RefreshMap(force));
        }
        private void RefreshLoSs()
        {
            maps.ForEach(x => x.RefreshCellsLoS());
        }
        private void RefreshWalks()
        {
            maps.ForEach(x => x.RefreshCellsWalk());
        }
        public void RefreshPreview()
        {
            maps.ForEach(x => x.UpdateFromExPreview(new MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 1, 1, 1, 1), true));
        }
        private void RefreshTeam1()
        {
            maps.ForEach(x => x.RefreshCellTeam1());
        }
        private void RefreshTeam2()
        {
            maps.ForEach(x => x.RefreshCellTeam2());
        }
        private void RefreshTiles()
        {
            try
            {
                if (listView1.LargeImageList != null)
                    listView1.LargeImageList.Dispose();

                listView1.Clear();

                ImageList list = new ImageList();
                list.ImageSize = new System.Drawing.Size(48, 48);
                list.ColorDepth = ColorDepth.Depth32Bit;

                var i = 0;

                treeView1.Enabled = false;

                foreach (var path in System.IO.Directory.GetDirectories(@"./input/tiles\" + treeView1.SelectedNode.FullPath.Replace(@"\\", @"\")))
                {
                    var name = path.Split('\\')[path.Split('\\').Count() - 1];
                    
                    if (name.Split('.').Count() < 2)
                    {
                        ListViewItem item = new ListViewItem(name, i);
                        i++;

                        var I = DWE.MapEditor.Properties.Resources.directory;

                        list.Images.Add(I);
                        listView1.Items.Add(item);
                    }
                }

                foreach (var path in System.IO.Directory.GetFiles(@"./input/tiles\" + treeView1.SelectedNode.FullPath.Replace(@"\\", @"\")))
                {
                    var name = path.Split('\\')[path.Split('\\').Count() - 1];

                    if (name.Split('.').Count() >= 2)
                    {
                        ListViewItem item = new ListViewItem(name.Split('.')[0], i);
                        i++;

                        if (name.Contains(".png") || name.Contains(".jpg") || name.Contains(".jpeg"))
                        {
                            list.Images.Add(Image.FromFile(("./input/tiles/" + treeView1.SelectedNode.FullPath + "/" + name).Replace("\\", "/")));
                            listView1.Items.Add(item);
                        }
                    }
                }

                listView1.LargeImageList = list;

                treeView1.Enabled = true;
            }
            catch { }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                if (treeView1.SelectedNode.FullPath.Contains("Sols"))
                {
                    lastWay = "Sols";
                    toolStripButton3.Enabled = true;
                    toolStripButton4.Enabled = false;
                    toolStripButton5.Enabled = false;
                    toolStripButton3_Click(sender, e);
                }
                else
                {
                    lastWay = "Objets";
                    toolStripButton4.Enabled = true;
                    toolStripButton5.Enabled = true;
                    toolStripButton3.Enabled = false;
                    toolStripButton4_Click(sender, e);
                }

                RefreshTiles();
            }
            catch { }
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!toolStripButton1.Checked)
                {
                    toolStripButton1.Checked = true;
                    toolStripButton2.Checked = false;

                    isEditing = true;
                }
            }
            catch { }
        }
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            try
            {
                if (!toolStripButton2.Checked)
                {
                    toolStripButton2.Checked = true;
                    toolStripButton1.Checked = false;

                    isEditing = false;
                }
            }
            catch { }
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            try
            {
                if (!toolStripButton3.Checked)
                {
                    toolStripButton3.Checked = true;
                    modifyLevel1 = true;
                    modifyLevel2 = false;
                    modifyLevel3 = false;

                    toolStripButton4.Checked = !modifyLevel1;
                    toolStripButton5.Checked = !modifyLevel1;
                }
            }
            catch { }
        }
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            try
            {
                if (!toolStripButton4.Checked && lastWay.Contains("Sols") == false)
                {
                    toolStripButton4.Checked = true;
                    modifyLevel2 = true;
                    modifyLevel1 = false;
                    modifyLevel3 = false;

                    toolStripButton3.Checked = !modifyLevel2;
                    toolStripButton5.Checked = !modifyLevel2;
                }
            }
            catch { }
        }
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            try
            {
                if (!toolStripButton5.Checked && lastWay.Contains("Sols") == false)
                {
                    toolStripButton5.Checked = true;
                    modifyLevel2 = false;
                    modifyLevel1 = false;
                    modifyLevel3 = true;

                    toolStripButton3.Checked = !modifyLevel3;
                    toolStripButton4.Checked = !modifyLevel3;
                }
            }
            catch { }
        }
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (listView1.SelectedItems.Count < 1)
                    return;

                ListViewItem l = listView1.SelectedItems[0];

                if (System.IO.Directory.Exists("./input/tiles/" + treeView1.SelectedNode.FullPath.Replace("\\", "/") + "/" + l.Text))
                {

                    if (listView1.LargeImageList != null)
                        listView1.LargeImageList.Dispose();

                    listView1.Clear();

                    ImageList list = new ImageList();
                    list.ImageSize = new System.Drawing.Size(48, 48);

                    var i2 = 0;

                    treeView1.Enabled = false;

                    foreach (var path in System.IO.Directory.GetFiles("./input/tiles/" + treeView1.SelectedNode.FullPath.Replace("\\", "/") + "/" + l.Text))
                    {
                        var name = path.Split('\\')[path.Split('\\').Count() - 1];

                        if (name.Split('.').Count() >= 2)
                        {
                            ListViewItem item = new ListViewItem(name.Split('.')[0], i2);
                            i2++;

                            if (name.Contains(".png") || name.Contains(".jpg") || name.Contains(".jpeg"))
                            {
                                list.Images.Add(Image.FromFile(("./input/tiles/" + treeView1.SelectedNode.FullPath + "/" + l.Text + "/" + name).Replace("\\", "/")));
                                listView1.Items.Add(item);
                            }
                        }
                    }

                    listView1.LargeImageList = list;

                    treeView1.Enabled = true;
                }
            }
            catch { }
        }
        private void iFormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
        private void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                viewGrid = !viewGrid;
                gridToolStripMenuItem.Checked = viewGrid;
            }
            catch { }

            RefreshMaps();
        }
        private void backgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                viewBackGround = !viewBackGround;
                backgroundToolStripMenuItem.Checked = viewBackGround;
            }
            catch { }

            RefreshMaps();
        }
        private void level1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                viewLevel1 = !viewLevel1;
                level1ToolStripMenuItem.Checked = viewLevel1;
            }
            catch { }

            RefreshMaps();
        }
        private void level2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                viewLevel2 = !viewLevel2;
                level2ToolStripMenuItem.Checked = viewLevel2;
            }
            catch { }

            RefreshMaps();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        private void level3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                viewLevel3 = !viewLevel3;
                level3ToolStripMenuItem.Checked = viewLevel3;
            }
            catch { }

            RefreshMaps();
        }
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            try
            {
                MapFrame form;

                if (maps.Count > 0)
                    form = new MapFrame(maps.OrderByDescending(x => x.ID).ToArray()[0].ID + 1);
                else
                    form = new MapFrame(10000);

                form.Location = new Point((maps.Count + 1) * 5, (maps.Count + 1) * 5);

                form.TopLevel = false;
                maps.Add(form);
                panel3.Controls.Add(form);
                form.Visible = true;

                form.RefreshMap(true);
            }
            catch { }
        }

        private void ResizeAllElements(object sender, EventArgs e)
        {
            try
            {
                panel3.Size = new Size(this.Width - 40, this.Height - 55 - 140 - 28);

                panel2.Size = new Size(this.Width - 40, 140);
                panel2.Location = new Point(12, panel3.Size.Height + 34);

                listView1.Size = new Size(panel2.Width - 156, 140);
            }
            catch { }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            RefreshMaps();
        }
        private void noneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (noneToolStripMenuItem.Checked == false)
                {
                    viewLoS = false;
                    viewWalkable = false;
                    viewCellFight1 = false;
                    viewCellFight2 = false;

                    noneToolStripMenuItem.Checked = true;
                    waiToolStripMenuItem.Checked = false;
                    ligneDeVueToolStripMenuItem.Checked = false;
                    fightCellsTeam1ToolStripMenuItem.Checked = false;
                    fightCellTeam0ToolStripMenuItem.Checked = false;

                    RefreshMaps();
                }
            }
            catch { }
        }
        private void waiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!viewWalkable)
                {
                    viewWalkable = true;
                    viewLoS = false;
                    viewCellFight1 = false;
                    viewCellFight2 = false;

                    waiToolStripMenuItem.Checked = true;
                    ligneDeVueToolStripMenuItem.Checked = false;
                    noneToolStripMenuItem.Checked = false;
                    fightCellsTeam1ToolStripMenuItem.Checked = false;
                    fightCellTeam0ToolStripMenuItem.Checked = false;

                    RefreshWalks();
                    RefreshMaps();
                }
            }
            catch { }
        }
        private void ligneDeVueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!viewLoS)
                {
                    viewLoS = true;
                    viewWalkable = false;
                    viewCellFight1 = false;
                    viewCellFight2 = false;

                    waiToolStripMenuItem.Checked = false;
                    noneToolStripMenuItem.Checked = false;
                    ligneDeVueToolStripMenuItem.Checked = true;
                    fightCellsTeam1ToolStripMenuItem.Checked = false;
                    fightCellTeam0ToolStripMenuItem.Checked = false;

                    RefreshLoSs();
                    RefreshMaps();
                }
            }
            catch { }
        }

        private void Apply_KeysFunctions(object sender, KeyPressEventArgs e)
        {
            try
            {
                var myKey = (Keys)e.KeyChar;

                if (myKey == Keys.F3)
                {
                    //LETTER : r

                    if (!mustTileFlip && !mustTileRot1 && !mustTileRot2 && !mustTileRot3)
                    {
                        toolStripLabel4.Text = "Flip";
                        mustTileFlip = true;
                    }
                    else if (mustTileFlip)
                    {
                        toolStripLabel4.Text = "180°None";
                        mustTileFlip = false;
                        mustTileRot2 = true;
                    }
                    else if (mustTileRot2)
                    {
                        toolStripLabel4.Text = "180°X";
                        mustTileRot2 = false;
                        mustTileRot3 = true;
                    }
                    else if (mustTileRot3)
                    {
                        toolStripLabel4.Text = "Aucune";
                        mustTileRot3 = false;
                    }
                }
                else if (myKey == Keys.NumPad9)
                {
                    //LETTER : i

                    if (!mustNextInterractif)
                    {
                        toolStripLabel6.Text = "Objet intéractif";
                        mustNextInterractif = true;
                    }
                    else
                    {
                        toolStripLabel6.Text = "Aucune";
                        mustNextInterractif = false;
                    }
                }

                maps.ForEach(x => x.UpdateFromExPreview(new MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 1, 1, 1, 1), true));
            }
            catch { }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                Program.OptionsFrame.Show();
                Program.OptionsFrame.UpdateForm();
            }
            catch { }
        }
        private void carteDOFUSswfToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFrame open = new OpenFrame();
                open.Show();
            }
            catch { }
        }
        private void fightCellTeam0ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!viewCellFight1)
                {
                    viewWalkable = false;
                    viewLoS = false;
                    viewCellFight1 = true;
                    viewCellFight2 = false;

                    waiToolStripMenuItem.Checked = false;
                    ligneDeVueToolStripMenuItem.Checked = false;
                    noneToolStripMenuItem.Checked = false;
                    fightCellsTeam1ToolStripMenuItem.Checked = false;
                    fightCellTeam0ToolStripMenuItem.Checked = true;

                    RefreshTeam1();
                    RefreshMaps();
                }
            }
            catch { }
        }
        private void fightCellsTeam1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!viewCellFight2)
                {
                    viewWalkable = false;
                    viewLoS = false;
                    viewCellFight2 = true;
                    viewCellFight1 = false;

                    waiToolStripMenuItem.Checked = false;
                    ligneDeVueToolStripMenuItem.Checked = false;
                    noneToolStripMenuItem.Checked = false;
                    fightCellsTeam1ToolStripMenuItem.Checked = true;
                    fightCellTeam0ToolStripMenuItem.Checked = false;

                    RefreshTeam2();
                    RefreshMaps();
                }
            }
            catch { }
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            try
            {
                var frame = new TilesManager();
                frame.Show();
            }
            catch { }
        }
    }
}
