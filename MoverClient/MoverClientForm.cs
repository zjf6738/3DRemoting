using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using CSharpModBusExample;
using log4net;
using Qzeim.ThrdPrint.BroadCast.Common;

namespace MoverClient
{
    public partial class MoverClientForm : Form, IBroadCastHandler
    {

        // 与服务端的通信
        private MoverComm moverComm = null;

        // 与PLC端的通信
        private SocketWrapper socketWrapper = null;

        // 信息监控
        private MoverMonitor moverMonitor = null;

        // 日志记录
        private MoverLog moverLog = null;

        // 发送次数
        private int allSendCount = 0;


        public MoverClientForm()
        {
            InitializeComponent();

            // 服务端通信
            moverComm = new MoverComm(this);
            // 日志
            moverLog = new MoverLog();
            // 监控
            moverMonitor = new MoverMonitor();

            // PLC通信
            socketWrapper = new SocketWrapper();

        }

        private void MoverClientForm_Load(object sender, EventArgs e)
        {
            // 服务端通信
            moverComm.StartClient();
            // PLC通信
            socketWrapper.Connect();
        }

        private void MoverClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 服务端通信
            moverComm.FinishClient();
            // PLC通信
            socketWrapper.Dispose();
        }


        public void OnBroadCastingInfo(string message)
        {
            CommObj commObj = CommObj.FromJson(message);

            if (commObj == null)
            {
                moverComm.RcvMsg = "Json解析错误";
            }
            else
            {
                commObj.RcvTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                moverComm.RcvMsg = commObj.ToString();

                ProcessBroadCastingInfo(commObj);

            }

            new Thread(Check).Start();

            moverLog.DisplayBroadCastInfo(moverComm.RcvMsg);
        }

        public void Check()
        {
            lock (this)
                Invoke(new MethodInvoker(delegate()
                {
                    //MessageBox.Show(moverComm.RcvMsg);
                    this.textBox1.Text += moverComm.RcvMsg;
                    this.textBox1.Text += allSendCount.ToString();
                }));
        }

        public void ProcessBroadCastingInfo(CommObj commObj)
        {
            // 视觉端发送，PLC端接收
            if (commObj.SrcId == 0x10 && commObj.DestId == 0x30)
            {
                if (commObj.DataType.Equals("PLCControlObj"))
                {
                    PLCControlObj plcControlObj = PLCControlObj.FromByteJson(commObj.DataBody);
                    SendToPLC(PLCControlObj.ToBytes(plcControlObj));
                    allSendCount++;

                }
            }
        }


        private void SendToPLCTest(Byte [] data)
        {
            //this.Wrapper.Send(Encoding.ASCII.GetBytes(this.tbxSendText.Text.Trim()));
            //this.Wrapper.Connect();

            if (!socketWrapper.IsConnected) socketWrapper.Connect();


     
            //byte[] data = { 0x00, 0x00, 0x00, 0x64, 0x00, 0x01, 0x27, 0x10, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            List<byte> values = new List<byte>(255);
            values.AddRange(data);

            for (int i = 0; i < 100 * 5; i++)
            {
                Console.WriteLine("发送:" + DateTime.Now.ToString("yyyy-MM-dd HH:MM:SS:fff"));
                socketWrapper.Write(values.ToArray());

                //[4].防止连续读写引起前台UI线程阻塞00 
                Application.DoEvents();
                //[5].读取Response: 写完后会返回12个byte的结果
                byte[] responseHeader = socketWrapper.Read(12);
                Console.WriteLine("接收:" + DateTime.Now.ToString("yyyy-MM-dd HH:MM:SS:fff"));

                //Thread.Sleep(100);
            }
        }

        private void SendToPLC(Byte[] data)
        {

            if (!socketWrapper.IsConnected) socketWrapper.Connect();

            List<byte> values = new List<byte>(255);
            values.AddRange(data);

            Console.WriteLine("发送:" + DateTime.Now.ToString("yyyy-MM-dd HH:MM:SS:fff"));
            socketWrapper.Write(values.ToArray());

            //[4].防止连续读写引起前台UI线程阻塞00 
            Application.DoEvents();
            //[5].读取Response: 写完后会返回12个byte的结果
            byte[] responseHeader = socketWrapper.Read(12);
            Console.WriteLine("接收:" + DateTime.Now.ToString("yyyy-MM-dd HH:MM:SS:fff"));

        }

        private void ClearTextButton_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "";
        }



    }

    public class MoverMonitor
    {
        private string severState = "未启动";

        [Category("服务端")]
        [ReadOnly(true)]
        protected string SeverState
        {
            get { return severState; }
            set { severState = value; }
        }
    }

    [assembly: log4net.Config.XmlConfigurator(Watch = true)]
    public class MoverLog
    {
        private ILog log = null;
        private string logInfo = "";

        public string LogInfo
        {
            get { return logInfo; }
            set { logInfo = value; }
        }

        public MoverLog()
        {
            log = log4net.LogManager.GetLogger("visClient.Logging");
            Debug.Assert(log != null);
        }

        public MoverLog(string logname)
        {
            log = log4net.LogManager.GetLogger(logname);
            Debug.Assert(log != null);
        }

        public void FrameCapture(int i)
        {
            logInfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "frame" + i.ToString() + " captured";
            log.Info(logInfo);
        }

        public void VideoWriting(int i)
        {
            logInfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "video" + i.ToString() + " writing...";
            log.Info(logInfo);
        }

        public void DisplayBroadCastInfo(string msg)
        {
            logInfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "BroadCastingMessage--" + msg;
            log.Info(logInfo);
        }

        public void DisplaySendToServerInfo(string msg)
        {
            logInfo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "send to server info--" + msg;
            log.Info(logInfo);
        }

    }

}
