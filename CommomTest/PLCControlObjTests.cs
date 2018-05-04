using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qzeim.ThrdPrint.BroadCast.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace Qzeim.ThrdPrint.BroadCast.Common.Tests
{
    [TestClass()]
    public class PLCControlObjTests
    {
        [TestMethod()]
        public void PLCControlObjTest()
        {
            PLCControlObj obj = new PLCControlObj(0,1000,1,200,0,300,0,1000);
            Assert.IsTrue(obj != null);
            Assert.AreEqual(obj.XDir,0);
            Assert.AreEqual(obj.XVal, 1000);
            Assert.AreEqual(obj.YDir, 1);
            Assert.AreEqual(obj.YVal, 200);
            Assert.AreEqual(obj.ZDir, 0);
            Assert.AreEqual(obj.ZVal, 300);
            Assert.AreEqual(obj.RDir, 0);
            Assert.AreEqual(obj.RVal, 1000);
        }

        [TestMethod()]
        public void PLCControlObjTest1()
        {
            PLCControlObj obj = new PLCControlObj();
            Assert.IsTrue(obj != null);
        }

        [TestMethod()]
        public void ToBytesTest()
        {
            PLCControlObj obj = new PLCControlObj(0, 1000, 1, 200, 0, 300, 0, 1000);
            Assert.IsTrue(obj != null);
            Byte[] bytes = PLCControlObj.ToBytes(obj);

            Assert.AreEqual(bytes[0],0);
            Assert.AreEqual(bytes[1], 0);
            Assert.AreEqual(bytes[2], 0x03);
            Assert.AreEqual(bytes[3], 0xe8);
            Assert.AreEqual(bytes[4], 0);
            Assert.AreEqual(bytes[5], 1);
            Assert.AreEqual(bytes[6], 0);
            Assert.AreEqual(bytes[7], 0xc8);
            Assert.AreEqual(bytes[8], 0);
            Assert.AreEqual(bytes[9], 0);
            Assert.AreEqual(bytes[10], 0x01);
            Assert.AreEqual(bytes[11], 0x2c);
            Assert.AreEqual(bytes[12], 0);
            Assert.AreEqual(bytes[13], 0);
            Assert.AreEqual(bytes[14], 0x03);
            Assert.AreEqual(bytes[15], 0xe8);


        }

        [TestMethod()]
        public void FromBytesTest()
        {
            Byte[] bytes = new Byte [] {0, 0, 0x03, 0xe8, 0, 0x01, 0, 0xc8, 0, 0, 0x01, 0x2c, 0, 0, 0x03, 0xe8};
            PLCControlObj obj = PLCControlObj.FromBytes(bytes);
            Assert.AreEqual(obj.XDir,0);
            Assert.AreEqual(obj.XVal, 1000);
            Assert.AreEqual(obj.YDir, 1);
            Assert.AreEqual(obj.YVal, 200);
            Assert.AreEqual(obj.ZDir, 0);
            Assert.AreEqual(obj.ZVal, 300);
            Assert.AreEqual(obj.RDir, 0);
            Assert.AreEqual(obj.RVal, 1000);
        }

        [TestMethod()]
        public void ToByteJsonTest()
        {
            PLCControlObj obj = new PLCControlObj(0, 1000, 1, 200, 0, 300, 0, 1000);
            Assert.IsTrue(obj != null);

            string json1 = PLCControlObj.ToByteJson(obj);

        }

        [TestMethod()]
        public void FromByteJsonTest()
        {
            string json1 = "\"AAAD6AABAMgAAAEsAAAD6A==\"";
            PLCControlObj obj = PLCControlObj.FromByteJson(json1);

            Assert.AreEqual(obj.XDir, 0);
            Assert.AreEqual(obj.XVal, 1000);
            Assert.AreEqual(obj.YDir, 1);
            Assert.AreEqual(obj.YVal, 200);
            Assert.AreEqual(obj.ZDir, 0);
            Assert.AreEqual(obj.ZVal, 300);
            Assert.AreEqual(obj.RDir, 0);
            Assert.AreEqual(obj.RVal, 1000);
        }
    }
}
