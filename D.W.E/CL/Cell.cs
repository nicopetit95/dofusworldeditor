using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace DWE.MapEditor.CL
{
    public class Cell
    {
        public bool active = true;
        public int id;
        public int gID = -1;
        public int o1ID = -1;
        public int o2ID = -1;
        public int type = 4;//Walkable
        public bool LoS = true;
        public int glvl = 7;
        public int gslope = 1;
        public bool gflip = false;
        public bool o1flip = false;
        public bool o2flip = false;
        public int grot = 0;
        public int o1rot = 0;
        public bool o2interactive = false;
        public Point pos;
        public Frames.MapFrame Frame;

        public Bitmap ground;
        public Bitmap object1;
        public Bitmap object2;

        public bool isCellTeam1 = false;
        public bool isCellTeam2 = false;
        
        public Cell(int i,int w,int h, Frames.MapFrame frame)
        {
            id = i;
            Frame = frame;
            pos = getPositionByCellID(i,glvl,w,h);
        }
        public Cell(int aid, string CellData, int w, int h, Frames.MapFrame frame)
        {
            try
            {
                id = aid;
                Frame = frame;
                byte[] CellInfo = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                for (int i = CellData.Length - 1; i >= 0; i--)
                    CellInfo[i] = (byte)Program.Hash.IndexOf(CellData[i]);

                active = ((CellInfo[0] & 32) >> 5) != 0;
                if (!active)
                {
                    gID = 0;
                    return;
                }
                LoS = (CellInfo[0] & 1) != 0;
                grot = (CellInfo[1] & 48) >> 4;
                glvl = CellInfo[1] & 15;
                type = (CellInfo[2] & 56) >> 3;
                gID = ((CellInfo[0] & 24) << 6) + ((CellInfo[2] & 7) << 6) + CellInfo[3];
                ground = frame.getBitmap(gID, true);
                gslope = (CellInfo[4] & 60) >> 2;
                gflip = ((CellInfo[4] & 2) >> 1) != 0;
                o1ID = ((CellInfo[0] & 4) << 11) + ((CellInfo[4] & 1) << 12) + (CellInfo[5] << 6) + CellInfo[6];
                object1 = frame.getBitmap(o1ID, false);
                o1rot = (CellInfo[7] & 48) >> 4;
                o1flip = ((CellInfo[7] & 8) >> 3) != 0;
                o2flip = ((CellInfo[7] & 4) >> 2) != 0;
                o2interactive = Convert.ToBoolean((CellInfo[7] & 2) >> 1);
                o2ID = ((CellInfo[0] & 2) << 12) + ((CellInfo[7] & 1) << 12) + (CellInfo[8] << 6) + CellInfo[9];
                object2 = frame.getBitmap(o2ID, false);

                if (ground != null)
                {
                    if (gflip)
                        ground.RotateFlip(RotateFlipType.RotateNoneFlipX);

                    if (grot == 1)
                    {
                        ground.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        gflip = true;
                        grot = 0;
                    }
                    else if (grot == 2)
                        ground.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    else if (grot == 3)
                        ground.RotateFlip(RotateFlipType.Rotate180FlipX);
                }

                if (object1 != null)
                {
                    if (o1flip)
                        object1.RotateFlip(RotateFlipType.RotateNoneFlipX);

                    if (o1rot == 1)
                    {
                        object1.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        o1flip = true;
                        o1rot = 0;
                    }
                    else if (o1rot == 2)
                        object1.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    else if (o1rot == 3)
                        object1.RotateFlip(RotateFlipType.Rotate180FlipX);
                }

                if (object2 != null)
                {
                    if (o2flip)
                        object2.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }

                pos = getPositionByCellID(id, glvl, w, h);
            }
            catch { }
        }
        public Point getPositionByCellID(int cellid, int glvl,int width, int height)
        {
            double x = 0, y = 0;
            double j = -2;
            double k = 0;
            int l = -1;
            int s = width - 2;

            for (int z = 1; z <= cellid; z++)
            {
                if (j == s)
                {
                    j = -1;
                    l++;
                    if (k == 0)
                    {
                        k = Frame.CELL_W_HALF;
                        s--;
                    }
                    else
                    {
                        k = 0;
                        s++;
                    }
                }
                else
                    ++j;
            }

            x = j * Frame.CELL_W + k + Frame.CELL_W_HALF;
            y = (l) * Frame.CELL_H_HALF;

            return new Point((int)x, (int)y);
        }
        
        public Point[] getPolygon()
        {
            List<Point> pts = new List<Point>();

            pts.Add(new Point((pos.X + Frame.CELL_W_HALF) + 1, pos.Y));//sommet de la case
            pts.Add(new Point((pos.X + Frame.CELL_W) + 1, (pos.Y + Frame.CELL_H_HALF)));//Droite
            pts.Add(new Point((pos.X + Frame.CELL_W_HALF) + 1, (pos.Y + Frame.CELL_H)));//Bas
            pts.Add(new Point(pos.X + 1, (pos.Y + Frame.CELL_H_HALF)));//Gauche

            return pts.ToArray();
        }
        public bool containsPoint(Point p)
        {
            Point p1, p2;
            Point[] poly = getPolygon();
            bool inside = false;

            if (poly.Length < 3) 
                return inside;

            Point oldPoint = new Point(poly[poly.Length - 1].X, poly[poly.Length - 1].Y);

            for (int i = 0; i < poly.Length; i++)
            {
                Point newPoint = new Point(poly[i].X, poly[i].Y);

                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;
                    p2 = newPoint;
                }
                else
                {
                    p1 = newPoint;
                    p2 = oldPoint;
                }

                if ((newPoint.X < p.X) == (p.X <= oldPoint.X) && 
                    ((long)p.Y - (long)p1.Y) * (long)(p2.X - p1.X) < ((long)p2.Y - (long)p1.Y) * (long)(p.X - p1.X))
                    inside = !inside;

                oldPoint = newPoint;
            }

            return inside;
        }

        public object HashCodeCell()
        {
            int num2 = (id % hash.Length);
            int num = Convert.ToInt32(Math.Round(Convert.ToDouble((Convert.ToDouble((id - num2)) / Convert.ToDouble(hash.Length)))));
            return (hash[num].ToString() + hash[num2].ToString());
        }

        private string hash = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_";
    }
}
