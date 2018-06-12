using System;
using System.Collections.Generic;
using System.Text;

namespace Qzeim.ThrdPrint.BroadCast.Common
{
    public class XYZ
    {
        public XYZ(double _x, double _y, double _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        private double x = 0;
        private double y = 0;
        private double z = 0;

        public double X
        {
            get { return x; }
            set { x = value; }
        }

        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        public double Z
        {
            get { return z; }
            set { z = value; }
        }

        public void SetXYZ(double _x, double _y, double _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
    }
}
