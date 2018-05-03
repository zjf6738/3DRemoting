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
            log.Info(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "frame"+i.ToString() +" captured");
        }

        public void VideoWriting(int i)
        {
            log.Info(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "video" + i.ToString() + " writing...");
        }

        public void DisplayBroadCastInfo(string msg)
        {
            log.Info("BroadCastingMessage--" + msg);
        }

    }
}
