using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using log4net;

namespace CameraCapture
{
    [assembly: log4net.Config.XmlConfigurator(Watch = true)]
    public class VisLog
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

    }
}
