﻿using System;
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
    public partial class CameraCapture3 : Form, IBroadCastHandler
    {

        // 视觉模块
        // private VisEmguVision3 theVisEmguVision; // opencv视觉系统
        private VisMindVision3 theVisMindVision = null; // 迈德威视视觉系统

        // 通信模块
        private VisComm visComm = null;
        // 信息监控
        private CameraCapture3Monitor visMonitor = null;
        // 日志记录
        private CameraCapture3Log visLog = null;
        // UI更新
        private CameraCapture3UI visUI = null;

        // 方法响应函数对象
        CameraCapture3MethodHandlerComposite methodHandlerComposite = null;

        // 测试
        private int allTimerCount = 1;

        #region 初始化
        public CameraCapture3()
        {
            InitializeComponent();

            // 通信
            visComm = new VisComm(this);
            // 日志
            visLog = new CameraCapture3Log();
            // 监控
            visMonitor = new CameraCapture3Monitor(this);
            // UI更新
            visUI = new CameraCapture3UI(this);

            // 视觉
            //theVisEmguVision = new VisEmguVision3(this);
            theVisMindVision = new VisMindVision3(this);

            methodHandlerComposite = new CameraCapture3MethodHandlerComposite();
            methodHandlerComposite.Add(visLog);
            methodHandlerComposite.Add(visMonitor);
            methodHandlerComposite.Add(visUI);

            propertyGrid1.SelectedObject = visMonitor;

            // 这是非线程安全地，需要进一步寻找优化方式
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;  
        }

        public VisMindVision3 TheVisMindVision
        {
            get { return theVisMindVision; }
            set { theVisMindVision = value; }
        }

        private void CameraCapture3_Load(object sender, EventArgs e)
        {
            try
            {
                theVisMindVision.Start();
                visComm.StartClient();
                methodHandlerComposite.OnStartClient();
                //UpdateControls();
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("CameraCapture3_Load",ex.Message);
            }

        }

        private void CameraCapture3_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                visComm.FinishClient();
                theVisMindVision.End();
                methodHandlerComposite.OnFinishClient();
            }
            catch (Exception ex)
            {

                methodHandlerComposite.OnException("CameraCapture3_FormClosing", ex.Message);
            }

        }

        #endregion


        #region 视觉模块
        public void CameraGrabberFrameCallback0(IntPtr Grabber, IntPtr pFrameBuffer, ref tSdkFrameHead pFrameHead, IntPtr Context)
        {
            try
            {
                theVisMindVision.CameraGrabberFrameCallback0(Grabber, pFrameBuffer, ref pFrameHead, Context);
                methodHandlerComposite.OnProcessFrame0("");
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("CameraGrabberFrameCallback0", ex.Message);
            }
        }

        public void CameraGrabberFrameCallback1(IntPtr Grabber, IntPtr pFrameBuffer, ref tSdkFrameHead pFrameHead, IntPtr Context)
        {
            try
            {
                theVisMindVision.CameraGrabberFrameCallback1(Grabber, pFrameBuffer, ref pFrameHead, Context);
                methodHandlerComposite.OnProcessFrame1("");
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("CameraGrabberFrameCallback1", ex.Message);
            }
        }

        public void CameraGrabberFrameCallback2(IntPtr Grabber, IntPtr pFrameBuffer, ref tSdkFrameHead pFrameHead, IntPtr Context)
        {
            try
            {
                theVisMindVision.CameraGrabberFrameCallback2(Grabber, pFrameBuffer, ref pFrameHead, Context);
                methodHandlerComposite.OnProcessFrame2("");
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("CameraGrabberFrameCallback2", ex.Message);
            }
        }

        public void CameraGrabberFrameCallback3(IntPtr Grabber, IntPtr pFrameBuffer, ref tSdkFrameHead pFrameHead, IntPtr Context)
        {
            try
            {
                theVisMindVision.CameraGrabberFrameCallback3(Grabber, pFrameBuffer, ref pFrameHead, Context);
                methodHandlerComposite.OnProcessFrame3("");
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("CameraGrabberFrameCallback3", ex.Message);
            }
        } 
        #endregion


        #region 通信部分

        public void OnBroadCastingInfo(string message)
        {

            try
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
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("OnBroadCastingInfo", ex.Message);
            }
            
            
        }

        public void Check()
        {
            lock (this)
                Invoke(new MethodInvoker(delegate()
                {
                    methodHandlerComposite.OnBroadCastMessage(visComm.RcvMsg);
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
            // visLog.DisplaySendToServerInfo(json);

        }


        #endregion


        private void captureButton_Click(object sender, EventArgs e)
        {
            try
            {
                theVisMindVision.OnCapture();
                methodHandlerComposite.OnCapture("");

                //UpdateControls();
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("captureButton_Click", ex.Message);
            }



        }

        private void snapButton_Click(object sender, EventArgs e)
        {

            try
            {
                theVisMindVision.OnSnap(-1);
                methodHandlerComposite.OnSnap("");
            }
            catch (Exception ex)
            {

                methodHandlerComposite.OnException("snapButton_Click", ex.Message);
            }


        }

        private void recordButton_Click(object sender, EventArgs e)
        {
            try
            {
                theVisMindVision.OnRecord(sender, e);
                methodHandlerComposite.OnRecord("");
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("recordButton_Click", ex.Message);
            }

        }

        private void commTestButton_Click(object sender, EventArgs e)
        {
            this.timer1.Enabled = true;
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

        public void UpdateControls()
        {
            
            if (theVisMindVision.CaptureInProgress)
            {
                captureButton.Text = "停止预览";
            }
            else
            {
                captureButton.Text = "开始预览";
            }

            if (theVisMindVision.VideoInProgress)
            {
                recordButton.Text = "停止录制";
            }
            else
            {
                recordButton.Text = "开始录制";
            }

            textBox1.SelectionStart = TextBox1.TextLength;
            textBox1.ScrollToCaret();

            //ImageBox0.Update();
            //ImageBox1.Update();
            //ImageBox2.Update();
            //ImageBox3.Update();
            Refresh();

        }

        public void AppendToTextBox1(string text)
        {
            textBox1.AppendText(text);
        }
        public void ClearTextBox1()
        {
            textBox1.Clear();
        }

        private void camera1SettingButton_Click(object sender, EventArgs e)
        {
            try
            {
                theVisMindVision.OnCameraSetting(0);
                //methodHandlerComposite.OnRecord("");
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("camera1SettingButton_Click", ex.Message);
            }
        }

        private void camera2SettingButton_Click(object sender, EventArgs e)
        {
            try
            {
                theVisMindVision.OnCameraSetting(1);
                //methodHandlerComposite.OnRecord("");
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("camera2SettingButton_Click", ex.Message);
            }
        }

        private void camera3SettingButton_Click(object sender, EventArgs e)
        {
            try
            {
                theVisMindVision.OnCameraSetting(2);
                //methodHandlerComposite.OnRecord("");
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("camera3SettingButton_Click", ex.Message);
            }
        }

        private void camera4SettingButton_Click(object sender, EventArgs e)
        {
            try
            {
                theVisMindVision.OnCameraSetting(3);
                //methodHandlerComposite.OnRecord("");
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("camera4SettingButton_Click", ex.Message);
            }
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

    public class CapturedPoints
    {
        private List<DateTime> times1 = new List<DateTime>();
        private List<Point> points1 = new List<Point>();
        private List<DateTime> times2 = new List<DateTime>();
        private List<Point> points2 = new List<Point>();
        private List<DateTime> times3 = new List<DateTime>();
        private List<Point> points3 = new List<Point>();
        private List<DateTime> times4 = new List<DateTime>();
        private List<Point> points4 = new List<Point>();

        public List<DateTime> Times1
        {
            get { return times1; }
            set { times1 = value; }
        }

        public List<Point> Points1
        {
            get { return points1; }
            set { points1 = value; }
        }

        public List<DateTime> Times2
        {
            get { return times2; }
            set { times2 = value; }
        }

        public List<Point> Points2
        {
            get { return points2; }
            set { points2 = value; }
        }

        public List<DateTime> Times3
        {
            get { return times3; }
            set { times3 = value; }
        }

        public List<Point> Points3
        {
            get { return points3; }
            set { points3 = value; }
        }

        public List<DateTime> Times4
        {
            get { return times4; }
            set { times4 = value; }
        }

        public List<Point> Points4
        {
            get { return points4; }
            set { points4 = value; }
        }
    }


    // MindVision视觉系统
    public class VisMindVision3
    {
        const int CAMERA_NUM = 4;

        #region MindVision
        // 采集卡
        protected IntPtr[] m_Grabber = new IntPtr[CAMERA_NUM];
        // 图像帧
        protected CameraHandle[] m_hCamera = new CameraHandle[CAMERA_NUM];
        // 相机信息
        protected tSdkCameraDevInfo[] m_DevInfo;
        // 采集卡回调函数
        protected pfnCameraGrabberFrameCallback[] m_FrameCallback = new pfnCameraGrabberFrameCallback[CAMERA_NUM];
        // 采集卡图像保存回调函数
        protected pfnCameraGrabberSaveImageComplete m_SaveImageComplete;
        #endregion


        #region EmguCV视频录制
        // EmguCV帧
        private Mat[] frame = new Mat[CAMERA_NUM];
        // EgmuCV视频写
        private VideoWriter[] vw = new VideoWriter[CAMERA_NUM];  
        // 视频编码
        private char[] codec = { 'D', 'I', 'V', 'X' }; // 视频编码
        // 视频帧率
        private int fps = 25; // 视频帧率
        // 视频录制状态
        //protected Boolean bRecording = false;
        
        // 保存的视频文件名称，主要用以记录
        private string saveAVIFilenames = "";
        // 保存的视频文件个数，主要用以记录
        private int countAVIFiles = 0;

        #endregion

        #region 属性及控制参数
        // 图像采集状态
        private bool captureInProgress = false;
        // 视频录制状态
        private bool videoInProgress = false;
        // 当前的工作相机个数
        private int numberOfAvailableCamera = 0;
        // 当前视觉系统的工作状态
        private VisEmguVisionState visEmguState = new VisEmguVisionState(); 
        // 当前采集的数据点
        CapturedPoints theCapturedPoints = new CapturedPoints();
        #endregion

        // 父窗口
        private CameraCapture3 frm = null;


        public VisMindVision3(CameraCapture3 _frm)
        {
            frm = _frm;
        }

        #region 属性


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

        public CapturedPoints TheCapturedPoints
        {
            get { return theCapturedPoints; }
            set { theCapturedPoints = value; }
        }

        #endregion

        /// <summary>
        /// 开启图像系统
        /// </summary>
        public void Start()
        {
            CvInvoke.UseOpenCL = false;

            try
            {

                // 关联事件响应函数
                m_FrameCallback[0] = new pfnCameraGrabberFrameCallback(frm.CameraGrabberFrameCallback0);
                m_FrameCallback[1] = new pfnCameraGrabberFrameCallback(frm.CameraGrabberFrameCallback1);
                m_FrameCallback[2] = new pfnCameraGrabberFrameCallback(frm.CameraGrabberFrameCallback2);
                m_FrameCallback[3] = new pfnCameraGrabberFrameCallback(frm.CameraGrabberFrameCallback3);

                // 关联图像保存响应函数
                m_SaveImageComplete = new pfnCameraGrabberSaveImageComplete(CameraGrabberSaveImageComplete2);

                // 关联图像显示窗口
                IntPtr[] hDispWnds = { frm.ImageBox0.Handle, frm.ImageBox1.Handle, frm.ImageBox2.Handle, frm.ImageBox3.Handle };

                // 列举出所有设备
                MvApi.CameraEnumerateDevice(out m_DevInfo);
                int NumDev = (m_DevInfo != null ? Math.Min(m_DevInfo.Length, CAMERA_NUM) : 0);
                numberOfAvailableCamera = NumDev;
                
                if (NumDev != CAMERA_NUM)
                {
                    visEmguState.SetState((int)VisEmguVisionState.StateEnum.Abnormal);
                    throw new Exception("相机设备不足.可用的相机数量应为：4,这里的可用相机数量为：" + NumDev.ToString());
                }
                

                for (int i = 0; i < NumDev; ++i)
                {
                    if (MvApi.CameraGrabber_Create(out m_Grabber[i], ref m_DevInfo[i]) == CameraSdkStatus.CAMERA_STATUS_SUCCESS)
                    {
                        // 关联图像帧
                        MvApi.CameraGrabber_GetCameraHandle(m_Grabber[i], out m_hCamera[i]);
                        // 关联属性配置窗口
                        MvApi.CameraCreateSettingPage(m_hCamera[i], frm.Handle, m_DevInfo[i].acFriendlyName, null, (IntPtr)0, 0);
                        // 关联回调函数
                        MvApi.CameraGrabber_SetRGBCallback(m_Grabber[i], m_FrameCallback[i], IntPtr.Zero);
                        // 关联图像保存函数
                        MvApi.CameraGrabber_SetSaveImageCompleteCallback(m_Grabber[i], m_SaveImageComplete, IntPtr.Zero);

                        // 黑白相机设置ISP输出灰度图像
                        // 彩色相机ISP默认会输出BGR24图像
                        tSdkCameraCapbility cap;
                        MvApi.CameraGetCapability(m_hCamera[i], out cap);
                        if (cap.sIspCapacity.bMonoSensor != 0) MvApi.CameraSetIspOutFormat(m_hCamera[i], (uint)MVSDK.emImageFormat.CAMERA_MEDIA_TYPE_MONO8);

                        // 关联图像显示窗口
                        MvApi.CameraGrabber_SetHWnd(m_Grabber[i], hDispWnds[i]);
                    }
                }

                // 开始图像预览
                for (int i = 0; i < NumDev; ++i)
                {
                    if (m_Grabber[i] != IntPtr.Zero)
                        MvApi.CameraGrabber_StartLive(m_Grabber[i]);
                }
                visEmguState.SetState((int)VisEmguVisionState.StateEnum.Capturing);
                captureInProgress = true;

            }
            catch (NullReferenceException excpt)
            {
                throw;
            }
        }

        /// <summary>
        /// 结束图像系统
        /// </summary>
        public void End()
        {
            try
            {
                // 销毁所有图像采集
                for (int i = 0; i < CAMERA_NUM; ++i)
                {
                    if (m_Grabber[i] != IntPtr.Zero)
                        MvApi.CameraGrabber_Destroy(m_Grabber[i]);
                }

                visEmguState.SetState((int)VisEmguVisionState.StateEnum.Finished);
            }
            catch (Exception ex)
            {
                throw;
            }

        }


        #region 图像采集及处理

        /// <summary>
        /// 迈德威视帧转换为Mat格式
        /// </summary>
        /// <param name="pFrameBuffer"></param>
        /// <param name="pFrameHead"></param>
        /// <returns></returns>
        private static Mat MvFrameBufferToCvMat(IntPtr pFrameBuffer, ref tSdkFrameHead pFrameHead)
        {
            Image image = MvApi.CSharpImageFromFrame(pFrameBuffer, ref pFrameHead);
            Bitmap bmpImage = new Bitmap(image);
            Emgu.CV.Image<Bgr, Byte> imageCV = new Emgu.CV.Image<Bgr, Byte>(bmpImage);
            Mat mat = imageCV.Mat;
            return mat;
        }

        private static Image CvMatToImage(Mat frame)
        {
            if (frame == null) return null;
            return frame.Bitmap;
        }


        public void CameraGrabberFrameCallback0(IntPtr Grabber,IntPtr pFrameBuffer,ref tSdkFrameHead pFrameHead,IntPtr Context)
        {
            // 图像处理
            frame[0] = MvFrameBufferToCvMat(pFrameBuffer, ref pFrameHead);
            
            // 调用相关函数进行处理
            CvInvoke.Invert(frame[0], frame[0], DecompMethod.Svd);

            // 获取坐标点
            Point pt = new Point((new Random()).Next(1, 512), (new Random()).Next(1, 512));
            theCapturedPoints.Points1.Add(pt);
            theCapturedPoints.Times1.Add(DateTime.Now);

            // 更新图像
            frm.ImageBox0.Image = (Image)frame[0].Bitmap;



            // 处于视频录制状态
            if (videoInProgress && vw[0]!=null)
            {
                vw[0].Write(frame[0]);
                //MvApi.CameraPushFrame(m_hCamera[0], pFrameBuffer, ref pFrameHead);
            }
        }

        public void CameraGrabberFrameCallback1(IntPtr Grabber,IntPtr pFrameBuffer,ref tSdkFrameHead pFrameHead,IntPtr Context)
        {
            frame[1] = MvFrameBufferToCvMat(pFrameBuffer, ref pFrameHead);


            if (videoInProgress && vw[1] != null)
            {
                vw[1].Write(frame[1]);
            }
        }

        public void CameraGrabberFrameCallback2(IntPtr Grabber,IntPtr pFrameBuffer,ref tSdkFrameHead pFrameHead,IntPtr Context)
        {
            frame[2] = MvFrameBufferToCvMat(pFrameBuffer, ref pFrameHead);

            if (videoInProgress && vw[2] != null)
            {
                //MvApi.CameraPushFrame(m_hCamera[2], pFrameBuffer, ref pFrameHead);
                vw[2].Write(frame[2]);
            }
        }

        public void CameraGrabberFrameCallback3(IntPtr Grabber,IntPtr pFrameBuffer,ref tSdkFrameHead pFrameHead,IntPtr Context)
        {
            frame[3] = MvFrameBufferToCvMat(pFrameBuffer, ref pFrameHead);

            if (videoInProgress && vw[3] != null)
            {
                //MvApi.CameraPushFrame(m_hCamera[3], pFrameBuffer, ref pFrameHead);
                vw[3].Write(frame[3]);
            }
        }


        /// <summary>
        /// 保存图片的回调函数
        /// </summary>
        /// <param name="Grabber">相机采集卡</param>
        /// <param name="Image">图片</param>
        /// <param name="Status">状态</param>
        /// <param name="Context">上下文</param>
        private void CameraGrabberSaveImageComplete2(
                IntPtr Grabber,
                IntPtr Image,	// 需要调用CameraImage_Destroy释放
                CameraSdkStatus Status,
                IntPtr Context)
        {
            if (Image != IntPtr.Zero)
            {
                string filename = GenerateFileName(Grabber,".jpg");

                MvApi.CameraImage_SaveAsJpeg(Image, filename, 90);

                saveAVIFilenames += filename + "\r\n";
                countAVIFiles++;
            }
            if (countAVIFiles == CAMERA_NUM)
            {
                MessageBox.Show(saveAVIFilenames);
            }


            MvApi.CameraImage_Destroy(Image);
        }

        private string GenerateFileName(IntPtr Grabber,string subfix)
        {
            tSdkCameraDevInfo devInfo;
            MvApi.CameraGrabber_GetCameraDevInfo(Grabber, out devInfo);

            Encoding myEncoding = Encoding.GetEncoding("utf-8");
            string sData = myEncoding.GetString(devInfo.acSn);
            sData = sData.TrimEnd('\0');
            sData = sData.Substring(0, 12);

            string filename = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(),
                string.Format("{0}-{1}-{2}"+subfix, System.Environment.TickCount, sData, devInfo.uInstance));
            return filename;
        }

        private void ProcessFrame0(object sender, EventArgs arg)
        {
            if (vw[0] != null && frame[0] != null)
            {
                ProcessVideo(vw[0], frame[0]);
            }
        }

        private void ProcessFrame1(object sender, EventArgs arg)
        {
            if (vw[1] != null && frame[1] != null)
            {
                ProcessVideo(vw[1], frame[1]);
            }
        }

        private void ProcessFrame2(object sender, EventArgs arg)
        {
            if (vw[2] != null && frame[2] != null)
            {
                ProcessVideo(vw[2], frame[2]);
            }
        }

        private void ProcessFrame3(object sender, EventArgs arg)
        {
            if (vw[3] != null && frame[3] != null)
            {
                ProcessVideo(vw[3], frame[3]);
            }
        }

        private void ProcessVideo(VideoWriter vw, Mat frame)
        {
            if (videoInProgress)
            {
                vw.Write(frame);
            }
        }


        #endregion


        /// <summary>
        /// 帧采集按钮事件
        /// </summary>
        public void OnCapture()
        {
            if (m_Grabber[0] != IntPtr.Zero && m_Grabber[1] != IntPtr.Zero && m_Grabber[2] != IntPtr.Zero && m_Grabber[3] != IntPtr.Zero)
            {
                if (captureInProgress)
                {  //stop the capture
                    //captureButton.Text = "开始采集";

                    for (int i = 0; i < CAMERA_NUM; ++i)
                    {
                        if (m_Grabber[i] != IntPtr.Zero)
                            MvApi.CameraGrabber_StopLive(m_Grabber[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < CAMERA_NUM; ++i)
                    {
                        if (m_Grabber[i] != IntPtr.Zero)
                            MvApi.CameraGrabber_StartLive(m_Grabber[i]);
                    }
                }

                captureInProgress = !captureInProgress;
                visEmguState.SetState((int)VisEmguVisionState.StateEnum.Capturing);
            }
        }

        // 一键采集
        public void OnSnap(int cameraIdx)
        {
            if (cameraIdx == -1)
            {
                for (int i = 0; i < CAMERA_NUM; i++)
                {
                    if (m_Grabber[i] != IntPtr.Zero)
                        MvApi.CameraGrabber_SaveImageAsync(m_Grabber[i]);
                }
            }
            else
            {
                if (m_Grabber[cameraIdx] != IntPtr.Zero)
                    MvApi.CameraGrabber_SaveImageAsync(m_Grabber[cameraIdx]);
            }
        }


        // 相机设置
        public void OnCameraSetting(int cameraIdx)
        {
            if (cameraIdx <0 || cameraIdx > 3 )
            {
                return;
            }

            if (m_Grabber[cameraIdx] != IntPtr.Zero)
                MvApi.CameraShowSettingPage(m_hCamera[cameraIdx], 1);

        }


        // 一键录制
        public void OnRecord(object sender, EventArgs e)
        {
            string dt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            if (!videoInProgress)
            {

                tSdkImageResolution resolution = new tSdkImageResolution();
                string filename = GenerateFileName(m_Grabber[0], ".avi");
                MvApi.CameraGetImageResolution(m_hCamera[0], out resolution);
                vw[0] = new VideoWriter(filename, VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
                                        fps,
                                        new Size(resolution.iWidth, resolution.iHeight),
                                        true);

                MvApi.CameraGetImageResolution(m_hCamera[1], out resolution);
                filename = GenerateFileName(m_Grabber[1], ".avi");
                vw[1] = new VideoWriter(filename, VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
                                        fps,
                                        new Size(resolution.iWidth, resolution.iHeight),
                                        true);

                MvApi.CameraGetImageResolution(m_hCamera[2], out resolution);
                filename = GenerateFileName(m_Grabber[2], ".avi");
                vw[2] = new VideoWriter(filename, VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
                                        fps,
                                        new Size(resolution.iWidth, resolution.iHeight),
                                        true);

                MvApi.CameraGetImageResolution(m_hCamera[3], out resolution);
                filename = GenerateFileName(m_Grabber[3], ".avi");
                vw[3] = new VideoWriter(filename, VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
                                        fps,
                                        new Size(resolution.iWidth, resolution.iHeight),
                                        true);


                Application.Idle += new EventHandler(ProcessFrame0);
                Application.Idle += new EventHandler(ProcessFrame1);
                Application.Idle += new EventHandler(ProcessFrame2);
                Application.Idle += new EventHandler(ProcessFrame3);

                visEmguState.SetState((int)VisEmguVisionState.StateEnum.Recording);
            }
            else
            {
                vw[0].Dispose();
                vw[1].Dispose();
                vw[2].Dispose();
                vw[3].Dispose();

                vw[0] = null;
                vw[1] = null;
                vw[2] = null;
                vw[3] = null;

                frame[0] = null;
                frame[1] = null;
                frame[2] = null;
                frame[3] = null;
                Application.Idle -= new EventHandler(ProcessFrame0);
                Application.Idle -= new EventHandler(ProcessFrame1);
                Application.Idle -= new EventHandler(ProcessFrame2);
                Application.Idle -= new EventHandler(ProcessFrame3);

                visEmguState.SetState((int)VisEmguVisionState.StateEnum.Capturing);
            }

            videoInProgress = !videoInProgress;
        }

    }



    #region 服务器功能接口
    // 主要用以响应CameraCapture3中的各个函数
    public interface ICameraCapture3MethodHandler
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
        // 保存第0帧
        void OnSaveFrame0(string msg);
        // 保存第1帧
        void OnSaveFrame1(string msg);
        // 保存第2帧
        void OnSaveFrame2(string msg);
        // 保存第3帧
        void OnSaveFrame3(string msg);
        // 相机采集
        void OnCapture(string msg);
        // 一键截图
        void OnSnap(string msg);
        // 一键录制
        void OnRecord(string msg);
        // 设置相机参数
        void OnCameraSetting(string msg);

    }

    public class CameraCapture3MethodHandler : ICameraCapture3MethodHandler
    {

        #region 实用文本格式

        protected string startClientFmt = "%%%%客户端已启动!%%%%";
        protected string finishClientFmt = "%%%%客户端已停止!%%%%";
        protected string pauseClientFmt = "%%%%客户端已暂停!%%%%";
        protected string resumeClientFmt = "%%%%客户端已恢复!%%%%";
        protected string broadCastMessageFmt = "####服务端广播的信息####\r\n{0}";
        protected string visUpCastMessageFmt = "----视觉端发送信息----\r\n{0}";
        protected string processFrameMessageFmt = "****相机{0}已处理\r\n{1}****";
        protected string saveFrameMessageFmt = "****相机{0}已保存\r\n{1}****";
        protected string captureMessageFmt = "$$$$预览状态$$$$\r\n{0}";
        protected string recordMessageFmt = "$$$$视频录制$$$$\r\n{0}";
        protected string snapMessageFmt = "$$$$一键截图$$$$\r\n{0}";
        protected string cameraSettingMessageFmt = "$$$$相机状态更改$$$$\r\n{0}";
        protected string exceptionMessageFmt = "！！！！异常信息！！！！\r\n异常位置：{0}\r\n异常信息：{1}";

        

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

       
        protected virtual string ExceptionMessageFmt
        {
            get { return exceptionMessageFmt; }
            set { exceptionMessageFmt = value; }
        }

        protected string StartClientFmt
        {
            get { return startClientFmt; }
            set { startClientFmt = value; }
        }

        protected string FinishClientFmt
        {
            get { return finishClientFmt; }
            set { finishClientFmt = value; }
        }

        protected string PauseClientFmt
        {
            get { return pauseClientFmt; }
            set { pauseClientFmt = value; }
        }

        protected string ResumeClientFmt
        {
            get { return resumeClientFmt; }
            set { resumeClientFmt = value; }
        }

        protected string ProcessFrameMessageFmt
        {
            get { return processFrameMessageFmt; }
            set { processFrameMessageFmt = value; }
        }

        protected string SaveFrameMessageFmt
        {
            get { return saveFrameMessageFmt; }
            set { saveFrameMessageFmt = value; }
        }

        protected string CaptureMessageFmt
        {
            get { return captureMessageFmt; }
            set { captureMessageFmt = value; }
        }

        protected string RecordMessageFmt
        {
            get { return recordMessageFmt; }
            set { recordMessageFmt = value; }
        }

        protected string SnapMessageFmt
        {
            get { return snapMessageFmt; }
            set { snapMessageFmt = value; }
        }

        protected string CameraSettingMessageFmt
        {
            get { return cameraSettingMessageFmt; }
            set { cameraSettingMessageFmt = value; }
        }

        #endregion

        public CameraCapture3MethodHandler() { }

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
        public virtual void OnProcessFrame3(string msg){}
        public virtual void OnSaveFrame0(string msg) { }
        public virtual void OnSaveFrame1(string msg) { }
        public virtual void OnSaveFrame2(string msg) { }
        public virtual void OnSaveFrame3(string msg) { }
        public virtual void OnCapture(string msg) { }
        public virtual void OnSnap(string msg) { }
        public virtual void OnRecord(string msg) { }
        public virtual void OnCameraSetting(string msg){}
    }

    public class CameraCapture3MethodHandlerComposite : CameraCapture3MethodHandler
    {
        private List<ICameraCapture3MethodHandler> methodHandlers = new List<ICameraCapture3MethodHandler>();

        public CameraCapture3MethodHandlerComposite() { }

        #region 基本列表操作
        public void Add(ICameraCapture3MethodHandler handler) { methodHandlers.Add(handler); }
        public void Remove(ICameraCapture3MethodHandler handler) { methodHandlers.Remove(handler); }
        public void Clear() { methodHandlers.Clear(); }
        #endregion

        public override void OnStartClient()
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnStartClient();
            }
        }

        public override void OnPauseClient()
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnPauseClient();
            }
        }

        public override void OnResumeClient()
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnResumeClient();
            }
        }

        public override void OnFinishClient()
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnFinishClient();
            }
        }

        public override void OnBroadCastMessage(string msg)
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnBroadCastMessage(msg);
            }
        }

        public override void OnUpCastEvent(string msg)
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnUpCastEvent(msg);
            }
        }

        public override void OnClearText()
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnClearText();
            }
        }

        public override void OnException(string point, string msg)
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnException(point, msg);
            }
        }

        public override void OnProcessFrame0(string msg)
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnProcessFrame0(msg);
            }
        }

        public override void OnProcessFrame1(string msg)
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnProcessFrame1(msg);
            }
        }

        public override void OnProcessFrame2(string msg)
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnProcessFrame2(msg);
            }
        }

        public override void OnProcessFrame3(string msg)
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnProcessFrame3(msg);
            }
        }

        public override void OnSaveFrame0(string msg)
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnSaveFrame0(msg);
            }
        }

        public override void OnSaveFrame1(string msg)
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnSaveFrame1(msg);
            }
        }

        public override void OnSaveFrame2(string msg)
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnSaveFrame2(msg);
            }
        }

        public override void OnSaveFrame3(string msg)
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnSaveFrame3(msg);
            }
        }


        public override void OnCapture(string msg)
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnCapture(msg);
            }
        }

        public override void OnSnap(string msg)
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnSnap(msg);
            }
        }

        public override void OnRecord(string msg)
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnRecord(msg);
            }
        }

        public override void OnCameraSetting(string msg)
        {
            foreach (ICameraCapture3MethodHandler handler in methodHandlers)
            {
                handler.OnCameraSetting(msg);
            }
        }

    }

    #endregion


    [assembly: log4net.Config.XmlConfigurator(Watch = true)]
    public class CameraCapture3Log : CameraCapture3MethodHandler
    {
        private ILog log = null;
        private string logInfo = "";

        public string LogInfo
        {
            get { return logInfo; }
            set { logInfo = value; }
        }

        public CameraCapture3Log()
        {
            log = log4net.LogManager.GetLogger("visClient.Logging");
            Debug.Assert(log != null);
        }

        public CameraCapture3Log(string logname)
        {
            log = log4net.LogManager.GetLogger(logname);
            Debug.Assert(log != null);
        }

        public override void OnStartClient(){log.Info(StartClientFmt);}
        public override void OnPauseClient() { log.Info(PauseClientFmt); }
        public override void OnResumeClient() { log.Info(ResumeClientFmt); }
        public override void OnFinishClient() { log.Info(FinishClientFmt); }
        public override void OnBroadCastMessage(string msg) { log.Info(String.Format(BroadCastMessageFmt, msg)); }
        public override void OnUpCastEvent(string msg) { log.Info(String.Format(VisUpCastMessageFmt, msg)); }
        public override void OnClearText() {  }
        public override void OnProcessFrame0(string msg) { log.Info(String.Format(ProcessFrameMessageFmt, 0,msg));}
        public override void OnProcessFrame1(string msg) {log.Info(String.Format(ProcessFrameMessageFmt, 1,msg)); }
        public override void OnProcessFrame2(string msg) {log.Info(String.Format(ProcessFrameMessageFmt, 2,msg)); }
        public override void OnProcessFrame3(string msg) { log.Info(String.Format(ProcessFrameMessageFmt, 3,msg));}

        public override void OnSaveFrame0(string msg) { log.Info(String.Format(SaveFrameMessageFmt, 0, msg)); }
        public override void OnSaveFrame1(string msg) { log.Info(String.Format(SaveFrameMessageFmt, 1, msg)); }
        public override void OnSaveFrame2(string msg) { log.Info(String.Format(SaveFrameMessageFmt, 2, msg)); }
        public override void OnSaveFrame3(string msg) { log.Info(String.Format(SaveFrameMessageFmt, 3, msg)); }

        public override void OnCapture(string msg) { log.Info(String.Format(CaptureMessageFmt, msg)); }
        public override void OnSnap(string msg) { log.Info(String.Format(SnapMessageFmt, msg)); }
        public override void OnRecord(string msg) { log.Info(String.Format(RecordMessageFmt, msg)); }
        public override void OnException(string addr, string msg){log.Info(String.Format(ExceptionMessageFmt, addr, msg));}
    }



    public class CameraCapture3UI : CameraCapture3MethodHandler
    {
        private CameraCapture3 frm;

        public CameraCapture3UI(CameraCapture3 _frm)
        {
            frm = _frm;
        }


        //private void UpdateControls()
        //{
        //    VisMindVision3 vis = frm.TheVisMindVision;

        //    if (vis.CaptureInProgress)
        //    {
        //        frm.CaptureButton.Text = "停止预览";
        //    }
        //    else
        //    {
        //        frm.TextBox1.Text = "开始预览";
        //    }

        //    if (vis.VideoInProgress)
        //    {
        //        frm.RecordButton.Text = "停止录制";
        //    }
        //    else
        //    {
        //        frm.TextBox1.Text = "开始录制";
        //    }

        //    frm.TextBox1.SelectionStart = frm.TextBox1.TextLength;
        //    frm.TextBox1.ScrollToCaret();
        //}

        public override void OnStartClient()
        {
            frm.AppendToTextBox1(StartClientFmt);
            frm.AppendToTextBox1("\r\n");
            frm.UpdateControls();
        }

        public override void OnPauseClient()
        {
            frm.AppendToTextBox1(PauseClientFmt);
            frm.AppendToTextBox1("\r\n");
            frm.UpdateControls();
        }

        public override void OnResumeClient()
        {
            frm.AppendToTextBox1(ResumeClientFmt);
            frm.AppendToTextBox1("\r\n");
            frm.UpdateControls();
        }

        public override void OnFinishClient()
        {
            frm.AppendToTextBox1(FinishClientFmt);
            frm.AppendToTextBox1("\r\n");
            frm.UpdateControls();
        }

        public override void OnBroadCastMessage(string msg)
        {
            frm.AppendToTextBox1(String.Format(BroadCastMessageFmt, msg));
            frm.AppendToTextBox1("\r\n");
            frm.UpdateControls();
        }

        public override void OnUpCastEvent(string msg)
        {
            frm.AppendToTextBox1(String.Format(VisUpCastMessageFmt, msg));
            frm.AppendToTextBox1("\r\n");
            frm.UpdateControls();
        }

        public override void OnClearText()
        {
            frm.ClearTextBox1();
            frm.UpdateControls();
        }

        public override void OnException(string point, string msg)
        {
            frm.AppendToTextBox1(String.Format(ExceptionMessageFmt, point, msg));
            frm.UpdateControls();
        }

        public override void OnProcessFrame0(string msg)
        {
            //frm.AppendToTextBox1(String.Format(ProcessFrameMessageFmt, 0, msg));
            //frm.UpdateControls();
        }

        public override void OnProcessFrame1(string msg)
        {
            //frm.AppendToTextBox1(String.Format(ProcessFrameMessageFmt, 1, msg));
            //frm.UpdateControls();            
        }

        public override void OnProcessFrame2(string msg)
        {
            //frm.AppendToTextBox1(String.Format(ProcessFrameMessageFmt, 2, msg));
            //frm.UpdateControls();            
        }

        public override void OnProcessFrame3(string msg)
        {
            //frm.AppendToTextBox1(String.Format(ProcessFrameMessageFmt, 3, msg));
            //frm.UpdateControls();            
        }

        public override void OnCapture(string msg)
        {
            frm.AppendToTextBox1(String.Format(CaptureMessageFmt, 0, msg));
            frm.UpdateControls();
        }

        public override void OnSnap(string msg)
        {
            frm.AppendToTextBox1(String.Format(SnapMessageFmt, 0, msg));
            frm.UpdateControls();
        }

        public override void OnRecord(string msg)
        {
            frm.AppendToTextBox1(String.Format(RecordMessageFmt, 0, msg));
            frm.UpdateControls();
        }
    }



    public class CameraCapture3Monitor:CameraCapture3MethodHandler
    {
        protected string clientState = "未启动";
        protected int numOfBroadCastEventObserver = 0;
        protected int numOfVisUpCastEventObserver = 0;
        protected int numberOfServerSend = 0;
        protected int numberOfServerRcv = 0;
        protected int numberOfVisSend = 0;
        protected int numberOfVisRcv = 0;
        protected int numberOfAvailableCaptures = 0;
        protected int numberOfException = 0;

        // 显示采集的帧数
        protected uint numberImagesCapturedByFrame0 = 0;
        protected uint numberImagesCapturedByFrame1 = 0;
        protected uint numberImagesCapturedByFrame2 = 0;
        protected uint numberImagesCapturedByFrame3 = 0;

        // 显示截图的帧数
        protected uint numberImagesSnapedByFrame0 = 0;
        protected uint numberImagesSnapedByFrame1 = 0;
        protected uint numberImagesSnapedByFrame2 = 0;
        protected uint numberImagesSnapedByFrame3 = 0;


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

        private CameraCapture3 frm;

        public CameraCapture3Monitor(CameraCapture3 _frm)
        {
            frm = _frm;
        }




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
        public int NumberOfAvailableCaptures
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

        public int NumberOfException
        {
            get { return numberOfException; }
            set { numberOfException = value; }
        }

        public uint NumberImagesSnapedByFrame0
        {
            get { return numberImagesSnapedByFrame0; }
            set { numberImagesSnapedByFrame0 = value; }
        }

        public uint NumberImagesSnapedByFrame1
        {
            get { return numberImagesSnapedByFrame1; }
            set { numberImagesSnapedByFrame1 = value; }
        }

        public uint NumberImagesSnapedByFrame2
        {
            get { return numberImagesSnapedByFrame2; }
            set { numberImagesSnapedByFrame2 = value; }
        }

        public uint NumberImagesSnapedByFrame3
        {
            get { return numberImagesSnapedByFrame3; }
            set { numberImagesSnapedByFrame3 = value; }
        }

        #region 响应视觉端业务代码

        public override void OnStartClient()
        {
            this.clientState = frm.TheVisMindVision.VisEmguState.ToString();
        }

        public override void OnPauseClient()
        {
            this.clientState = frm.TheVisMindVision.VisEmguState.ToString();
        }

        public override void OnResumeClient()
        {
            this.clientState = frm.TheVisMindVision.VisEmguState.ToString();
        }

        public override void OnFinishClient()
        {
            this.clientState = frm.TheVisMindVision.VisEmguState.ToString();
        }

        public override void OnBroadCastMessage(string msg)
        {
            this.numberOfServerSend++;
        }

        public override void OnUpCastEvent(string msg)
        {
            this.numberOfVisSend++;
        }
        public override void OnClearText() { }

        public override void OnException(string point, string msg)
        {
            this.numberOfException++;
        }

        public override void OnProcessFrame0(string msg)
        {
            this.numberImagesCapturedByFrame0++;
        }

        public override void OnProcessFrame1(string msg)
        {
            this.numberImagesCapturedByFrame1++;
        }

        public override void OnProcessFrame2(string msg)
        {
            this.numberImagesCapturedByFrame2++;
        }

        public override void OnProcessFrame3(string msg)
        {
            this.numberImagesCapturedByFrame3++;
        }

        public override void OnCapture(string msg)
        {
            this.clientState = frm.TheVisMindVision.VisEmguState.ToString();
        }

        public override void OnSnap(string msg)
        {
            //this.clientState = frm.TheVisMindVision.VisEmguState.ToString();
            this.numberImagesSnapedByFrame0++;
            this.numberImagesSnapedByFrame1++;
            this.numberImagesSnapedByFrame2++;
            this.numberImagesSnapedByFrame3++;

        }

        public override void OnRecord(string msg)
        {
            this.clientState = frm.TheVisMindVision.VisEmguState.ToString();
        } 
        #endregion
    }

    public class VisEmguVision3
    {
        // 视频采集
        private VideoCapture capture0 = null;
        private VideoCapture capture1 = null;
        private VideoCapture capture2 = null;
        private VideoCapture capture3 = null;
        // 采集的图片
        private Mat frame0 = null;
        private Mat frame1 = null;
        private VideoWriter vw0 = null;
        private VideoWriter vw1 = null;
        private Mat frame2 = null;
        private Mat frame3 = null;
        // 视频写
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
        private CameraCapture3 frm = null;


        public VisEmguVision3(CameraCapture3 _frm)
        {
            frm = _frm;
            //theVisMindVision = new VisMindVision3(frm.Handle);
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
                throw;
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

                frm.ImageBox0.Image = frame0.Bitmap;
                ProcessVideo(vw0, frame0);
            }
        }

        private void ProcessFrame1(object sender, EventArgs arg)
        {
            if (capture1 != null && capture1.Ptr != IntPtr.Zero)
            {
                capture1.Retrieve(frame1, 0);
                frm.ImageBox1.Image = frame1.Bitmap;
                ProcessVideo(vw1, frame1);
            }
        }

        private void ProcessFrame2(object sender, EventArgs arg)
        {
            if (capture2 != null && capture2.Ptr != IntPtr.Zero)
            {
                capture2.Retrieve(frame2, 0);
                frm.ImageBox2.Image = frame2.Bitmap;
                ProcessVideo(vw2, frame2);
            }
        }

        private void ProcessFrame3(object sender, EventArgs arg)
        {
            if (capture3 != null && capture3.Ptr != IntPtr.Zero)
            {
                capture3.Retrieve(frame3, 0);
                frm.ImageBox3.Image = frame3.Bitmap;
                ProcessVideo(vw3, frame3);
            }
        }

        private void ProcessVideo(VideoWriter vw, Mat frame)
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
            //        VisEmguVision3.videoInProgress = true;

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
            //        VisEmguVision3.videoInProgress = false;


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

}