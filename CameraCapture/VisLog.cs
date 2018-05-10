using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;

namespace CameraCapture
{
    [assembly: log4net.Config.XmlConfigurator(Watch = true)]
    public class VisLog:CameraCapture2MethodHandler
    {
        private ILog log = null;
        private string logInfo = "";

        public string LogInfo
        {
            get { return logInfo; }
            set { logInfo = value; }
        }

        public VisLog()
        {
            log = log4net.LogManager.GetLogger("visClient.Logging");
            Debug.Assert(log != null);
        }

        public VisLog(string logname)
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


        #region 业务代码响应
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
