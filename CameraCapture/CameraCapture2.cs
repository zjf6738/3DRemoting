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
using MVSDK;
using CameraHandle = System.Int32;
using MvApi = MVSDK.MvApi;
using Newtonsoft.Json;

namespace CameraCapture
{
   
    [assembly: log4net.Config.XmlConfigurator(Watch = true)]
    public partial class CameraCapture2 : Form, IBroadCastHandler
    {

        // 视觉模块
        private VisEmguVision2 theVisEmguVision; // opencv视觉系统
        private VisMindVision2 theVisMindVision = null; // 迈德威视视觉系统

        // 通信模块
        private VisComm visComm = null;
        // 信息监控
        private VisMonitor visMonitor = null;
        // 日志记录
        private VisLog3 visLog = null;
        // UI更新
        private VisUI visUI = null;

        // 方法响应函数对象
        CameraCapture2MethodHandlerComposite methodHandlerComposite = null;

        // 测试
        private int allTimerCount = 1;

        #region 初始化
        public CameraCapture2()
        {
            InitializeComponent();

            // 通信
            visComm = new VisComm(this);
            // 日志
            visLog = new VisLog3();
            // 监控
            visMonitor = new VisMonitor();
            // 视觉
            theVisEmguVision = new VisEmguVision2(this);
            theVisMindVision = new VisMindVision2(this);
        }

        public VisEmguVision2 TheVisEmguVision
        {
            get { return theVisEmguVision; }
        }

        private void CameraCapture2_Load(object sender, EventArgs e)
        {
            // 初始化视觉设备 
            theVisEmguVision.Start();

            // UI更新
            propertyGrid1.SelectedObject = visMonitor;

            // 通信
            try
            {
                visComm.StartClient();
            }
            catch (Exception ex)
            {
                this.textBox1.AppendText(ex.Message);
                MessageBox.Show(ex.Message + "请检查远程服务端设置是否正确！");
            }

        }

        private void CameraCapture2_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                visComm.FinishClient();
            }
            catch (Exception ex)
            {
                
                this.textBox1.AppendText(ex.Message);
            }

        }

        #endregion



        //#region 图像采集及处理
        //private void ProcessFrame0(object sender, EventArgs arg)
        //{
        //    if (VisEmguVision2.capture0 != null && VisEmguVision2.capture0.Ptr != IntPtr.Zero)
        //    {
        //        VisEmguVision2.capture0.Retrieve(VisEmguVision2.frame0, 0);
        //        imageBox0.Image = VisEmguVision2.frame0;

        //        if (VisEmguVision2.videoInProgress)
        //        {
        //            VisEmguVision2.vw0.Write(VisEmguVision2.frame0);
        //        }

        //        UpdateInfoByCapture(0);
        //    }
        //}

        //private void ProcessFrame1(object sender, EventArgs arg)
        //{
        //    if (VisEmguVision2.capture1 != null && VisEmguVision2.capture1.Ptr != IntPtr.Zero)
        //    {
        //        VisEmguVision2.capture1.Retrieve(VisEmguVision2.frame1, 0);
        //        imageBox1.Image = VisEmguVision2.frame1;

        //        if (VisEmguVision2.videoInProgress)
        //        {
        //            VisEmguVision2.vw1.Write(VisEmguVision2.frame1);
        //        }

        //        UpdateInfoByCapture(1);

        //    }
        //}

        //private void ProcessFrame2(object sender, EventArgs arg)
        //{
        //    if (VisEmguVision2.capture2 != null && VisEmguVision2.capture2.Ptr != IntPtr.Zero)
        //    {
        //        VisEmguVision2.capture2.Retrieve(VisEmguVision2.frame2, 0);
        //        imageBox2.Image = VisEmguVision2.frame2;

        //        if (VisEmguVision2.videoInProgress)
        //        {
        //            VisEmguVision2.vw2.Write(VisEmguVision2.frame2);
        //        }

        //        UpdateInfoByCapture(2);

        //    }
        //}

        //private void ProcessFrame3(object sender, EventArgs arg)
        //{
        //    if (VisEmguVision2.capture3 != null && VisEmguVision2.capture3.Ptr != IntPtr.Zero)
        //    {
        //        VisEmguVision2.capture3.Retrieve(VisEmguVision2.frame3, 0);
        //        imageBox3.Image = VisEmguVision2.frame3;

        //        if (VisEmguVision2.videoInProgress)
        //        {
        //            VisEmguVision2.vw3.Write(VisEmguVision2.frame3);
        //        }

        //        UpdateInfoByCapture(3);

        //    }
        //}

        //private void UpdateInfoByCapture(int frameId)
        //{
        //    visLog.FrameCapture(frameId);
        //    if (VisEmguVision2.videoInProgress)
        //    {
        //        visLog.VideoWriting(frameId);
        //    }

        //    long dt = DateTime.Now.Ticks;
        //    switch (frameId)
        //    {
        //        case 0:
        //            visMonitor.相机1采集的总图像数++; break;
        //        case 1:
        //            visMonitor.相机2采集的总图像数++; break;
        //        case 2:
        //            visMonitor.相机3采集的总图像数++; break;
        //        case 3:
        //            visMonitor.相机4采集的总图像数++; break;
        //    }

        //    visLog.FrameCapture(frameId);

        //    new Thread(Check2).Start();

        //}


        //public void Check2()
        //{
        //    lock (this)
        //        Invoke(new MethodInvoker(delegate()
        //        {
        //            //propertyGrid1.Update();
        //            propertyGrid1.Refresh();
        //        }));
        //}


        //#endregion

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
                    //MessageBox.Show(visComm.RcvMsg);
                    this.textBox1.Text += visComm.RcvMsg;
                }));
        }

        // 定时发送消息到服务端
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (allTimerCount % 20 == 0) { this.timer1.Enabled = false; }
            allTimerCount++;


            PLCControlObj obj = new PLCControlObj(0, 500, 0, 0, 0, 0, 0, 0);

            CommObj commObj = new CommObj();
            commObj.SrcId = 0x10;
            commObj.DestId = 0x30;
            commObj.SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            commObj.DataType = "PLCControlObj";
            commObj.DataCmd = "";
            commObj.DataBody = PLCControlObj.ToByteJson(obj);

            string json = CommObj.ToJson(commObj);
            visComm.SendToServer(json);

            // visComm.SendToServer(json);
            visLog.DisplaySendToServerInfo(json);

        }


        #endregion


        private void captureButton_Click(object sender, EventArgs e)
        {
            theVisEmguVision.OnCapture();

        }

        private void snapButton_Click(object sender, EventArgs e)
        {
            //string dt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            //for (int i = 0; i < 4; i++)
            //{
            //    imageBox0.Image.Save(dt+"-"+i.ToString()+".png");
            //}
            theVisEmguVision.OnSnap(sender,e);

        }

        private void recordButton_Click(object sender, EventArgs e)
        {
            theVisEmguVision.OnRecord(sender,e);

            //string dt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            //if (recordButton.Text == "一键录制")
            //{
            //    if (MessageBox.Show("开始录制吗？", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
            //    {
            //        VisEmguVision2.videoInProgress = true;

            //        // char[] codec = { 'M', 'J', 'P', 'G' };
            //        char[] codec = { 'D', 'I', 'V', 'X' };
            //        int fps = 25;

            //        VisEmguVision2.vw0 = new VideoWriter(dt+"-0.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
            //            25,
            //            new Size(VisEmguVision2.capture0.Width, VisEmguVision2.capture0.Height),
            //            true);

            //        VisEmguVision2.vw1 = new VideoWriter(dt + "-1.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
            //                                25,
            //                                new Size(VisEmguVision2.capture0.Width, VisEmguVision2.capture0.Height),
            //                                true);

            //        VisEmguVision2.vw2 = new VideoWriter(dt + "-2.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
            //                                25,
            //                                new Size(VisEmguVision2.capture0.Width, VisEmguVision2.capture0.Height),
            //                                true);

            //        VisEmguVision2.vw3 = new VideoWriter(dt + "-3.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
            //                                25,
            //                                new Size(VisEmguVision2.capture0.Width, VisEmguVision2.capture0.Height),
            //                                true);


            //        Application.Idle += new EventHandler(ProcessFrame0);
            //        Application.Idle += new EventHandler(ProcessFrame1);
            //        Application.Idle += new EventHandler(ProcessFrame2);
            //        Application.Idle += new EventHandler(ProcessFrame3);

            //        recordButton.Text = "暂停录制";
            //    }


            //}
            //else
            //{
            //    if (MessageBox.Show("停止录制吗？", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
            //    {
            //        VisEmguVision2.videoInProgress = false;


            //        VisEmguVision2.vw0.Dispose();
            //        VisEmguVision2.vw1.Dispose();
            //        VisEmguVision2.vw2.Dispose();
            //        VisEmguVision2.vw3.Dispose();
            //        Application.Idle -= new EventHandler(ProcessFrame0);
            //        Application.Idle -= new EventHandler(ProcessFrame1);
            //        Application.Idle -= new EventHandler(ProcessFrame2);
            //        Application.Idle -= new EventHandler(ProcessFrame3);
            //        recordButton.Text = "录制";
            //    }
            //}
        }

        private void commTestButton_Click(object sender, EventArgs e)
        {
            //PLCControlObj obj = new PLCControlObj(0, 1000, 1, 1000, 1, 1000, 1, 1000);

            //CommObj commObj = new CommObj();
            //commObj.SrcId = 0x10;
            //commObj.DestId = 0x30;
            //commObj.SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            //commObj.DataType = "PLCControlObj";
            //commObj.DataCmd = "";
            //commObj.DataBody = PLCControlObj.ToByteJson(obj);

            //string json = CommObj.ToJson(commObj);

            //int N = 20;

            //for (int i = 0; i < N; i++)
            //{
            //    visComm.SendToServer(json);
            //    Thread.Sleep(1000);
            //}
            this.timer1.Enabled = true;

            //new Thread(CheckCommTestButton).Start();

        }

        public void CheckCommTestButton()
        {
            lock (this)
                Invoke(new MethodInvoker(delegate()
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
                }));
        }

        private void cameraSettingButton_Click(object sender, EventArgs e)
        {
            theVisEmguVision.End();
            Thread.Sleep(500);
            //theVisMindVision.Start();

            //theVisMindVision.SetCameraAttris(0);
            //theVisMindVision.SetCameraAttris(1);
            //theVisMindVision.SetCameraAttris(2);
            //theVisMindVision.SetCameraAttris(3);

            //theVisMindVision.End();
            Thread.Sleep(500);
            theVisEmguVision.Start();
        }

        

    }

    // 视觉模块
    public class VisEmguVisionState
    {
        public enum StateEnum
        {
            Starting = 0,
            Capturing,
            Recording,
            Abnormal,
            Finished
        };
        public int state = 0; // 0:开始状态，1：采集状态，2：录制状态，3:异常状态，4：结束状态

        public string GetStateString()
        {
            switch (state)
            {
                case (int)global::CameraCapture.VisEmguVisionState.StateEnum.Starting: return "开始状态";
                case (int)global::CameraCapture.VisEmguVisionState.StateEnum.Capturing: return "采集状态";
                case (int)global::CameraCapture.VisEmguVisionState.StateEnum.Recording: return "录制状态";
                case (int)global::CameraCapture.VisEmguVisionState.StateEnum.Abnormal: return "异常状态";
                case (int)global::CameraCapture.VisEmguVisionState.StateEnum.Finished: return "结束状态";
                default: return "未知状态";
            }
        }
        public void SetState(int _state) { state = _state; }
        public int GetState() { return state; }

    }


    public class VisEmguVision2
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
        // 视频写
        private VideoWriter vw0 = null;
        private VideoWriter vw1 = null;
        private VideoWriter vw2 = null;
        private VideoWriter vw3 = null;

        // 采集及录制状态状态
        private bool captureInProgress = false;
        private bool videoInProgress = false;
        private char[] codec = { 'D', 'I', 'V', 'X' };
        private int fps = 25;

        // 当前的工作相机个数
        private int numberOfAvailableCamera = 0;

        // 当前视觉系统的工作状态
        private VisEmguVisionState visEmguState;

        // 父窗口
        private CameraCapture2 frm = null;


            

        public VisEmguVision2(CameraCapture2 _frm)
        {
            frm = _frm;
            //theVisMindVision = new VisMindVision2(frm.Handle);
        }

        #region 属性
        public VideoCapture Capture0
        {
            get { return capture0; }
            set { capture0 = value; }
        }

        public VideoCapture Capture1
        {
            get { return capture1; }
            set { capture1 = value; }
        }

        public VideoCapture Capture2
        {
            get { return capture2; }
            set { capture2 = value; }
        }

        public VideoCapture Capture3
        {
            get { return capture3; }
            set { capture3 = value; }
        }

        public Mat Frame0
        {
            get { return frame0; }
            set { frame0 = value; }
        }

        public Mat Frame1
        {
            get { return frame1; }
            set { frame1 = value; }
        }

        public Mat Frame2
        {
            get { return frame2; }
            set { frame2 = value; }
        }

        public Mat Frame3
        {
            get { return frame3; }
            set { frame3 = value; }
        }

        public bool CaptureInProgress
        {
            get { return captureInProgress; }
            set { captureInProgress = value; }
        }

        public bool VideoInProgress
        {
            get { return videoInProgress; }
            set { videoInProgress = value; }
        }

        public VideoWriter Vw0
        {
            get { return vw0; }
            set { vw0 = value; }
        }

        public VideoWriter Vw1
        {
            get { return vw1; }
            set { vw1 = value; }
        }

        public VideoWriter Vw2
        {
            get { return vw2; }
            set { vw2 = value; }
        }

        public VideoWriter Vw3
        {
            get { return vw3; }
            set { vw3 = value; }
        }

        public int NumberOfAvailableCamera
        {
            get { return numberOfAvailableCamera; }
            set { numberOfAvailableCamera = value; }
        }

        public VisEmguVisionState VisEmguState
        {
            get { return visEmguState; }
            set { visEmguState = value; }
        }

        #endregion


        public void Start()
        {
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
                throw ;
            }
        }

        public void End()
        {
            try
            {
                capture0.Stop();
                capture1.Stop();
                capture2.Stop();
                capture3.Stop();

                //capture0.ImageGrabbed -= ProcessFrame0;
                //capture0 = null;

                //capture1.ImageGrabbed -= ProcessFrame1;
                //capture1 = null;

                //capture2.ImageGrabbed -= ProcessFrame2;
                //capture2 = null;

                //capture3.ImageGrabbed -= ProcessFrame3;
                //capture3 = null;

                capture0.Dispose();
                capture1.Dispose();
                capture2.Dispose();
                capture3.Dispose();


                frame0.Dispose();
                frame1.Dispose();
                frame2.Dispose();
                frame3.Dispose();

                //frame0 = null;
                //frame1 = null;
                //frame2 = null;
                //frame3 = null;
            }
            catch (Exception ex)
            {
                
                throw;
            }

        }


        #region 图像采集及处理
        private void ProcessFrame0(object sender, EventArgs arg)
        {
            if (capture0 != null && capture0.Ptr != IntPtr.Zero)
            {
                capture0.Retrieve(frame0, 0);

                frm.ImageBox0.Image = frame0;
                ProcessVideo(vw0, frame0);
            }
        }

        private void ProcessFrame1(object sender, EventArgs arg)
        {
            if (capture1 != null && capture1.Ptr != IntPtr.Zero)
            {
                capture1.Retrieve(frame1, 0);
                frm.ImageBox1.Image = frame1;
                ProcessVideo(vw1, frame1);
            }
        }

        private void ProcessFrame2(object sender, EventArgs arg)
        {
            if (capture2 != null && capture2.Ptr != IntPtr.Zero)
            {
                capture2.Retrieve(frame2, 0);
                frm.ImageBox2.Image = frame2;
                ProcessVideo(vw2, frame2);
            }
        }

        private void ProcessFrame3(object sender, EventArgs arg)
        {
            if (capture3 != null && capture3.Ptr != IntPtr.Zero)
            {
                capture3.Retrieve(frame3, 0);
                frm.ImageBox3.Image = frame3;
                ProcessVideo(vw3,frame3);
            }
        }

        private void ProcessVideo(VideoWriter vw,Mat frame)
        {
            if (videoInProgress)
            {
                vw.Write(frame);
            }
        }

        #endregion


        public void OnCapture()
        {
            if (capture0 != null && capture1 != null && capture2 != null && capture3 != null)
            {
                if (captureInProgress)
                {  //stop the capture
                    //captureButton.Text = "开始采集";
                    capture0.Pause();
                    capture1.Pause();
                    capture2.Pause();
                    capture3.Pause();
                }
                else
                {
                    //start the capture
                    //captureButton.Text = "停止采集";
                    capture0.Start();
                    capture1.Start();
                    capture2.Start();
                    capture3.Start();
                }

                captureInProgress = !captureInProgress;
            }
        }

        public void OnSnap(object sender, EventArgs e)
        {

        }

        public void OnRecord(object sender, EventArgs e)
        {
            string dt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            if (videoInProgress)
            {
                vw0 = new VideoWriter(dt + "-0.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
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
            }
            else
            {
                vw0.Dispose();
                vw1.Dispose();
                vw2.Dispose();
                vw3.Dispose();
                Application.Idle -= new EventHandler(ProcessFrame0);
                Application.Idle -= new EventHandler(ProcessFrame1);
                Application.Idle -= new EventHandler(ProcessFrame2);
                Application.Idle -= new EventHandler(ProcessFrame3);
            }

            videoInProgress = !videoInProgress;



            //if (recordButton.Text == "一键录制")
            //{
            //    if (MessageBox.Show("开始录制吗？", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
            //    {
            //        VisEmguVision2.videoInProgress = true;

            //        // char[] codec = { 'M', 'J', 'P', 'G' };
            //        char[] codec = { 'D', 'I', 'V', 'X' };
            //        int fps = 25;

            //        vw0 = new VideoWriter(dt + "-0.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
            //            25,
            //            new Size(capture0.Width,capture0.Height),
            //            true);

            //        vw1 = new VideoWriter(dt + "-1.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
            //                                25,
            //                                new Size(capture0.Width, capture0.Height),
            //                                true);

            //        vw2 = new VideoWriter(dt + "-2.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
            //                                25,
            //                                new Size(capture0.Width, capture0.Height),
            //                                true);

            //        vw3 = new VideoWriter(dt + "-3.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
            //                                25,
            //                                new Size(capture0.Width, capture0.Height),
            //                                true);


            //        Application.Idle += new EventHandler(ProcessFrame0);
            //        Application.Idle += new EventHandler(ProcessFrame1);
            //        Application.Idle += new EventHandler(ProcessFrame2);
            //        Application.Idle += new EventHandler(ProcessFrame3);

            //    }


            //}
            //else
            //{
            //    if (MessageBox.Show("停止录制吗？", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
            //    {
            //        VisEmguVision2.videoInProgress = false;


            //        vw0.Dispose();
            //        vw1.Dispose();
            //        vw2.Dispose();
            //        vw3.Dispose();
            //        Application.Idle -= new EventHandler(ProcessFrame0);
            //        Application.Idle -= new EventHandler(ProcessFrame1);
            //        Application.Idle -= new EventHandler(ProcessFrame2);
            //        Application.Idle -= new EventHandler(ProcessFrame3);
            //        recordButton.Text = "录制";
            //    }
            //}
        }
    }

    // MindVision视觉系统
    public class VisMindVision2
    {
        const int CAMERA_NUM = 4;

        protected IntPtr[] m_Grabber = new IntPtr[CAMERA_NUM];
        protected CameraHandle[] m_hCamera = new CameraHandle[CAMERA_NUM];
        protected tSdkCameraDevInfo[] m_DevInfo;
        protected pfnCameraGrabberFrameCallback[] m_FrameCallback = new pfnCameraGrabberFrameCallback[CAMERA_NUM];
        protected pfnCameraGrabberSaveImageComplete m_SaveImageComplete;

        protected CameraCapture2 frm = null;
        protected IntPtr hDispWnd = IntPtr.Zero;

        public VisMindVision2(CameraCapture2 _frm)
        {
            frm = _frm;
            hDispWnd = _frm.Handle;
        }

        public void Start()
        {
            MvApi.CameraEnumerateDevice(out m_DevInfo);
            int NumDev = (m_DevInfo != null ? Math.Min(m_DevInfo.Length, CAMERA_NUM) : 0);


            IntPtr[] hDispWnds = { frm.ImageBox0.Handle, frm.ImageBox1.Handle, frm.ImageBox2.Handle, frm.ImageBox3.Handle };
            for (int i = 0; i < NumDev; ++i)
            {
                if (MvApi.CameraGrabber_Create(out m_Grabber[i], ref m_DevInfo[i]) == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                {
                    MvApi.CameraGrabber_GetCameraHandle(m_Grabber[i], out m_hCamera[i]);
                    MvApi.CameraCreateSettingPage(m_hCamera[i], frm.Handle, m_DevInfo[i].acFriendlyName, null, (IntPtr)0, 0);
                }
            }
        }

        public void End()
        {
            for (int i = 0; i < CAMERA_NUM; ++i)
            {
                if (m_Grabber[i] != IntPtr.Zero)
                    MvApi.CameraGrabber_Destroy(m_Grabber[i]);
            }
        }

        public void SetCameraAttris(int cameraIdx)
        {
            Debug.Assert(cameraIdx >= 0 && cameraIdx < 4);

            if (m_Grabber[cameraIdx] != IntPtr.Zero)
                MvApi.CameraShowSettingPage(m_hCamera[cameraIdx], 1);
        }


    }



    #region 服务器功能接口
    // 主要用以响应CameraCapture2中的各个函数
    public interface ICameraCapture2MethodHandler
    {
        // 开启客户端
        void OnStartClient();
        // 暂停客户端
        void OnPauseClient();
        // 恢复客户端
        void OnResumeClient();
        // 结束客户端
        void OnFinishClient();
        // 上传数据
        void OnUpCastEvent(string msg);
        // 响应广播事件
        void OnBroadCastMessage(string msg);
        // 清理文本
        void OnClearText();
        // 异常处理
        void OnException(string point, string msg);
        // 处理第0帧
        void OnProcessFrame0(string msg);
        // 处理第1帧
        void OnProcessFrame1(string msg);
        // 处理第2帧
        void OnProcessFrame2(string msg);
        // 处理第3帧
        void OnProcessFrame3(string msg);
        // 相机采集
        void OnCapture(string msg);
        // 一键截图
        void OnSnap(string msg);
        // 一键录制
        void OnRecord(string msg);

    }

    public class CameraCapture2MethodHandler : ICameraCapture2MethodHandler
    {

        #region 实用文本格式
        protected string startServerFmt = "%%%%服务端已启动!%%%%";
        protected string finishServerFmt = "%%%%服务端已停止!%%%%";
        protected string pauseServerFmt = "%%%%服务端已暂停!%%%%";
        protected string resumeServerFmt = "%%%%服务端已恢复!%%%%";
        protected string broadCastMessageFmt = "####服务端广播的信息####\r\n{0}";
        protected string visUpCastMessageFmt = "----视觉端发送信息----\r\n{0}";
        protected string robotUpCastMessageFmt = "----机器人端发送信息----\r\n{0}";
        protected string moverUpCastMessageFmt = "----机器人端发送信息----\r\n{0}";
        protected string exceptionMessageFmt = "！！！！异常信息！！！！\r\n异常位置：{0}\r\n异常信息：{1}";

        protected virtual string StartServerFmt
        {
            get { return startServerFmt; }
            set { startServerFmt = value; }
        }

        protected virtual string FinishServerFmt
        {
            get { return finishServerFmt; }
            set { finishServerFmt = value; }
        }

        protected virtual string PauseServerFmt
        {
            get { return pauseServerFmt; }
            set { pauseServerFmt = value; }
        }

        protected virtual string ResumeServerFmt
        {
            get { return resumeServerFmt; }
            set { resumeServerFmt = value; }
        }

        protected virtual string BroadCastMessageFmt
        {
            get { return broadCastMessageFmt; }
            set { broadCastMessageFmt = value; }
        }

        protected virtual string VisUpCastMessageFmt
        {
            get { return visUpCastMessageFmt; }
            set { visUpCastMessageFmt = value; }
        }

        protected virtual string RobotUpCastMessageFmt
        {
            get { return robotUpCastMessageFmt; }
            set { robotUpCastMessageFmt = value; }
        }

        protected virtual string MoverUpCastMessageFmt
        {
            get { return moverUpCastMessageFmt; }
            set { moverUpCastMessageFmt = value; }
        }

        protected virtual string ExceptionMessageFmt
        {
            get { return exceptionMessageFmt; }
            set { exceptionMessageFmt = value; }
        }

        #endregion

        public CameraCapture2MethodHandler() { }

        public virtual void OnStartClient() { }
        public virtual void OnPauseClient() { }
        public virtual void OnResumeClient() { }
        public virtual void OnFinishClient() { }
        public virtual void OnBroadCastMessage(string msg) { }
        public virtual void OnUpCastEvent(string msg) { }
        public virtual void OnClearText() { }
        public virtual void OnException(string point, string msg) { }
        public virtual void OnProcessFrame0(string msg) { }
        public virtual void OnProcessFrame1(string msg) { }
        public virtual void OnProcessFrame2(string msg) { }
        public virtual void OnProcessFrame3(string msg) { }

        public virtual void OnCapture(string msg) { }
        public virtual void OnSnap(string msg) { }
        public virtual void OnRecord(string msg) { }
    }

    public class CameraCapture2MethodHandlerComposite : CameraCapture2MethodHandler
    {
        private List<ICameraCapture2MethodHandler> methodHandlers = new List<ICameraCapture2MethodHandler>();

        public CameraCapture2MethodHandlerComposite() { }

        #region 基本列表操作
        public void Add(ICameraCapture2MethodHandler handler) { methodHandlers.Add(handler); }
        public void Remove(ICameraCapture2MethodHandler handler) { methodHandlers.Remove(handler); }
        public void Clear() { methodHandlers.Clear(); }
        #endregion

        public override void OnStartClient()
        {
            foreach (ICameraCapture2MethodHandler handler in methodHandlers)
            {
                handler.OnStartClient();
            }
        }

        public override void OnPauseClient()
        {
            foreach (ICameraCapture2MethodHandler handler in methodHandlers)
            {
                handler.OnPauseClient();
            }
        }

        public override void OnResumeClient()
        {
            foreach (ICameraCapture2MethodHandler handler in methodHandlers)
            {
                handler.OnResumeClient();
            }
        }

        public override void OnFinishClient()
        {
            foreach (ICameraCapture2MethodHandler handler in methodHandlers)
            {
                handler.OnFinishClient();
            }
        }

        public override void OnBroadCastMessage(string msg)
        {
            foreach (ICameraCapture2MethodHandler handler in methodHandlers)
            {
                handler.OnBroadCastMessage(msg);
            }
        }

        public override void OnUpCastEvent(string msg)
        {
            foreach (ICameraCapture2MethodHandler handler in methodHandlers)
            {
                handler.OnUpCastEvent(msg);
            }
        }

        public override void OnClearText()
        {
            foreach (ICameraCapture2MethodHandler handler in methodHandlers)
            {
                handler.OnClearText();
            }
        }

        public override void OnException(string point, string msg)
        {
            foreach (ICameraCapture2MethodHandler handler in methodHandlers)
            {
                handler.OnException(point, msg);
            }
        }

        public override void OnProcessFrame0(string msg)
        {
            foreach (ICameraCapture2MethodHandler handler in methodHandlers)
            {
                handler.OnProcessFrame0(msg);
            }
        }

        public override void OnProcessFrame1(string msg)
        {
            foreach (ICameraCapture2MethodHandler handler in methodHandlers)
            {
                handler.OnProcessFrame1(msg);
            }
        }

        public override void OnProcessFrame2(string msg)
        {
            foreach (ICameraCapture2MethodHandler handler in methodHandlers)
            {
                handler.OnProcessFrame2(msg);
            }
        }

        public override void OnProcessFrame3(string msg)
        {
            foreach (ICameraCapture2MethodHandler handler in methodHandlers)
            {
                handler.OnProcessFrame3(msg);
            }
        }

        public virtual void OnCapture(string msg)
        {
            foreach (ICameraCapture2MethodHandler handler in methodHandlers)
            {
                handler.OnCapture(msg);
            }
        }

        public virtual void OnSnap(string msg)
        {
            foreach (ICameraCapture2MethodHandler handler in methodHandlers)
            {
                handler.OnSnap(msg);
            }
        }

        public virtual void OnRecord(string msg)
        {
            foreach (ICameraCapture2MethodHandler handler in methodHandlers)
            {
                handler.OnRecord(msg);
            }
        }

    }

    #endregion


    public class VisUI : CameraCapture2MethodHandler
    {

        public VisUI() { }

        public override void OnStartClient() { }
        public override void OnPauseClient() { }
        public override void OnResumeClient() { }
        public override void OnFinishClient() { }
        public override void OnBroadCastMessage(string msg) { }
        public override void OnUpCastEvent(string msg) { }
        public override void OnClearText() { }
        public override void OnException(string point, string msg) { }
        public override void OnProcessFrame0(string msg) { }
        public override void OnProcessFrame1(string msg) { }
        public override void OnProcessFrame2(string msg) { }
        public override void OnProcessFrame3(string msg) { }

        public override void OnCapture(string msg) { }
        public override void OnSnap(string msg) { }
        public override void OnRecord(string msg) { }
    }



    public class VisMonitor:CameraCapture2MethodHandler
    {
        protected string clientState = "未启动";
        protected int numOfBroadCastEventObserver = 0;
        protected int numOfVisUpCastEventObserver = 0;
        protected int numberOfServerSend = 0;
        protected int numberOfServerRcv = 0;
        protected int numberOfVisSend = 0;
        protected int numberOfVisRcv = 0;
        protected int numberOfAvailableCaptures = 0;

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

        [Category("视觉端")]
        [ReadOnly(true)]
        public string A0_视觉端状态
        {
            get { return clientState; }
            set { clientState = value; }
        }

        [Category("视觉端")]
        [ReadOnly(true)]
        [Browsable(false)]
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
        public int A3_接收广播事件次数
        {
            get { return numberOfVisRcv; }
            set { numberOfVisRcv = value; }
        }

        [Category("视觉端")]
        [ReadOnly(true)]
        [Browsable(true)]
        [Description("可用的相机个数，正常情况下是4。否则，就存在异常。")]
        protected int NumberOfAvailableCaptures
        {
            get { return numberOfAvailableCaptures; }
            set { numberOfAvailableCaptures = value; }
        }


        [Category("相机0")]
        [ReadOnly(true)]
        public uint 相机1采集的总图像数
        {
            get { return numberImagesCapturedByFrame0; }
            set { numberImagesCapturedByFrame0 = value; }
        }

        [Category("相机1")]
        [ReadOnly(true)]
        public uint 相机2采集的总图像数
        {
            get { return numberImagesCapturedByFrame1; }
            set { numberImagesCapturedByFrame1 = value; }
        }
        [Category("相机2")]
        [ReadOnly(true)]
        public uint 相机3采集的总图像数
        {
            get { return numberImagesCapturedByFrame2; }
            set { numberImagesCapturedByFrame2 = value; }
        }

        [Category("相机3")]
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


        #region 响应视觉端业务代码
        public override void OnStartClient() { }
        public override void OnPauseClient() { }
        public override void OnResumeClient() { }
        public override void OnFinishClient() { }
        public override void OnBroadCastMessage(string msg) { }
        public override void OnUpCastEvent(string msg) { }
        public override void OnClearText() { }
        public override void OnException(string point, string msg) { }
        public override void OnProcessFrame0(string msg) { }
        public override void OnProcessFrame1(string msg) { }
        public override void OnProcessFrame2(string msg) { }
        public override void OnProcessFrame3(string msg) { }

        public override void OnCapture(string msg) { }
        public override void OnSnap(string msg) { }
        public override void OnRecord(string msg) { } 
        #endregion
    }
}
