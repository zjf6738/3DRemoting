using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Configuration;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using Qzeim.ThrdPrint.BroadCast.Common;
using log4net;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using Newtonsoft.Json;

namespace CameraCapture
{
    [assembly: log4net.Config.XmlConfigurator(Watch = true)]
    public partial class CameraCapture2 : Form, IBroadCastHandler
    {
        // 视频采集
        private VideoCapture capture0 = null;
        private VideoCapture capture1 = null;
        private VideoCapture capture2 = null;
        private VideoCapture capture3 = null;

        // 采集的图片
        private Mat frame0 = null;
        private Mat frame1 = null;
        private Mat frame2 = null;
        private Mat frame3 = null;

        // 采集及录制状态状态
        private bool captureInProgress = false;
        private bool videoInProgress = false;

        // 视频写
        private VideoWriter vw0 = null;
        private VideoWriter vw1 = null;
        private VideoWriter vw2 = null;
        private VideoWriter vw3 = null;

        // 通信模块
        private VisComm visComm = null;

        // 信息监控
        private VisMonitor visMonitor = null;

        // 日志记录
        private VisLog visLog = null;


        #region 初始化
        public CameraCapture2()
        {
            InitializeComponent();

            // 通信
            visComm = new VisComm(this);
            // 日志
            visLog = new VisLog();
            // 监控
            visMonitor = new VisMonitor();

        }

        private void CameraCapture2_Load(object sender, EventArgs e)
        {
            // 初始化各个图像对象
            CvInvoke.UseOpenCL = false;

            try
            {
                capture0 = new VideoCapture(0);
                capture0.ImageGrabbed += ProcessFrame0;

                capture1 = new VideoCapture(1);
                capture1.ImageGrabbed += ProcessFrame1;

                capture2 = new VideoCapture(2);
                capture2.ImageGrabbed += ProcessFrame2;

                capture3 = new VideoCapture(3);
                capture3.ImageGrabbed += ProcessFrame3;

                frame0 = new Mat();
                frame1 = new Mat();
                frame2 = new Mat();
                frame3 = new Mat();

            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }

            // UI更新
            propertyGrid1.SelectedObject = visMonitor;

            // 通信
            visComm.StartClient();
        }

        private void CameraCapture2_FormClosing(object sender, FormClosingEventArgs e)
        {
            visComm.FinishClient();
        }

        #endregion



        #region 图像采集及处理
        private void ProcessFrame0(object sender, EventArgs arg)
        {
            if (capture0 != null && capture0.Ptr != IntPtr.Zero)
            {
                capture0.Retrieve(frame0, 0);
                imageBox0.Image = frame0;

                if (videoInProgress)
                {
                    vw0.Write(frame0);
                }

                UpdateInfoByCapture(0);
            }
        }

        private void ProcessFrame1(object sender, EventArgs arg)
        {
            if (capture1 != null && capture1.Ptr != IntPtr.Zero)
            {
                capture1.Retrieve(frame1, 0);
                imageBox1.Image = frame1;

                if (videoInProgress)
                {
                    vw1.Write(frame1);
                }

                UpdateInfoByCapture(1);

            }
        }

        private void ProcessFrame2(object sender, EventArgs arg)
        {
            if (capture2 != null && capture2.Ptr != IntPtr.Zero)
            {
                capture2.Retrieve(frame2, 0);
                imageBox2.Image = frame2;

                if (videoInProgress)
                {
                    vw2.Write(frame2);
                }

                UpdateInfoByCapture(2);

            }
        }

        private void ProcessFrame3(object sender, EventArgs arg)
        {
            if (capture3 != null && capture3.Ptr != IntPtr.Zero)
            {
                capture3.Retrieve(frame3, 0);
                imageBox3.Image = frame3;

                if (videoInProgress)
                {
                    vw3.Write(frame3);
                }

                UpdateInfoByCapture(3);

            }
        }

        private void UpdateInfoByCapture(int frameId)
        {
            visLog.FrameCapture(frameId);
            if (videoInProgress)
            {
                visLog.VideoWriting(frameId);
            }

            long dt = DateTime.Now.Ticks;
            switch (frameId)
            {
                case 0:
                    visMonitor.相机1采集的总图像数++; break;
                case 1:
                    visMonitor.相机2采集的总图像数++; break;
                case 2:
                    visMonitor.相机3采集的总图像数++; break;
                case 3:
                    visMonitor.相机4采集的总图像数++; break;
            }

            visLog.FrameCapture(frameId);

            new Thread(Check2).Start();

        }


        public void Check2()
        {
            lock (this)
                Invoke(new MethodInvoker(delegate()
                {
                    //propertyGrid1.Update();
                    propertyGrid1.Refresh();
                }));
        }


        #endregion

        #region 通信部分

        public void OnBroadCastingInfo(string message)
        {
            CommObj commObj = CommObj.FromJson(message);

            if (commObj == null)
            {
                visComm.RcvMsg = "Json解析错误";
            }
            else
            {
                commObj.RcvTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                visComm.RcvMsg = commObj.ToString();
            }

            new Thread(Check).Start();

            visLog.DisplayBroadCastInfo(visComm.RcvMsg);
        }

        public void Check()
        {
            lock (this)
                Invoke(new MethodInvoker(delegate()
                {
                    //txtMessage.Text += "I got it:" + VisComm.RcvMsg;
                    //txtMessage.Text += System.Environment.NewLine;
                    MessageBox.Show(visComm.RcvMsg);
                }));
        }

        // 定时发送消息到服务端
        private void timer1_Tick(object sender, EventArgs e)
        {

            PLCControlObj obj = new PLCControlObj(1, 1000, 1, 1000, 1, 1000, 1, 1000);

            CommObj commObj = new CommObj();
            commObj.SrcId = 0x10;
            commObj.DestId = 0x30;
            commObj.SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            commObj.DataType = "PLCControlObj";
            commObj.DataCmd = "";
            commObj.DataBody = PLCControlObj.ToByteJson(obj);

            string json = CommObj.ToJson(commObj);

            // visComm.SendToServer(json);

            visLog.DisplaySendToServerInfo(json);

        }


        #endregion


        private void captureButton_Click(object sender, EventArgs e)
        {
            if (capture0 != null && capture1 != null && capture2 != null && capture3 != null)
            {
                if (captureInProgress)
                {  //stop the capture
                    captureButton.Text = "开始采集";
                    capture0.Pause();
                    capture1.Pause();
                    capture2.Pause();
                    capture3.Pause();
                }
                else
                {
                    //start the capture
                    captureButton.Text = "停止采集";
                    capture0.Start();
                    capture1.Start();
                    capture2.Start();
                    capture3.Start();
                }

                captureInProgress = !captureInProgress;
            }
        }

        private void snapButton_Click(object sender, EventArgs e)
        {
            string dt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            for (int i = 0; i < 4; i++)
            {
                imageBox0.Image.Save(dt+"-"+i.ToString()+".png");
            }
        }

        private void recordButton_Click(object sender, EventArgs e)
        {
            string dt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            if (recordButton.Text == "一键录制")
            {
                if (MessageBox.Show("开始录制吗？", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                {
                    videoInProgress = true;

                    // char[] codec = { 'M', 'J', 'P', 'G' };
                    char[] codec = { 'D', 'I', 'V', 'X' };
                    int fps = 25;

                    vw0 = new VideoWriter(dt+"-0.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
                        25,
                        new Size(capture0.Width, capture0.Height),
                        true);

                    vw1 = new VideoWriter(dt + "-1.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
                                            25,
                                            new Size(capture0.Width, capture0.Height),
                                            true);

                    vw2 = new VideoWriter(dt + "-2.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
                                            25,
                                            new Size(capture0.Width, capture0.Height),
                                            true);

                    vw3 = new VideoWriter(dt + "-3.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
                                            25,
                                            new Size(capture0.Width, capture0.Height),
                                            true);


                    Application.Idle += new EventHandler(ProcessFrame0);
                    Application.Idle += new EventHandler(ProcessFrame1);
                    Application.Idle += new EventHandler(ProcessFrame2);
                    Application.Idle += new EventHandler(ProcessFrame3);

                    recordButton.Text = "暂停录制";
                }


            }
            else
            {
                if (MessageBox.Show("停止录制吗？", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                {
                    videoInProgress = false;


                    vw0.Dispose();
                    vw1.Dispose();
                    vw2.Dispose();
                    vw3.Dispose();
                    Application.Idle -= new EventHandler(ProcessFrame0);
                    Application.Idle -= new EventHandler(ProcessFrame1);
                    Application.Idle -= new EventHandler(ProcessFrame2);
                    Application.Idle -= new EventHandler(ProcessFrame3);
                    recordButton.Text = "录制";
                }
            }
        }

        private void resSettingButton_Click(object sender, EventArgs e)
        {
            //capture0.SetCaptureProperty(CapProp.FrameHeight, 3088);
            //capture0.SetCaptureProperty(CapProp.FrameWidth, 2064);
            //capture0.SetCaptureProperty(CapProp.FrameHeight, 3088);
        }

        private void commTestButton_Click(object sender, EventArgs e)
        {
            PLCControlObj obj = new PLCControlObj(0, 1000, 1, 1000, 1, 1000, 1, 1000);

            CommObj commObj = new CommObj();
            commObj.SrcId = 0x10;
            commObj.DestId = 0x30;
            commObj.SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            commObj.DataType = "PLCControlObj";
            commObj.DataCmd = "";
            commObj.DataBody = PLCControlObj.ToByteJson(obj);

            string json = CommObj.ToJson(commObj);

            int N = 20;

            for (int i = 0; i < N; i++)
            {
                visComm.SendToServer(json);
                Thread.Sleep(1000);
            }

        }

        

    }

    public class VisMonitor
    {
        protected string severState = "未启动";
        protected int numOfBroadCastEventObserver = 0;
        protected int numOfVisUpCastEventObserver = 0;
        protected int numberOfServerSend = 0;
        protected int numberOfServerRcv = 0;
        protected int numberOfVisSend = 0;
        protected int numberOfVisRcv = 0;

        // 显示采集的帧数
        protected uint numberImagesCapturedByFrame0 = 0;
        protected uint numberImagesCapturedByFrame1 = 0;
        protected uint numberImagesCapturedByFrame2 = 0;
        protected uint numberImagesCapturedByFrame3 = 0;

        // 更新实时帧率用
        protected uint lastNumberImagesCapturedByFrame0 = 0;
        protected uint lastNumberImagesCapturedByFrame1 = 0;
        protected uint lastNumberImagesCapturedByFrame2 = 0;
        protected uint lastNumberImagesCapturedByFrame3 = 0;

        // 上一帧的时间
        protected long lastTicksOfFrame0 = DateTime.Now.Ticks;
        protected long lastTicksOfFrame1 = DateTime.Now.Ticks;
        protected long lastTicksOfFrame2 = DateTime.Now.Ticks;
        protected long lastTicksOfFrame3 = DateTime.Now.Ticks;

        // 实时帧率
        protected double fpsOfFrame0 = 0;
        protected double fpsOfFrame1 = 0;
        protected double fpsOfFrame2 = 0;
        protected double fpsOfFrame3 = 0;

        [Category("服务端")]
        [ReadOnly(true)]
        public string A0_服务端状态
        {
            get { return severState; }
            set { severState = value; }
        }

        [Category("视觉端")]
        [ReadOnly(true)]
        public int A1_视觉端事件观察者个数
        {
            get { return numOfVisUpCastEventObserver; }
            set { numOfVisUpCastEventObserver = value; }
        }

        [Category("视觉端")]
        [ReadOnly(true)]
        public int A2_视觉端发送次数
        {
            get { return numberOfVisSend; }
            set { numberOfVisSend = value; }
        }

        [Category("视觉端")]
        [ReadOnly(true)]
        public int A3_视觉端接收次数
        {
            get { return numberOfVisRcv; }
            set { numberOfVisRcv = value; }
        }

        [Category("相机1-1")]
        [ReadOnly(true)]
        public uint 相机1采集的总图像数
        {
            get { return numberImagesCapturedByFrame0; }
            set { numberImagesCapturedByFrame0 = value; }
        }

        [Category("相机2-1")]
        [ReadOnly(true)]
        public uint 相机2采集的总图像数
        {
            get { return numberImagesCapturedByFrame1; }
            set { numberImagesCapturedByFrame1 = value; }
        }
        [Category("相机3-1")]
        [ReadOnly(true)]
        public uint 相机3采集的总图像数
        {
            get { return numberImagesCapturedByFrame2; }
            set { numberImagesCapturedByFrame2 = value; }
        }

        [Category("相机4-1")]
        [ReadOnly(true)]
        public uint 相机4采集的总图像数
        {
            get { return numberImagesCapturedByFrame3; }
            set { numberImagesCapturedByFrame3 = value; }
        }

        [Category("相机1-1")]
        [ReadOnly(true)]
        [Browsable(false)]
        public double 相机1的实时帧率
        {
            get { return fpsOfFrame0; }
            set { fpsOfFrame0 = value; }
        }

        [Category("相机2-1")]
        [ReadOnly(true)]
        [Browsable(false)]
        public double 相机2的实时帧率
        {
            get { return fpsOfFrame1; }
            set { fpsOfFrame1 = value; }
        }

        [Category("相机3-1")]
        [ReadOnly(true)]
        [Browsable(false)]
        public double 相机3的实时帧率
        {
            get { return fpsOfFrame2; }
            set { fpsOfFrame2 = value; }
        }

        [Category("相机4-1")]
        [ReadOnly(true)]
        [Browsable(false)]
        public double 相机4的实时帧率
        {
            get { return fpsOfFrame3; }
            set { fpsOfFrame3 = value; }
        }

        [Browsable(false)]
        public uint LastNumberImagesCapturedByFrame0
        {
            get { return lastNumberImagesCapturedByFrame0; }
            set { lastNumberImagesCapturedByFrame0 = value; }
        }

        [Browsable(false)]
        public uint LastNumberImagesCapturedByFrame1
        {
            get { return lastNumberImagesCapturedByFrame1; }
            set { lastNumberImagesCapturedByFrame1 = value; }
        }

        [Browsable(false)]
        public uint LastNumberImagesCapturedByFrame2
        {
            get { return lastNumberImagesCapturedByFrame2; }
            set { lastNumberImagesCapturedByFrame2 = value; }
        }

        [Browsable(false)]
        public uint LastNumberImagesCapturedByFrame3
        {
            get { return lastNumberImagesCapturedByFrame3; }
            set { lastNumberImagesCapturedByFrame3 = value; }
        }

        [Browsable(false)]
        public long LastTicksOfFrame0
        {
            get { return lastTicksOfFrame0; }
            set { lastTicksOfFrame0 = value; }
        }

        [Browsable(false)]
        public long LastTicksOfFrame1
        {
            get { return lastTicksOfFrame1; }
            set { lastTicksOfFrame1 = value; }
        }

        [Browsable(false)]
        public long LastTicksOfFrame2
        {
            get { return lastTicksOfFrame2; }
            set { lastTicksOfFrame2 = value; }
        }

        [Browsable(false)]
        public long LastTicksOfFrame3
        {
            get { return lastTicksOfFrame3; }
            set { lastTicksOfFrame3 = value; }
        }
    }
}
