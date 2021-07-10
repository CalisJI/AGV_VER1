using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace READ_TEXT485
{
    public class Caculator
    {
        public  double caculate_module(int x1, int y1, int x2, int y2)
        {
            double dis = 0;
            dis = Math.Sqrt(Math.Pow((double)((double)x1 - (double)x2), 2) + Math.Pow((double)((double)y1 - (double)y2), 2));
            return dis;

        }
        public  double calutator_angle(int x1, int y1, int x2, int y2, int corner_x, int corner_y)
        {
            double ang = 0;
            ang = (Math.Atan2(corner_y - y1, corner_x - x1) - Math.Atan2(y2 - y1, x2 - x1)) * (double)(180 / Math.PI);
            return ang;
        }
        public double current_angle(int RPM,double timer) 
        {
            double angle;
            angle = ((double)(RPM * Math.PI * 0.13 * timer * 360) / 60) / Math.PI * 0.32;
            return angle;
        }
    }
}
