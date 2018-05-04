using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;

namespace Qzeim.ThrdPrint.BroadCast.Common
{
    public class PLCControlObj
    {
        private UInt16 xDir = 0;
        private UInt16 xVal = 0;
        private UInt16 yDir = 0;
        private UInt16 yVal = 0;
        private UInt16 zDir = 0;
        private UInt16 zVal = 0;
        private UInt16 rDir = 0;
        private UInt16 rVal = 0;

        public PLCControlObj()
        {

        }

        public PLCControlObj(UInt16 _xDir, UInt16 _xVal, UInt16 _yDir, UInt16 _yVal, UInt16 _zDir, UInt16 _zVal, UInt16 _rDir, UInt16 _rVal)
        {
            xDir = _xDir;
            xVal = _xVal;
            yDir = _yDir;
            yVal = _yVal;
            zDir = _zDir;
            zVal = _zVal;
            rDir = _rDir;
            rVal = _rVal;
        }

        public UInt16 XDir
        {
            get { return xDir; }
            set { xDir = value; }
        }

        public UInt16 XVal
        {
            get { return xVal; }
            set { xVal = value; }
        }

        public UInt16 YDir
        {
            get { return yDir; }
            set { yDir = value; }
        }

        public UInt16 YVal
        {
            get { return yVal; }
            set { yVal = value; }
        }

        public UInt16 ZDir
        {
            get { return zDir; }
            set { zDir = value; }
        }

        public UInt16 ZVal
        {
            get { return zVal; }
            set { zVal = value; }
        }

        public UInt16 RDir
        {
            get { return rDir; }
            set { rDir = value; }
        }

        public UInt16 RVal
        {
            get { return rVal; }
            set { rVal = value; }
        }

        public static Byte[] ToBytes(PLCControlObj paras)
        {
            Debug.Assert(paras != null);

            Byte[] bytes = new byte[16];
            bytes[0] = (Byte)(paras.xDir >> 8);
            bytes[1] = (Byte)(paras.xDir % 256);
            bytes[2] = (Byte)(paras.xVal >> 8);
            bytes[3] = (Byte)(paras.xVal % 256);

            bytes[4] = (Byte)(paras.yDir >> 8);
            bytes[5] = (Byte)(paras.yDir % 256);
            bytes[6] = (Byte)(paras.yVal >> 8);
            bytes[7] = (Byte)(paras.yVal % 256);

            bytes[8] = (Byte)(paras.zDir >> 8);
            bytes[9] = (Byte)(paras.zDir % 256);
            bytes[10] = (Byte)(paras.zVal >> 8);
            bytes[11] = (Byte)(paras.zVal % 256);

            bytes[12] = (Byte)(paras.rDir >> 8);
            bytes[13] = (Byte)(paras.rDir % 256);
            bytes[14] = (Byte)(paras.rVal >> 8);
            bytes[15] = (Byte)(paras.rVal % 256);

            return bytes;
        }

        public static PLCControlObj FromBytes(Byte[] bytes)
        {
            PLCControlObj paras = new PLCControlObj();
            paras.XDir = (UInt16)((Convert.ToUInt16(bytes[0]) << 8) + Convert.ToUInt16(bytes[1]));
            paras.XVal = (UInt16)((Convert.ToUInt16(bytes[2]) << 8) + Convert.ToUInt16(bytes[3]));
            paras.YDir = (UInt16)((Convert.ToUInt16(bytes[4]) << 8) + Convert.ToUInt16(bytes[5]));
            paras.YVal = (UInt16)((Convert.ToUInt16(bytes[6]) << 8) + Convert.ToUInt16(bytes[7]));
            paras.ZDir = (UInt16)((Convert.ToUInt16(bytes[8]) << 8) + Convert.ToUInt16(bytes[9]));
            paras.ZVal = (UInt16)((Convert.ToUInt16(bytes[10]) << 8) + Convert.ToUInt16(bytes[11]));
            paras.RDir = (UInt16)((Convert.ToUInt16(bytes[12]) << 8) + Convert.ToUInt16(bytes[13]));
            paras.RVal = (UInt16)((Convert.ToUInt16(bytes[14]) << 8) + Convert.ToUInt16(bytes[15]));

            return paras;
        }

        public static string ToByteJson(PLCControlObj paras)
        {
            Byte[] bytes = ToBytes(paras);
            string json1 = JsonConvert.SerializeObject(bytes);
            return json1;
        }

        public static PLCControlObj FromByteJson(string json1)
        {
            Byte[] bytes = JsonConvert.DeserializeObject<Byte[]>(json1);
            PLCControlObj paras = FromBytes(bytes);
            return paras;
        }
    }
}
