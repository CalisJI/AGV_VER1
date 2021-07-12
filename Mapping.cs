using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace READ_TEXT485
{
    public class Mapping
    {
        public List<Rectangle> Rectangles { get; set; }
        public List<Point> Egde { get; set; }
        public List<string> Route { get; set; }
    }
    public class Shape 
    {
      
        public int X;
        public int Y;

      
        public Rectangle Ex_Rectangle(int Width,int Height,int X,int Y) 
        {
            Rectangle rec = new Rectangle();
            rec.Width = Width;
            rec.Height = Height;
            rec.X = X - Width / 2;
            rec.Y = Y - Height / 2;
            this.X = X;
            this.Y = Y;
            return rec;
        }

        

    }
}
