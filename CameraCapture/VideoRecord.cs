//----------------------------------------------------------------------------
//  Copyright (C) 2004-2017 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

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

namespace CameraCapture
{
    [assembly: log4net.Config.XmlConfigurator(Watch = true)]
   public partial class VideoRecord : Form
   {
      private VideoCapture _capture0 = null;
      private VideoCapture _capture1 = null;

      private bool _captureInProgress;
      private Mat _frame0;
      private Mat _frame1;
      private Mat _grayFrame;
      private Mat _smallGrayFrame;
      private Mat _smoothedGrayFrame;
      private Mat _cannyFrame;

      private VideoWriter vw;
      private bool flag = false;

      public VideoRecord()
      {
         InitializeComponent();
         CvInvoke.UseOpenCL = false;
         try
         {
            _capture0 = new VideoCapture(0);
            _capture0.ImageGrabbed += ProcessFrame;

            _capture1 = new VideoCapture(1);
            _capture1.ImageGrabbed += ProcessFrame1;


         }
         catch (NullReferenceException excpt)
         {
            MessageBox.Show(excpt.Message);
         }

         _frame0 = new Mat();
         _frame1 = new Mat();
         _grayFrame = new Mat();
         _smallGrayFrame = new Mat();
         _smoothedGrayFrame = new Mat();
         _cannyFrame = new Mat();
      }

      private void ProcessFrame(object sender, EventArgs arg)
      {
         if (_capture0 != null && _capture0.Ptr != IntPtr.Zero)
         {
            _capture0.Retrieve(_frame0, 0);

            CvInvoke.CvtColor(_frame0, _grayFrame, ColorConversion.Bgr2Gray);

            CvInvoke.PyrDown(_grayFrame, _smallGrayFrame);

            CvInvoke.PyrUp(_smallGrayFrame, _smoothedGrayFrame);

            CvInvoke.Canny(_smoothedGrayFrame, _cannyFrame, 100, 60);

            captureImageBox.Image = _frame0;
            //grayscaleImageBox.Image = _grayFrame;
            smoothedGrayscaleImageBox.Image = _smoothedGrayFrame;
            cannyImageBox.Image = _cannyFrame;

            ILog log = log4net.LogManager.GetLogger("visClient.Logging");
            log.Info(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "frame0 captured");

            if (flag)
            {
                vw.Write(_frame0);
                log.Info(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "video writing...");
            }

         }
      }

      private void ProcessFrame1(object sender, EventArgs arg)
      {
          if (_capture1 != null && _capture1.Ptr != IntPtr.Zero)
          {
              _capture1.Retrieve(_frame1, 0);

              grayscaleImageBox.Image = _frame1;

              ILog log = log4net.LogManager.GetLogger("visClient.Logging");
              log.Info(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+"frame1 captured");

          }
      }

      private void captureButtonClick(object sender, EventArgs e)
      {
          if (_capture0 != null && _capture1 != null)
         {
            if (_captureInProgress)
            {  //stop the capture
               captureButton.Text = "Start Capture";
               _capture0.Pause();
               _capture1.Pause();
               
               
            }
            else
            {
               //start the capture
               captureButton.Text = "Stop";
               _capture0.Start();
               _capture1.Start();
            }

            _captureInProgress = !_captureInProgress;
         }
      }

      private void ReleaseData()
      {
         if (_capture0 != null)
            _capture0.Dispose();

         if (_capture1 != null)
             _capture1.Dispose();
      }

      private void FlipHorizontalButtonClick(object sender, EventArgs e)
      {
         if (_capture0 != null) _capture0.FlipHorizontal = !_capture0.FlipHorizontal;
      }

      private void FlipVerticalButtonClick(object sender, EventArgs e)
      {
         if (_capture0 != null) _capture0.FlipVertical = !_capture0.FlipVertical;
      }

      private void Recordbutton_Click(object sender, EventArgs e)
      {


          if (Recordbutton.Text == "录制")
             {
                 if (MessageBox.Show("开始录制吗？", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                 {
                     flag = true;
                     //vw = new VideoWriter("E:\\1.avi", -1, 25,(int)CvInvoke.cvGetCaptureProperty(capture, Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_WIDTH), (int)CvInvoke.cvGetCaptureProperty(capture, Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT), true);

                     // char[] codec = { 'M', 'J', 'P', 'G' };
                     char [] codec = { 'D', 'I', 'V', 'X' };

                     vw = new VideoWriter("2.avi", VideoWriter.Fourcc(codec[0], codec[1], codec[2], codec[3]),
                         25,
                         new Size(_capture0.Width, _capture0.Height), 
                        true);
                     Application.Idle += new EventHandler(ProcessFrame);
                     Recordbutton.Text = "暂停";
                 }

                
             }
             else 
             {
                 if (MessageBox.Show("停止录制吗？", "Information", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) == DialogResult.OK)
                 {
                     flag = false;
                     vw.Dispose();
                     Application.Idle -= new EventHandler(ProcessFrame);
                     Recordbutton.Text = "录制";
                 }
             }
      }

      #region 通信部分

        private IBroadCast watch = null;
        private EventWrapper wrapper = null;
        private IUpCast upCast = null;
        private string rcvMsg = "";

      // 开启客户端
      private void StartClient()
      {
          BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
          BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
          serverProvider.TypeFilterLevel = TypeFilterLevel.Full;

          IDictionary props = new Hashtable();
          props["port"] = 0;
          TcpChannel channel = new TcpChannel(props, clientProvider, serverProvider);
          ChannelServices.RegisterChannel(channel);

          // 由config中读取相关数据
          string broadCastObjURL = ConfigurationManager.AppSettings["BroadCastObjURL"];

          // 获取广播远程对象
          watch = (IBroadCast)Activator.GetObject(typeof(IBroadCast), broadCastObjURL);
          wrapper = new EventWrapper();
          wrapper.LocalBroadCastEvent += new BroadCastEventHandler(BroadCastingMessage);
          watch.BroadCastEvent += new BroadCastEventHandler(wrapper.BroadCasting);

          // upcast
          upCast = (IUpCast)Activator.GetObject(typeof(IUpCast), "tcp://localhost:8086/VisUpCastObj.soap");
      }

      // 结束客户端
      private void FinishClient()
      {
          watch.BroadCastEvent -= new BroadCastEventHandler(wrapper.BroadCasting);
      }

      // 响应广播信息
      public void BroadCastingMessage(string message)
      {
          CommObj commObj = CommObj.FromJson(message);

          if (commObj == null)
          {
              rcvMsg = "Json解析错误";
          }
          else
          {
              commObj.RcvTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
              rcvMsg = commObj.ToString();
          }

          new Thread(Check).Start();

          log.Info("BroadCastingMessage--" + rcvMsg);
      }

      public void Check()
      {
          lock (this)
              Invoke(new MethodInvoker(delegate()
              {
                  //txtMessage.Text += "I got it:" + rcvMsg;
                  //txtMessage.Text += System.Environment.NewLine;
              }));
      }

      // 发送消息到服务端
      private void btn_Send_Click(object sender, EventArgs e)
      {
          //SendToServerForm ssForm = new SendToServerForm();
          //ssForm.StartPosition = FormStartPosition.CenterParent;
          //ssForm.ShowDialog();



      }
      #endregion

      #region 日志模块
      ILog log = null;

      private void InitLog4net()
      {
          log = log4net.LogManager.GetLogger("visClient.Logging");
          Debug.Assert(log != null);
      }

      #endregion

      private void VideoRecord_Load(object sender, EventArgs e)
      {
          StartClient();
          InitLog4net();
      }

      private void VideoRecord_FormClosing(object sender, FormClosingEventArgs e)
      {
          FinishClient();
      }

      private void SendStatusMsgbutton_Click(object sender, EventArgs e)
      {

          CommObj commObj = new CommObj();
          commObj.SrcId = 0x00000002;
          commObj.DestId = 0x00000000;
          commObj.SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
          commObj.DataType = "String";
          commObj.DataBody = "vision system";

          string json = CommObj.ToJson(commObj);

          upCast.SendMsg(json);

          log.Info("visUpcast.SendMsg--" + json);

      }


   }
}
