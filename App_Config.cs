using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace READ_TEXT485
{
    public class App_Config
    {
        public string COM { get; set; }
        public string Baud { get; set; }
        public string Database { get; set; }
        public string Table { get; set; }
        public string Kp { get; set; }
        public string Ki { get; set; }
        public string Kd { get; set; }
        public string rotate { get; set; }
        public float current_angle { get; set; }
        public int manual_speed { get; set; }

    }
}
