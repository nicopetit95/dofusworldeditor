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
using System.IO;
using System.Diagnostics;
using System.Threading;
using DWE.MapEditor.CL;

namespace DWE.MapEditor.Frames
{
    public partial class MapFrame : Form
    {
        public MapFrame(int id, bool big = false)
        {
            InitializeComponent();
            
            ID = id;
            this.Text = id.ToString();
            toolStripMenuItem2.Checked = true;

            if (big)
            {
                width = 19;
                height = 22;
            }
        }
        public MapFrame(int id, string path,string _key, bool swf = true)
        {
            InitializeComponent();

            ID = id;
            this.Text = id.ToString();
            toolStripMenuItem2.Checked = true;

            pathMap = path;
            isSWF = swf;
            key = _key;
        }

        public int ID, width = 15, height = 17;
        private int selectedTileID = -1, lastCellID = -1, screensCount = 0;
        private bool mustUpdateApercu = false, mustLayingTile = false;
        public string continent = "Aucun", background = null, signature = "00000", key = "";
        public string pathMap = "";
        public bool isSWF = true, mustLay = false;
        public object locker = new object();
        private Bitmap preview;

        public int BackID
        {
            get
            {
                if (background == null)
                    return 0;
                else
                {
                    try
                    {
                        return int.Parse(background.Split('\\')[background.Split('\\').Length - 1].Split('.')[0]);
                    }
                    catch { return 0; };
                }
            }
        }

        private Point NewDelecage(Point tile)
        {
            var p = new Point();
            
            var num = (double)(CELL_W_HALF / 26.0);
            var num2 = (double)(CELL_H_HALF / 15.0);

            p.X = (int)Math.Round((double)(tile.X * num));
            p.Y = (int)Math.Round((double)(tile.Y * num2));

            return p;
        }

        public bool challengeAuto = true, agroAuto = true, externMap = true, saveAuto = true, telepAuto = true;

        public Graphics graphGround;
        public Bitmap bmpGround;
        public Graphics graphPreview;
        public Bitmap bmpPreview;
        public Graphics graphCellID;
        public Bitmap bmpCellID;
        public Graphics graphObject1;
        public Bitmap bmpObject1;
        public Graphics graphObject2;
        public Bitmap bmpObject2;
        public Bitmap bmpGrid;
        public Graphics graphGrid;
        public Graphics graphBG;
        public Bitmap bmpBG;
        public Graphics graphLOS;
        public Bitmap bmpLOS;
        public Graphics graphWalk;
        public Bitmap bmpWalk;
        public Graphics graphTeam1;
        public Bitmap bmpTeam1;
        public Graphics graphTeam2;
        public Bitmap bmpTeam2;

        public Dictionary<int, CL.Cell> Cells;
        private ContextMenuStrip cellsInfosMenu;

        public MouseEventArgs lastClic;
        public Point nMousePosition;
        public int collec;
        public int collecSelected;
        public int ground = 0;

        private void MapFrame_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text = ID.ToString() + " (CHARGEMENT...)";

                var t = new System.Threading.Thread(new System.Threading.ThreadStart(this.UpdateMapFromThread));
                t.Start();
            }
            catch { }
        }
        private void UpdateMapFromThread()
        {
            try
            {
                cellsInfosMenu = new ContextMenuStrip();

                lock(locker)
                    InitiliazeGraphics();

                Cells = new Dictionary<int, CL.Cell>();

                if (pathMap != "")
                {
                    if (isSWF)
                    {
                        CL.SwfFile swf = new CL.SwfFile(pathMap);
                        string idS = swf.GetPushValue("id").Replace("'", ""),
                            mS = swf.GetPushValue("musicId").Replace("'", ""),
                            aS = swf.GetPushValue("ambianceId").Replace("'", ""),
                            cS = swf.GetPushValue("capabilities").Replace("'", ""),
                            bS = swf.GetPushValue("backgroundNum").Replace("'", ""),
                            wS = swf.GetPushValue("width").Replace("'", ""),
                            hS = swf.GetPushValue("height").Replace("'", ""),
                            mdS = swf.GetPushValue("mapData").Replace("'", ""),
                            oS = swf.GetPushValue("bOutdoor").Replace("'", "");

                        ID = int.Parse(idS);

                        this.Invoke(new Action(delegate()
                            {
                                this.Text = ID.ToString() + " (CHARGEMENT...)";
                            }));
                                            

                        ParseCapatibilities(int.Parse(cS));

                        if (File.Exists(@".\input\backgrounds\" + int.Parse(bS) + ".png"))
                            background = @".\input\backgrounds\" + int.Parse(bS) + ".png";
                        else if (File.Exists(@".\input\backgrounds\" + int.Parse(bS) + ".bmp"))
                            background = @".\input\backgrounds\" + int.Parse(bS) + ".bmp";
                        else if (File.Exists(@".\input\backgrounds\" + int.Parse(bS) + ".jpg"))
                            background = @".\input\backgrounds\" + int.Parse(bS) + ".jpg";
                        ChangeGround(background);

                        width = int.Parse(wS);
                        height = int.Parse(hS);

                        externMap = (oS == "1" ? true : false);

                        Cells.Clear();

                        string data = DecypherData(mdS, key);
                        for (int i = 0; i < data.Length; i += 10)
                        {
                            string CurrentCell = data.Substring(i, 10);
                            Cells.Add((i / 10) + 1, new CL.Cell((i / 10) + 1, CurrentCell, width, height, this));
                        }

                        this.Invoke(new Action(delegate()
                            {
                                this.Text = ID.ToString() + " (Mise en page...)";
                            }));

                        RefreshMap(true);

                        this.Invoke(new Action(delegate
                        {
                            this.Text = ID.ToString();
                            this.pictureBox1.Enabled = true;
                        }));

                        return;
                    }
                    else
                    {
                        StreamReader reader = new StreamReader(pathMap);
                        string idS = reader.ReadLine().Substring(3),
                            wS = reader.ReadLine().Substring(3),
                            hS = reader.ReadLine().Substring(3),
                            bS = reader.ReadLine().Substring(3),
                            aS = reader.ReadLine().Substring(3),
                            mS = reader.ReadLine().Substring(3),
                            oS = reader.ReadLine().Substring(3),
                            cS = reader.ReadLine().Substring(3),
                            mdS = reader.ReadLine().Substring(3),
                            cellsFight = reader.ReadLine().Substring(3);

                        reader.Close();

                        ID = int.Parse(idS);
                        ParseCapatibilities(int.Parse(cS));

                        if (File.Exists("./input/backgrounds/" + int.Parse(bS) + ".png"))
                            background = "./input/backgrounds/" + int.Parse(bS) + ".png";
                        else if (File.Exists("./input/backgrounds/" + int.Parse(bS) + ".bmp"))
                            background = "./input/backgrounds/" + int.Parse(bS) + ".bmp";
                        else if (File.Exists("./input/backgrounds/" + int.Parse(bS) + ".jpg"))
                            background = "./input/backgrounds/" + int.Parse(bS) + ".jpg";

                        width = int.Parse(wS);
                        height = int.Parse(hS);

                        externMap = (oS == "1" ? true : false);

                        Cells.Clear();

                        string data = DecypherData(mdS, "");//prepareKey integré
                        for (int i = 0; i < data.Length; i += 10)
                        {
                            string CurrentCell = data.Substring(i, 10);
                            Cells.Add((i / 10) + 1, new CL.Cell((i / 10) + 1, CurrentCell, width, height, this));
                        }

                        var cellsteam1 = cellsFight.Split('|')[0];
                        var cellsteam2 = cellsFight.Split('|')[1];

                        foreach (string i in cellsteam1.Split(';'))
                        {
                            if (i == "")
                                continue;

                            var id = int.Parse(i);
                            Cells.Values.First(x => x.id == id).isCellTeam1 = true;
                        }

                        foreach (string i in cellsteam2.Split(';'))
                        {
                            if (i == "")
                                continue;

                            var id = int.Parse(i);
                            Cells.Values.First(x => x.id == id).isCellTeam2 = true;
                        }

                        RefreshMap(true);

                        this.Invoke(new Action(delegate
                        {
                            this.Text = ID.ToString();
                            this.pictureBox1.Enabled = true;
                        }));
                    }
                }
                else
                {
                    InitializeCells();
                    RefreshMap();

                    this.Invoke(new Action(delegate
                    {
                        this.Text = ID.ToString();
                        this.pictureBox1.Enabled = true;
                    }));
                }
            }
            catch { }
        }
        public static string DecypherData(string Data, string DecryptKey)
        {
            try
            {
                string result = string.Empty;
                if (DecryptKey != "")
                {
                    DecryptKey = PrepareKey(DecryptKey);
                    int checkSum = CheckSum(DecryptKey) * 2;
                    for (int i = 0, k = 0; i < Data.Length; i += 2)
                        result += (char)(int.Parse(Data.Substring(i, 2), System.Globalization.NumberStyles.HexNumber) ^
                            (int)(DecryptKey[(k++ + checkSum) % DecryptKey.Length]));
                    return Uri.UnescapeDataString(result);
                }
                else return Data;
            }
            catch { return ""; }
        }
        private static int CheckSum(string Data)
        {
            int result = 0;

            for (int i = 0; i < Data.Length; i++)
                result += Data[i] % 16;

            return result % 16;
        }
        private static string PrepareKey(string Key)
        {
            string keyResult = "";

            for (int i = 0; i < Key.Length; i += 2)
                keyResult += Convert.ToChar(int.Parse(Key.Substring(i, 2), System.Globalization.NumberStyles.HexNumber));

            return Uri.UnescapeDataString(keyResult);
        }
        private void ParseCapatibilities(int nb)
        {
            try
            {
                int decimalNumber = nb;

                int remainder;
                string result = string.Empty;

                if (nb == 0)
                    result = "0000";

                while (decimalNumber > 0)
                {
                    remainder = decimalNumber % 2;
                    decimalNumber /= 2;
                    result = remainder.ToString() + result;
                }

                if (result.ToString().Substring(0, 1) == "1")
                    telepAuto = true;
                if (result.ToString().Substring(1, 1) == "1")
                    saveAuto = true;
                if (result.ToString().Substring(2, 1) == "1")
                    agroAuto = true;
                if (result.ToString().Substring(3, 1) == "1")
                    challengeAuto = true;
            }
            catch { }
        }
        private void ResizeMap(object sender, EventArgs e)
        {
            try
            {
                toolStripMenuItem2.Checked = false;
                toolStripMenuItem3.Checked = false;
                toolStripMenuItem4.Checked = false;

                lock (locker)
                {
                    pictureBox1.Size = new Size(this.Width - 17, this.Height - 63);

                    RefreshBitmaps();

                    graphBG.Clear(Color.Transparent);
                    bmpBG = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphBG = Graphics.FromImage(bmpBG);
                    ChangeGround(background);

                    graphGrid.Clear(Color.Transparent);
                    bmpGrid = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphGrid = Graphics.FromImage(bmpGrid);

                    graphPreview.Clear(Color.Transparent);
                    bmpPreview = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphPreview = Graphics.FromImage(bmpPreview);

                    graphGround.Clear(Color.Transparent);
                    bmpGround = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphGround = Graphics.FromImage(bmpGround);

                    graphObject1.Clear(Color.Transparent);
                    bmpObject1 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphObject1 = Graphics.FromImage(bmpObject1);

                    graphObject2.Clear(Color.Transparent);
                    bmpObject2 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphObject2 = Graphics.FromImage(bmpObject2);

                    int size = 1;
                    Pen p = new Pen(Color.Gray, size);
                    for (int x = 0; x < (int)((1 + width) * (1 + height) / 2); x++)
                    {
                        int X1 = (CELL_W_HALF + x * CELL_W);
                        int Y2 = (CELL_H_HALF + x * CELL_H);
                        graphGrid.DrawLine(p, new Point(X1, 0), new Point(0, Y2));

                        X1 = ((x + 1) * CELL_W);
                        Y2 = (CELL_H_HALF + (height - x) * CELL_H);
                        graphGrid.DrawLine(p, new Point(X1, (CELL_H_HALF + (height + 1) * CELL_H)), new Point(0, Y2));
                    }
                }

                Cells.Values.ToList().ForEach(x => x.pos = x.getPositionByCellID(x.id, 7, width, height));

                RefreshMap(true);
            }
            catch { }
        }
        private Bitmap ResizeBitmap(Bitmap sourceBMP, int width, int height)
        {
            try
            {
                Bitmap result = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(result))
                    g.DrawImage(sourceBMP, 0, 0, width, height);
                return result;
            }
            catch { return null; }
        }
        private void willClose(object sender, FormClosingEventArgs e)
        {
            lock (Program.MainFrame.maps)
                Program.MainFrame.maps.Remove(this);
        }

        public void RefreshMap(bool force = false)
        {
            try
            {
                lock (locker)
                {
                    if (force)
                    {
                        RefreshGroundCells();
                        RefreshObject1Cells();
                        RefreshObject2Cells();
                        RefreshCellsWalk();
                        RefreshCellsLoS();
                        RefreshCellTeam1();
                        RefreshCellTeam2();
                    }

                    Bitmap i = new Bitmap(pictureBox1.Width, pictureBox1.Height);

                    if (Program.MainFrame.viewBackGround)
                        (Graphics.FromImage(i)).DrawImage(bmpBG, new Point());

                    if (Program.MainFrame.viewLevel1)
                        (Graphics.FromImage(i)).DrawImage(bmpGround, new Point());

                    if (Program.MainFrame.CurrentLevel == 1)
                        (Graphics.FromImage(i)).DrawImage(bmpPreview, new Point());

                    if (Program.MainFrame.viewLevel2)
                        (Graphics.FromImage(i)).DrawImage(bmpObject1, new Point());

                    if (Program.MainFrame.CurrentLevel == 2)
                        (Graphics.FromImage(i)).DrawImage(bmpPreview, new Point());

                    if (Program.MainFrame.viewLevel3)
                        (Graphics.FromImage(i)).DrawImage(bmpObject2, new Point());

                    if (Program.MainFrame.CurrentLevel == 3)
                        (Graphics.FromImage(i)).DrawImage(bmpPreview, new Point());

                    if (Program.MainFrame.viewLoS)
                    {
                        (Graphics.FromImage(i)).DrawImage(bmpLOS, new Point());
                        (Graphics.FromImage(i)).DrawImage(bmpPreview, new Point());
                    }

                    if (Program.MainFrame.viewWalkable)
                    {
                        (Graphics.FromImage(i)).DrawImage(bmpWalk, new Point());
                        (Graphics.FromImage(i)).DrawImage(bmpPreview, new Point());
                    }

                    if (Program.MainFrame.viewCellFight1)
                    {
                        (Graphics.FromImage(i)).DrawImage(bmpTeam1, new Point());
                        (Graphics.FromImage(i)).DrawImage(bmpPreview, new Point());
                    }

                    if (Program.MainFrame.viewCellFight2)
                    {
                        (Graphics.FromImage(i)).DrawImage(bmpTeam2, new Point());
                        (Graphics.FromImage(i)).DrawImage(bmpPreview, new Point());
                    }

                    if (Program.MainFrame.viewCellID)
                        (Graphics.FromImage(i)).DrawImage(bmpCellID, new Point());

                    if (Program.MainFrame.viewGrid)
                        (Graphics.FromImage(i)).DrawImage(bmpGrid, new Point());

                    pictureBox1.Image = i;
                }
            }
            catch { }
        }

        public void ChangeGround(string path)
        {
            try
            {
                lock (locker)
                {
                    graphBG.Clear(Color.Transparent);
                    background = path;

                    if (path == null || BackID == 0)
                        return;

                    Bitmap last = new Bitmap(Image.FromFile(path));
                    graphBG.DrawImage(ResizeBitmap(last, pictureBox1.Size.Width, pictureBox1.Size.Height), 0, 0, pictureBox1.Size.Width, pictureBox1.Size.Height);
                }

                RefreshMap();
            }
            catch { }
        }

        private void InitiliazeGraphics()
        {
            try
            {
                lock (locker)
                {
                    graphBG = Graphics.FromImage(new Bitmap(pictureBox1.Width, pictureBox1.Height));
                    bmpGround = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphGround = Graphics.FromImage(bmpGround);

                    bmpPreview = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphPreview = Graphics.FromImage(bmpPreview);

                    bmpObject1 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphObject1 = Graphics.FromImage(bmpObject1);

                    bmpObject2 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphObject2 = Graphics.FromImage(bmpObject2);

                    bmpBG = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphBG = Graphics.FromImage(bmpBG);

                    bmpGrid = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphGrid = Graphics.FromImage(bmpGrid);

                    bmpLOS = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphLOS = Graphics.FromImage(bmpLOS);

                    bmpWalk = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphWalk = Graphics.FromImage(bmpWalk);

                    bmpTeam1 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphTeam1 = Graphics.FromImage(bmpTeam1);

                    bmpTeam2 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphTeam2 = Graphics.FromImage(bmpTeam2);

                    bmpCellID = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphCellID = Graphics.FromImage(bmpCellID);

                    graphBG.SmoothingMode = SmoothingMode.HighQuality;
                    graphPreview.SmoothingMode = SmoothingMode.HighQuality;
                    graphGround.SmoothingMode = SmoothingMode.HighQuality;
                    graphGrid.SmoothingMode = SmoothingMode.HighQuality;
                    graphObject1.SmoothingMode = SmoothingMode.HighQuality;
                    graphObject2.SmoothingMode = SmoothingMode.HighQuality;
                    graphLOS.SmoothingMode = SmoothingMode.HighQuality;
                    graphWalk.SmoothingMode = SmoothingMode.HighQuality;
                    graphTeam1.SmoothingMode = SmoothingMode.HighQuality;
                    graphTeam2.SmoothingMode = SmoothingMode.HighQuality;
                    graphCellID.SmoothingMode = SmoothingMode.HighQuality;

                    int size = 1;
                    Pen p = new Pen(Color.Gray, size);
                    for (int x = 0; x < (int)((1 + width) * (1 + height) / 2); x++)
                    {
                        int X1 = (int)(CELL_W_HALF + x * CELL_W);
                        int Y2 = (int)(CELL_H_HALF + x * CELL_H);
                        graphGrid.DrawLine(p, new Point(X1, 0), new Point(0, Y2));

                        X1 = (int)((x + 1) * CELL_W);
                        Y2 = (int)(CELL_H_HALF + (height - x) * CELL_H);
                        graphGrid.DrawLine(p, new Point(X1, (int)(CELL_H_HALF + (height + 1) * CELL_H)), new Point(0, Y2));
                    }
                }
            }
            catch { }
        }
        private void InitializeCells()
        {
            int maxCellID = width * height + (width - 1) * (height - 1);

            for (int a = 1; a <= maxCellID; a++)
                Cells.Add(a, new CL.Cell(a, width, height, this));
        }
        public void RefreshCellsLoS()
        {
            try
            {
                lock (locker)
                {
                    graphLOS.Clear(Color.Transparent);
                    bmpLOS = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphLOS = Graphics.FromImage(bmpLOS);

                    foreach (CL.Cell DC in Cells.Values)
                    {
                        Color c;

                        if (DC.LoS) c = Color.FromArgb(127, Color.Yellow);
                        else c = Color.FromArgb(127, Color.YellowGreen);
                        graphLOS.FillPolygon(new SolidBrush(c), DC.getPolygon());
                    }
                }
            }
            catch { }
        }
        public void RefreshCellsWalk()
        {
            try
            {
                lock (locker)
                {
                    graphWalk.Clear(Color.Transparent);
                    bmpWalk = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphWalk = Graphics.FromImage(bmpWalk);

                    foreach (CL.Cell DC in Cells.Values)
                    {
                        Color c;

                        if (DC.type == 4) c = Color.FromArgb(127, Color.Blue);
                        else if (DC.type == 0) c = Color.FromArgb(127, Color.Red);
                        else c = Color.FromArgb(127, Color.Blue);
                        graphWalk.FillPolygon(new SolidBrush(c), DC.getPolygon());
                    }
                }
            }
            catch { }
        }
        public void RefreshCellTeam1()
        {
            try
            {
                lock (locker)
                {
                    graphTeam1.Clear(Color.Transparent);
                    bmpTeam1 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphTeam1 = Graphics.FromImage(bmpTeam1);

                    foreach (CL.Cell DC in Cells.Values)
                    {
                        if (DC.isCellTeam1)
                        {
                            Color c = Color.FromArgb(127, Color.Blue);
                            graphTeam1.FillPolygon(new SolidBrush(c), DC.getPolygon());
                        }
                    }
                }
            }
            catch { }
        }
        public void RefreshCellTeam2()
        {
            try
            {
                lock (locker)
                {
                    graphTeam2.Clear(Color.Transparent);
                    bmpTeam2 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                    graphTeam2 = Graphics.FromImage(bmpTeam2);

                    foreach (CL.Cell DC in Cells.Values)
                    {
                        if (DC.isCellTeam2)
                        {
                            Color c = Color.FromArgb(127, Color.Red);
                            graphTeam2.FillPolygon(new SolidBrush(c), DC.getPolygon());
                        }
                    }
                }
            }
            catch { }
        }
        private void RefreshBitmaps()
        {
            try
            {
                foreach (var DC in Cells.Values.Where(x => x.gID >= 0).OrderBy(x => x.id))
                {
                    DC.ground = getBitmap(DC.gID, true);

                    if (DC.ground != null)
                    {
                        if (DC.gflip)
                            DC.ground.RotateFlip(RotateFlipType.RotateNoneFlipX);

                        if (DC.grot == 2)
                            DC.ground.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        else if (DC.grot == 3)
                            DC.ground.RotateFlip(RotateFlipType.Rotate180FlipX);
                    }
                }

                foreach (var DC in Cells.Values.Where(x => x.o1ID >= 0).OrderBy(x => x.id))
                {
                    DC.object1 = getBitmap(DC.o1ID, false);

                    if (DC.object1 != null)
                    {
                        if (DC.o1flip)
                            DC.object1.RotateFlip(RotateFlipType.RotateNoneFlipX);

                        if (DC.o1rot == 2)
                            DC.object1.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        else if (DC.o1rot == 3)
                            DC.object1.RotateFlip(RotateFlipType.Rotate180FlipX);
                    }
                }

                foreach (var DC in Cells.Values.Where(x => x.o2ID >= 0).OrderBy(x => x.id))
                {
                    DC.object2 = getBitmap(DC.o2ID, false);

                    if (DC.object2 != null)
                    {
                        if (DC.o2flip)
                            DC.object2.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    }
                }
            }
            catch { }
        }
        private void RefreshGroundCells()
        {
            try
            {
                lock (locker)
                {
                    graphGround.Clear(Color.Transparent);

                    foreach (var DC in Cells.Values.Where(x => x.gID >= 0).OrderBy(x => x.id))
                    {
                        Point pos = DC.pos;
                        Bitmap b = DC.ground;
                        if (b != null)
                        {
                            var decal = CL.Manager.GetTile(CL.Manager.TileType.grounds, DC.gID);
                            if (decal != "")
                            {
                                var pInfos = decal.Split(';');
                                var p = new Point(int.Parse(pInfos[1]), int.Parse(pInfos[2]));
                                var newDecal = NewDelecage(p);
                                pos.X -= newDecal.X - CELL_W_HALF;
                                pos.Y -= newDecal.Y - CELL_H_HALF;
                            }

                            graphGround.DrawImage(b, pos);
                        }
                    }
                }
            }
            catch { }
        }
        private void RefreshObject1Cells()
        {
            try
            {
                lock (locker)
                {
                    graphObject1.Clear(Color.Transparent);

                    foreach (var DC in Cells.Values.Where(x => x.o1ID >= 0).OrderBy(x => x.id))
                    {
                        Point pos = DC.pos;
                        Bitmap b = DC.object1;

                        if (b != null)
                        {
                            var decal = Manager.GetTile(Manager.TileType.objects, DC.o1ID);
                            if (decal != "")
                            {
                                var pInfos = decal.Split(';');
                                var p = new Point(int.Parse(pInfos[1]), int.Parse(pInfos[2]));
                                var newDecal = NewDelecage(p);
                                pos.X -= newDecal.X - CELL_W_HALF;
                                pos.Y -= newDecal.Y - CELL_H_HALF;
                            }

                            graphObject1.DrawImage(b, pos);
                        }
                    }
                }
            }
            catch { }
        }
        private void RefreshObject2Cells()
        {
            try
            {
                lock (locker)
                {
                    graphObject2.Clear(Color.Transparent);

                    foreach (var DC in Cells.Values.Where(x => x.o2ID >= 0).OrderBy(x => x.id))
                    {
                        Point pos = DC.pos;
                        Bitmap b = DC.object2;

                        if (b != null)
                        {
                            var decal = Manager.GetTile(Manager.TileType.objects, DC.o2ID);
                            if (decal != "")
                            {
                                var pInfos = decal.Split(';');
                                var p = new Point(int.Parse(pInfos[1]), int.Parse(pInfos[2]));
                                var newDecal = NewDelecage(p);
                                pos.X -= newDecal.X - CELL_W_HALF;
                                pos.Y -= newDecal.Y - CELL_H_HALF;
                            }

                            graphObject2.DrawImage(b, pos);
                        }
                    }
                }
            }
            catch { }
        }

        public int CELL_H
        {
            get
            {
                return CELL_H_HALF * 2;
            }
        }
        public int CELL_H_HALF
        {
            get
            {
                return (int)Math.Round((double)(pictureBox1.Height / (height * 2 - 2)));
            }
        }
        public int CELL_W
        {
            get
            {
                return CELL_W_HALF * 2;
            }
        }
        public int CELL_W_HALF
        {
            get
            {
                return (int)Math.Round((double)(pictureBox1.Width / (width * 2 - 2)));
            }
        }

        public Bitmap getBitmap(int id, bool ground = false)
        {
            try
            {
                foreach (var direc in System.IO.Directory.GetDirectories("./input/tiles/" + (ground ? "Sols/" : "Objets")))
                {
                    foreach (var direc2 in System.IO.Directory.GetDirectories(direc))
                    {
                        if (System.IO.File.Exists(direc2 + "/" + id + ".png"))
                        {
                            Bitmap last = new Bitmap(Image.FromFile(direc2 + "/" + id + ".png"));
                            Size newSize = newSizeImage(last.Size);
                            Bitmap result = new Bitmap(newSize.Width, newSize.Height);

                            using (Graphics g = Graphics.FromImage((Image)result))
                                g.DrawImage(last, 0, 0, newSize.Width, newSize.Height);

                            return result;
                        }
                        else if (System.IO.File.Exists(direc2 + "/" + id + ".jpg"))
                        {
                            Bitmap last = new Bitmap(Image.FromFile(direc2 + "/" + id + ".jpg"));
                            Size newSize = newSizeImage(last.Size);
                            Bitmap result = new Bitmap(newSize.Width, newSize.Height);

                            using (Graphics g = Graphics.FromImage((Image)result))
                                g.DrawImage(last, 0, 0, newSize.Width, newSize.Height);

                            return result;
                        }
                    }
                    if (System.IO.File.Exists(direc + "/" + id + ".png"))
                    {
                        Bitmap last = new Bitmap(Image.FromFile(direc + "/" + id + ".png"));
                        Size newSize = newSizeImage(last.Size);
                        Bitmap result = new Bitmap(newSize.Width, newSize.Height);

                        using (Graphics g = Graphics.FromImage((Image)result))
                            g.DrawImage(last, 0, 0, newSize.Width, newSize.Height);

                        return result;
                    }
                    else if (System.IO.File.Exists(direc + "/" + id + ".jpg"))
                    {
                        Bitmap last = new Bitmap(Image.FromFile(direc + "/" + id + ".jpg"));
                        Size newSize = newSizeImage(last.Size);
                        Bitmap result = new Bitmap(newSize.Width, newSize.Height);

                        using (Graphics g = Graphics.FromImage((Image)result))
                            g.DrawImage(last, 0, 0, newSize.Width, newSize.Height);

                        return result;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
        public Bitmap RotateImage(Bitmap img, float rotationAngle)
        {
            try
            {
                Bitmap bmp = new Bitmap(img.Width, img.Height);
                Graphics gfx = Graphics.FromImage(bmp);

                gfx.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);
                gfx.RotateTransform(rotationAngle);
                gfx.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);
                gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;
                gfx.DrawImage(img, new Point(0, 0));
                gfx.Dispose();

                return bmp;
            }
            catch { return null; }
        }
        public Size newSizeImage(Size size)
        {
            try
            {
                var newsize = new Size();

                if (pictureBox1.Size.Height == 430)
                    newsize.Height = size.Height;
                else
                {
                    var pourcent = (double)430 / 100;
                    var actualheightprocent = (double)pictureBox1.Size.Height / pourcent;
                    var actualtileheightprocent = (double)size.Height / 100;
                    newsize.Height = (int)((actualtileheightprocent * actualheightprocent));
                }

                if (pictureBox1.Size.Width == 740)
                    newsize.Width = size.Width;
                else
                {
                    var pourcent = (double)740 / 100;
                    var actualwidthprocent = (double)pictureBox1.Size.Width / pourcent;
                    var actualtilewidthprocent = (double)size.Width / 100;
                    newsize.Width = (int)((actualtilewidthprocent * actualwidthprocent));
                }

                return newsize;
            }
            catch { return new Size(); }
        }

        private void pictureBox1_Click(object sender, MouseEventArgs e)
        {
            try
            {
                lastClic = e;

                collec = Program.MainFrame.listView1.Items.Count;

                if(Program.MainFrame.listView1.SelectedItems.Count > 0)
                    collecSelected = int.Parse(Program.MainFrame.listView1.SelectedItems[0].Text);

                ground = (Program.MainFrame.treeView1.SelectedNode.FullPath.Contains("Objets") ? 0 : 1);

                nMousePosition = pictureBox1.PointToClient(MousePosition);

                var t = new System.Threading.Thread(new System.Threading.ThreadStart(this.UpdateFromThread));
                t.Start();
            }
            catch { }
        }
        private void UpdateFromThread()
        {
            try
            {
                var e = lastClic;
                var MP = nMousePosition;
                CL.Cell DC = null;

                if (Cells.Values.Any(x => x.containsPoint(MP)))
                {
                    DC = Cells.Values.First(x => x.containsPoint(MP));

                    if (Program.MainFrame.isEditing == false && e.Button != System.Windows.Forms.MouseButtons.Right)
                        return;
                }
                else 
                    return;

                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    if (Program.OptionsFrame.mustRightClicShowMenu)
                    {
                        this.Invoke(new Action(delegate
                        {
                            cellsInfosMenu.Items.Clear();
                            cellsInfosMenu.Items.Add(new ToolStripMenuItem(DC.id.ToString()));
                            cellsInfosMenu.Items.Add(new ToolStripSeparator());

                            var groundcell = new ToolStripMenuItem(string.Concat("Supprimer le fond (", DC.gID.ToString(), ")"));

                            if (DC.gID >= 0)
                                groundcell.Image = DC.ground;

                            groundcell.Click += delegate
                            {
                                DC.gID = -1;
                                DC.gflip = false;
                                DC.ground = null;
                                DC.grot = 0;
                                RefreshGroundCells();
                                RefreshMap();
                            };
                            cellsInfosMenu.Items.Add(groundcell);

                            var objectcell = new ToolStripMenuItem(string.Concat("Supprimer l'objet 1 (", DC.o1ID.ToString(), ")"));

                            if (DC.o1ID >= 0)
                                objectcell.Image = DC.object1;

                            objectcell.Click += delegate
                            {
                                DC.o1ID = -1;
                                DC.o1flip = false;
                                DC.object1 = null;
                                DC.o1rot = 0;
                                RefreshObject1Cells();
                                RefreshMap();
                            };
                            cellsInfosMenu.Items.Add(objectcell);

                            var object2cell = new ToolStripMenuItem(string.Concat("Supprimer l'objet 2 (", DC.o2ID.ToString(), ")"));

                            if (DC.o2ID >= 0)
                                object2cell.Image = DC.object2;

                            object2cell.Click += delegate
                            {
                                DC.o2ID = -1;
                                DC.o2flip = false;
                                DC.object2 = null;
                                RefreshObject2Cells();
                                RefreshMap();
                            };
                            cellsInfosMenu.Items.Add(object2cell);

                            if (DC.o2ID >= 1)
                            {
                                cellsInfosMenu.Items.Add(new ToolStripSeparator());

                                var objectInterac = new ToolStripMenuItem();
                                if (DC.o2interactive == true)
                                {
                                    objectInterac.Text = "Retirer l'intéractivié de l'objet (Calque Objet 2)";
                                    objectInterac.Click += delegate
                                    {
                                        DC.o2interactive = false;
                                    };
                                }
                                else
                                {
                                    objectInterac.Text = "Mettre l'objet intéractif (Calque Objet 2)";
                                    objectInterac.Click += delegate
                                    {
                                        DC.o2interactive = true;
                                    };
                                }
                                cellsInfosMenu.Items.Add(objectInterac);                                
                            }

                            cellsInfosMenu.Show(this, nMousePosition);
                        }));
                    }
                    else if (Program.OptionsFrame.mustRightClicRemoveTile)
                    {
                        switch (Program.MainFrame.CurrentLevel)
                        {
                            case 1:

                                DC.gflip = false;
                                DC.gID = -1;
                                DC.grot = 0;
                                DC.ground = null;

                                RefreshGroundCells();
                                RefreshMap();
                                break;

                            case 2 :

                                DC.o1flip = false;
                                DC.o1ID = -1;
                                DC.o1rot = 0;
                                DC.object1 = null;

                                RefreshObject1Cells();
                                RefreshMap();
                                break;

                            case 3 :

                                DC.o2flip = false;
                                DC.o2ID = -1;
                                DC.o2interactive = false;
                                DC.object2 = null;

                                RefreshObject2Cells();
                                RefreshMap();
                                break;
                        }
                    }

                    return;
                }

                if (Program.MainFrame.viewWalkable || Program.MainFrame.viewLoS || Program.MainFrame.viewCellFight2 || Program.MainFrame.viewCellFight1)
                {
                    if (Program.MainFrame.viewWalkable)
                    {
                        switch (DC.type)
                        {
                            case 0://Unwalkable
                                DC.type = 4;
                                break;
                            case 1:
                            case 2:
                            case 3:
                                DC.type = 4;
                                break;
                            default:
                                DC.type = 0;
                                break;
                        }
                        RefreshCellsWalk();
                        RefreshMap();
                    }
                    else if (Program.MainFrame.viewLoS)
                    {
                        DC.LoS = !DC.LoS;
                        RefreshCellsLoS();
                        RefreshMap();
                    }
                    else if (Program.MainFrame.viewCellFight1)
                    {
                        DC.isCellTeam1 = !DC.isCellTeam1;
                        RefreshCellTeam1();
                        RefreshMap();
                    }
                    else if (Program.MainFrame.viewCellFight2)
                    {
                        DC.isCellTeam2 = !DC.isCellTeam2;
                        RefreshCellTeam2();
                        RefreshMap();
                    }

                    return;
                }

                if (collec < 1)
                    return;

                switch (Program.MainFrame.CurrentLevel)
                {
                    case 1:

                        if (ground != 1)
                            return;

                        DC.gID = collecSelected;
                        DC.ground = getBitmap(collecSelected, true);

                        if (Program.MainFrame.mustTileFlip)
                        {
                            DC.gflip = true;
                            DC.ground.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        }
                        else if (Program.MainFrame.mustTileRot2)
                        {
                            DC.grot = 2;
                            DC.ground.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        }
                        else if (Program.MainFrame.mustTileRot3)
                        {
                            DC.grot = 3;
                            DC.ground.RotateFlip(RotateFlipType.Rotate180FlipX);
                        }
                        else
                        {
                            DC.grot = 0;
                            DC.gflip = false;
                        }

                        RefreshGroundCells();
                        RefreshMap();
                        return;

                    case 2:

                        if (ground != 0)
                            return;

                        DC.o1ID = collecSelected;
                        DC.object1 = getBitmap(collecSelected, false);

                        if (Program.MainFrame.mustTileFlip)
                        {
                            DC.o1flip = true;
                            DC.object1.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        }
                        else if (Program.MainFrame.mustTileRot2)
                        {
                            DC.o1rot = 2;
                            DC.object1.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        }
                        else if (Program.MainFrame.mustTileRot3)
                        {
                            DC.o1rot = 3;
                            DC.object1.RotateFlip(RotateFlipType.Rotate180FlipX);
                        }
                        else
                        {
                            DC.o1rot = 0;
                            DC.o1flip = false;
                        }

                        RefreshObject1Cells();
                        RefreshMap();
                        return;

                    case 3:

                        if (ground != 0)
                            return;

                        DC.o2ID = collecSelected;
                        DC.object2 = getBitmap(collecSelected, false);

                        if (Program.MainFrame.mustTileFlip)
                        {
                            DC.o2flip = true;
                            DC.object2.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        }
                        else
                            DC.o2flip = false;

                        if (Program.MainFrame.mustNextInterractif)
                            DC.o2interactive = true;

                        RefreshObject2Cells();
                        RefreshMap();
                        return;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.ToString()); }
        }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                if (toolStripMenuItem2.Checked == false)
                {
                    this.Size = new Size(579, 354);
                    toolStripMenuItem2.Checked = true;
                }
            }
            catch { }
        }
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            try
            {
                if (toolStripMenuItem3.Checked == false)
                {
                    this.Size = new Size(665, 405);
                    toolStripMenuItem3.Checked = true;
                }
            }
            catch { }
        }
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            try
            {
                if (toolStripMenuItem4.Checked == false)
                {
                    this.Size = new Size(775, 480);
                    toolStripMenuItem4.Checked = true;
                }
            }
            catch { }
        }

        private void imageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!System.IO.Directory.Exists("./output/" + ID))
                    System.IO.Directory.CreateDirectory("./output/" + ID);

                if (System.IO.File.Exists("./output/" + ID + "/screen_" + screensCount++ + ".png"))
                    System.IO.File.Delete("./output/" + ID + "/screen_" + screensCount + ".png");

                pictureBox1.Image.Save("./output/" + ID + "/screen_" + screensCount + ".png", System.Drawing.Imaging.ImageFormat.Png);
            }
            catch
            {
                MessageBox.Show("Cannot save the image !");
            }
        }

        private void StartPreview(object sender, EventArgs e)
        {
            try
            {
                if (Program.MainFrame.listView1.SelectedItems.Count >= 1)
                {
                    int ID;

                    if (!int.TryParse(Program.MainFrame.listView1.SelectedItems[0].Text, out ID))
                        return;

                    selectedTileID = ID;
                    preview = getBitmap(selectedTileID, (Program.MainFrame.modifyLevel1 ? true : false));

                    if (Program.MainFrame.mustTileFlip)
                        preview.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    else if (Program.MainFrame.mustTileRot2)
                        preview.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    else if (Program.MainFrame.mustTileRot3)
                        preview.RotateFlip(RotateFlipType.Rotate180FlipX);
                }

                mustUpdateApercu = true;
                graphPreview.Clear(Color.Transparent);
            }
            catch { }
        }
        private void StartLaying(object sender, MouseEventArgs e)
        {
            mustLayingTile = true;
            mustUpdateApercu = false;
            mustLay = true;
        }
        private void UpdatePreview(object sender, MouseEventArgs e)
        {
            try
            {
                collec = Program.MainFrame.listView1.Items.Count;
                var MP = pictureBox1.PointToClient(MousePosition);
                nMousePosition = MP;

                if (Program.MainFrame.listView1.SelectedItems.Count > 0)
                    collecSelected = int.Parse(Program.MainFrame.listView1.SelectedItems[0].Text);

                ground = (Program.MainFrame.treeView1.SelectedNode.FullPath.Contains("Objets") ? 0 : 1);

                if (Cells.Values.ToList().Any(x => x.containsPoint(MP)))
                {
                    CL.Cell cell = Cells.Values.ToList().First(x => x.containsPoint(MP));

                    if (cell.id == lastCellID && !mustLay)
                        return;

                    mustLay = false;

                    lastCellID = cell.id;

                    if (Program.MainFrame.isEditing == false)
                        return;

                    if (mustUpdateApercu)
                    {
                        graphPreview.Clear(Color.Transparent);

                        if (Program.MainFrame.viewLoS || Program.MainFrame.viewWalkable || Program.MainFrame.viewCellFight1 || Program.MainFrame.viewCellFight2)
                        {
                            graphPreview.FillPolygon(new SolidBrush(Color.FromArgb(127, Color.Black)), cell.getPolygon());
                            RefreshMap();
                            return;
                        }
                        else if (selectedTileID == -1 || collec < 1)
                            return;

                        if (preview == null)
                            return;

                        Point pos = cell.pos;

                        if (Program.MainFrame.modifyLevel3 || Program.MainFrame.modifyLevel2)
                        {
                            var decal = Manager.GetTile(Manager.TileType.objects, selectedTileID);
                            if (decal != "")
                            {
                                var pInfos = decal.Split(';');
                                var p = new Point(int.Parse(pInfos[1]), int.Parse(pInfos[2]));
                                var newDecal = NewDelecage(p);
                                pos.X -= newDecal.X - CELL_W_HALF;
                                pos.Y -= newDecal.Y - CELL_H_HALF;
                            }
                        }
                        else
                        {
                            var decal = Manager.GetTile(Manager.TileType.grounds, selectedTileID);
                            if (decal != "")
                            {
                                var pInfos = decal.Split(';');
                                var p = new Point(int.Parse(pInfos[1]), int.Parse(pInfos[2]));
                                var newDecal = NewDelecage(p);
                                pos.X -= newDecal.X - CELL_W_HALF;
                                pos.Y -= newDecal.Y - CELL_H_HALF;
                            }
                        }

                        lock (locker)
                        {
                            graphPreview.FillPolygon(new SolidBrush(Color.FromArgb(127, Color.Gray)), cell.getPolygon());
                            graphPreview.DrawImage(preview, pos);
                        }


                        RefreshMap();
                    }

                    if (mustLayingTile)
                        pictureBox1_Click(null, e);

                }
            }
            catch { }
        }
        public void UpdateFromExPreview(MouseEventArgs e, bool force = true)
        {
            try
            {
                collec = Program.MainFrame.listView1.Items.Count;
                var MP = pictureBox1.PointToClient(MousePosition);
                nMousePosition = MP;

                if (Program.MainFrame.listView1.SelectedItems.Count > 0)
                    collecSelected = int.Parse(Program.MainFrame.listView1.SelectedItems[0].Text);

                ground = (Program.MainFrame.treeView1.SelectedNode.FullPath.Contains("Objets") ? 0 : 1);

                if (Cells.Values.ToList().Any(x => x.containsPoint(MP)))
                {
                    CL.Cell cell = Cells.Values.ToList().First(x => x.containsPoint(MP));

                    if (cell.id == lastCellID && !mustLay && !force)
                        return;

                    mustLay = false;

                    lastCellID = cell.id;

                    if (Program.MainFrame.isEditing == false)
                        return;

                    if (force && (selectedTileID != -1 || collec >= 1) && Program.MainFrame.isEditing == true)
                    {
                        preview = getBitmap(selectedTileID, (Program.MainFrame.modifyLevel1 ? true : false));

                        if (Program.MainFrame.mustTileFlip)
                            preview.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        else if (Program.MainFrame.mustTileRot2)
                            preview.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        else if (Program.MainFrame.mustTileRot3)
                            preview.RotateFlip(RotateFlipType.Rotate180FlipX);
                    }

                    if (mustUpdateApercu)
                    {
                        graphPreview.Clear(Color.Transparent);

                        if (Program.MainFrame.viewLoS || Program.MainFrame.viewWalkable || Program.MainFrame.viewCellFight1 || Program.MainFrame.viewCellFight2)
                        {
                            graphPreview.FillPolygon(new SolidBrush(Color.FromArgb(127, Color.Black)), cell.getPolygon());
                            RefreshMap();
                            return;
                        }
                        else if (selectedTileID == -1 || collec < 1)
                            return;

                        if (preview == null)
                            return;

                        Point pos = cell.pos;

                        if (Program.MainFrame.modifyLevel3 || Program.MainFrame.modifyLevel2)
                        {
                            var decal = Manager.GetTile(Manager.TileType.objects, selectedTileID);
                            if (decal != "")
                            {
                                var pInfos = decal.Split(';');
                                var p = new Point(int.Parse(pInfos[1]), int.Parse(pInfos[2]));
                                var newDecal = NewDelecage(p);
                                pos.X -= newDecal.X - CELL_W_HALF;
                                pos.Y -= newDecal.Y - CELL_H_HALF;
                            }
                        }
                        else
                        {
                            var decal = Manager.GetTile(Manager.TileType.grounds, selectedTileID);
                            if (decal != "")
                            {
                                var pInfos = decal.Split(';');
                                var p = new Point(int.Parse(pInfos[1]), int.Parse(pInfos[2]));
                                var newDecal = NewDelecage(p);
                                pos.X -= newDecal.X - CELL_W_HALF;
                                pos.Y -= newDecal.Y - CELL_H_HALF;
                            }
                        }

                        lock (locker)
                        {
                            graphPreview.FillPolygon(new SolidBrush(Color.FromArgb(127, Color.Gray)), cell.getPolygon());
                            graphPreview.DrawImage(preview, pos);
                        }


                        RefreshMap();
                    }

                    if (mustLayingTile)
                        pictureBox1_Click(null, e);

                }
            }
            catch { }
        }
        private void StopPreview(object sender, EventArgs e)
        {
            mustUpdateApercu = false;
            mustLayingTile = false;

            lock(locker)
                graphPreview.Clear(Color.Transparent);

            RefreshMap();
        }
        private void StopLaying(object sender, MouseEventArgs e)
        {
            mustUpdateApercu = true;
            mustLayingTile = false;
        }

        private void optionsDeLaMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MapOptions option = new MapOptions(this);
                option.Show();
            }
            catch { }
        }
        private void carteDMdmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (Program.OptionsFrame.InsertAutoBDD)
                    CL.Database.SaveMap(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            SaveMapDM();
            SaveMapSQL();
            SaveMapSWF();
        }

        private void SaveMapSWF()
        {
            try
            {
                if (Program.OptionsFrame.mustCopyAutoSWF)
                {
                    string basepath = Program.OptionsFrame.LinkSWF;
                    string filename = ID.ToString() + "_" + signature;

                    string swfname = filename + ".swf";
                    string flmname = filename + ".flm";

                    if (!Directory.Exists(basepath))
                        Directory.CreateDirectory(basepath);

                    if (!Directory.Exists(basepath + @"\" + ID))
                        Directory.CreateDirectory(basepath + @"\" + ID);

                    if (File.Exists(basepath + @"\" + ID + @"\" + swfname))
                        File.Delete(basepath + @"\" + ID + @"\" + swfname);
                    if (File.Exists(basepath + @"\" + ID + @"\" + flmname))
                        File.Delete(basepath + @"\" + ID + @"\" + flmname);

                    File.Copy("./utilities/default.swf", basepath + @"\" + ID + @"\" + swfname);

                    string swfPath = basepath + @"\" + ID + @"\" + swfname;
                    string flmPath = basepath + @"\" + ID + @"\" + flmname;

                    ProcessStartInfo PSI = new ProcessStartInfo(Application.StartupPath + "/utilities/flasm.exe", swfPath);
                    PSI.WindowStyle = ProcessWindowStyle.Hidden;
                    (Process.Start(PSI)).WaitForExit();

                    StreamReader SR = new StreamReader(flmPath);
                    string flmFile = SR.ReadToEnd();
                    SR.Close();

                    flmFile = flmFile.Replace("INSERTMAPIDHERE", ID.ToString());
                    flmFile = flmFile.Replace("INSERTMAPWIDTHHERE", width.ToString());
                    flmFile = flmFile.Replace("INSERTMAPHEIGHTHERE", height.ToString());
                    flmFile = flmFile.Replace("INSERTBGIDHERE", BackID.ToString());
                    flmFile = flmFile.Replace("INSERTAMBIDHERE", "0");
                    flmFile = flmFile.Replace("INSERTMUSICIDHERE", "0");
                    flmFile = flmFile.Replace("INSERTOUTDOORHERE", externMap ? "1" : "0");
                    flmFile = flmFile.Replace("INSERTCAPABILITIESHERE", getCapabilities().ToString());
                    flmFile = flmFile.Replace("INSERTMAPDATAHERE", compressMap());
                    File.Delete(flmPath);
                    StreamWriter SW = new StreamWriter(flmPath, true);
                    SW.Write(flmFile);
                    SW.Flush();
                    SW.Close();

                    PSI = new ProcessStartInfo(Application.StartupPath + "/utilities/flasm.exe", flmPath);
                    PSI.WindowStyle = ProcessWindowStyle.Hidden;
                    (Process.Start(PSI)).WaitForExit();

                    File.Delete(flmPath);
                    File.Delete(flmPath.Replace(".flm", ".$wf"));

                    if (Program.OptionsFrame.mustCopyServerSWF)
                    {
                        if (File.Exists(Program.OptionsFrame.LinkCopySWF + "/" + swfname))
                            File.Delete(Program.OptionsFrame.LinkCopySWF + "/" + swfname);

                        File.Copy(swfPath, Program.OptionsFrame.LinkCopySWF + "/" + swfname);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void SaveMapDM()
        {
            try
            {
                if (Program.OptionsFrame.mustCopyAutoDM)
                {
                    string basepath = Program.OptionsFrame.LinkDM;
                    string filename = ID.ToString() + "_" + signature + ".dm";

                    if (!Directory.Exists(basepath))
                        Directory.CreateDirectory(basepath);

                    if (!Directory.Exists(basepath + @"\" + ID))
                        Directory.CreateDirectory(basepath + @"\" + ID);

                    string file = basepath + @"\" + ID + @"\" + filename;

                    var writer = new StreamWriter(file, false, Encoding.Default);

                    writer.WriteLine("ID=" + ID.ToString());
                    writer.WriteLine("WD=" + width.ToString());
                    writer.WriteLine("HG=" + height.ToString());
                    writer.WriteLine("BG=" + BackID.ToString());
                    writer.WriteLine("AM=" + "0");
                    writer.WriteLine("MU=" + "0");
                    writer.WriteLine("OD=" + (externMap ? "1" : "0"));
                    writer.WriteLine("CA=" + getCapabilities().ToString());
                    writer.WriteLine("MD=" + compressMap());
                    writer.WriteLine("CF=" + GetCellsFight());

                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void SaveMapSQL()
        {
            try
            {
                if (Program.OptionsFrame.mustCopyAutoSQL)
                {
                    string basepath = Program.OptionsFrame.LinkSQL;
                    string filename = ID.ToString() + "_" + signature + ".sql";

                    if (!Directory.Exists(basepath))
                        Directory.CreateDirectory(basepath);

                    if (!Directory.Exists(basepath + @"\" + ID))
                        Directory.CreateDirectory(basepath + @"\" + ID);

                    string file = basepath + @"\" + ID + @"\" + filename;

                    var writer = new StreamWriter(file, false, Encoding.Default);

                    writer.WriteLine("DELETE FROM maps WHERE id=" + ID + ";");
                    writer.WriteLine("INSERT INTO `maps` (`id`, `date`, `width`, `heigth`, `places`, `key`, `mapData`, `monsters`, `capabilities`, `mappos`, `numgroup`) VALUES (" +
                        "'" + ID + "','" + signature + "','" + width + "','" + height + "','" + GetCellsFight(true) + "','','" + compressMap() + "','','" + getCapabilities() + "','0,0,0','5')");

                    writer.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public string GetCellsFight(bool sql = false)
        {
            try
            {
                var cellteam1 = string.Empty;
                var cellteam2 = string.Empty;

                if (sql)
                {
                    cellteam1 = string.Join("", from a in Cells.Values.Where(x => x.isCellTeam1) select a.HashCodeCell().ToString());
                    cellteam2 = string.Join("", from a in Cells.Values.Where(x => x.isCellTeam2) select a.HashCodeCell().ToString());
                }
                else
                {
                    cellteam1 = string.Join(";", from a in Cells.Values.Where(x => x.isCellTeam1) select a.id.ToString());
                    cellteam2 = string.Join(";", from a in Cells.Values.Where(x => x.isCellTeam2) select a.id.ToString());
                }

                return cellteam1 + "|" + cellteam2;
            }
            catch { return "|"; }
        }

        public string compressMap()
        {
            try
            {
                string data = "";
                foreach (CL.Cell DC in Cells.Values)
                {
                    data += compressCell(DC);
                }
                return data;
            }
            catch { return ""; }
        }
        private string compressCell(CL.Cell DC)
        {
            string data = "";
            int[] infos = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            infos[0] = (DC.active ? (1) : (0)) << 5;
            infos[0] = infos[0] | (DC.LoS ? (1) : (0));
            infos[0] = infos[0] | (DC.gID & 1536) >> 6;
            infos[0] = infos[0] | (DC.o1ID & 8192) >> 11;
            infos[0] = infos[0] | (DC.o2ID & 8192) >> 12;

            infos[1] = (DC.grot & 3) << 4;
            infos[1] = infos[1] | DC.glvl & 15;

            infos[2] = (DC.type & 7) << 3;
            infos[2] = infos[2] | DC.gID >> 6 & 7;

            infos[3] = DC.gID & 63;

            infos[4] = (DC.gslope & 15) << 2;
            infos[4] = infos[4] | (DC.gflip ? (1) : (0)) << 1;
            infos[4] = infos[4] | DC.o1ID >> 12 & 1;

            infos[5] = DC.o1ID >> 6 & 63;

            infos[6] = DC.o1ID & 63;

            infos[7] = (DC.o1rot & 3) << 4;
            infos[7] = infos[7] | (DC.o1flip ? (1) : (0)) << 3;
            infos[7] = infos[7] | (DC.o2flip ? (1) : (0)) << 2;
            infos[7] = infos[7] | (DC.o2interactive ? (1) : (0)) << 1;
            infos[7] = infos[7] | DC.o2ID >> 12 & 1;

            infos[8] = DC.o2ID >> 6 & 63;

            infos[9] = DC.o2ID & 63;

            char[] HASH = {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's',
	            't', 'u', 'v', 'w', 'x', 'y', 'z', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
	            'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '_'};

            foreach (int i in infos)
            {
                data += HASH[i];
            }
            return data;
        }
        public int getCapabilities()
        {
            try
            {
                string str = (!telepAuto ? "1" : "0") + (!saveAuto ? "1" : "0") + (!agroAuto ? "1" : "0") + (!challengeAuto ? "1" : "0");
                return Convert.ToInt32(str, 2);
            }
            catch { return 0; }
        }
    }
}
