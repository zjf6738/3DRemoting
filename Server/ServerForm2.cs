using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using Qzeim.ThrdPrint.BroadCast.RemoteObject;
using Qzeim.ThrdPrint.BroadCast.Common;
using log4net;

namespace Qzeim.ThrdPrint.BroadCast.Server
{
    public partial class ServerForm2 : Form, IUpCastHandler
    {
        // 通信对象
        private ServerComm serverComm = null;
        // 监控对象
        private ServerMonitor serverMonitor = null;
        // 日志对象
        private ServerLog serverLog = null;
        // UI更新对象
        private ServerUI serverUI = null;

        // 方法响应函数，类似于装饰者模式
        ServerForm2MethodHandlerComposite methodHandlerComposite = null;

        public ServerForm2()
        {
            InitializeComponent();

            TheServerComm = new ServerComm(this);
            serverLog = new ServerLog();
            serverMonitor = new ServerMonitor(this);
            serverUI = new ServerUI(this);

            methodHandlerComposite = new ServerForm2MethodHandlerComposite();
            methodHandlerComposite.Add(serverLog);
            methodHandlerComposite.Add(serverMonitor);
            methodHandlerComposite.Add(serverUI);

            propertyGrid1.SelectedObject = serverMonitor;
        }

        public ServerComm TheServerComm
        {
            get { return serverComm; }
            set { serverComm = value; }
        }

        private void ServerForm2_Load(object sender, EventArgs e)
        {
            try
            {
                serverComm.StartServer();
                methodHandlerComposite.OnStartServer();
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("ServerForm2_Load",ex.Message);
            }

        }

        private void ServerForm2_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                serverComm.FinishServer();
                methodHandlerComposite.OnFinishServer();
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("ServerForm2_FormClosing", ex.Message);
            }

        }

        #region 通信部分

        // 响应客户端的事件
        private void DefaultUpCastEventHandler(string msg)
        {

            // 接收并提取信息
            CommObj commObj = CommObj.FromJson(msg);

            if (commObj == null)
            {
                serverComm.RcvMsg = "Json解析错误";
            }
            else
            {
                commObj.RcvTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                serverComm.RcvMsg = commObj.ToString();

                BroadCastMessage(msg);
            }

            // UI线程安全,监控，日志
            new Thread(Check).Start();
        }

        public void Check()
        {
            lock (this)
                Invoke(new MethodInvoker(delegate()
                {
                    methodHandlerComposite.OnBroadCastMessage(serverComm.RcvMsg);
                }));
        }


        public void OnUpCastEvent(string msg)
        {
            DefaultUpCastEventHandler(msg);

            
        }

        public void OnVisUpCastEvent(string msg)
        {
            try
            {
                methodHandlerComposite.OnVisUpCastEvent(msg);
                DefaultUpCastEventHandler(msg);
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("OnVisUpCastEvent", ex.Message);
            }

        }

        public void OnRobotUpCastEvent(string msg)
        {
            try
            {
                methodHandlerComposite.OnRobotUpCastEvent(msg);
                DefaultUpCastEventHandler(msg);
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("OnRobotUpCastEvent", ex.Message);
            }


        }

        public void OnMoverUpCastEvent(string msg)
        {
            try
            {
                methodHandlerComposite.OnMoverUpCastEvent(msg);
                DefaultUpCastEventHandler(msg);
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("OnMoverUpCastEvent", ex.Message);
            }
        }

        public void BroadCastMessage(string json)
        {
            ServerComm.Obj.BroadCastingInfo(json);
        }

        #endregion

        // 广播事件
        private void 广播ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            string text = "这是广播的测试信息";

            try
            {
                CommObj commObj = new CommObj();
                commObj.SrcId = 0xff;
                commObj.DestId = 0x0;
                commObj.SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                commObj.DataType = "String";
                commObj.DataBody = text;

                string json = CommObj.ToJson(commObj);

                BroadCastMessage(json);

                methodHandlerComposite.OnBroadCastMessage(json);
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("广播ToolStripMenuItem_Click", ex.Message);
            }
        }



        private void 暂停ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                serverComm.PauseServer();
                methodHandlerComposite.OnPauseServer();
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("暂停ToolStripMenuItem_Click", ex.Message);
            }


        }

        private void 恢复ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                serverComm.ResumeServer();
                methodHandlerComposite.OnResumeServer();
            }
            catch (Exception ex)
            {
                methodHandlerComposite.OnException("恢复ToolStripMenuItem_Click", ex.Message);
            }


        }

        private void 清除收发信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            methodHandlerComposite.OnClearText();
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }

    #region 服务器功能接口
    // 主要用以响应ServerForm2中的各个函数
    public interface IServerForm2MethodHandler
    {
        void OnStartServer();
        void OnPauseServer();
        void OnResumeServer();
        void OnFinishServer();
        void OnVisUpCastEvent(string msg);
        void OnRobotUpCastEvent(string msg);
        void OnMoverUpCastEvent(string msg);
        void OnBroadCastMessage(string msg);
        void OnClearText();
        void OnException(string point, string msg);
    }

    public class ServerForm2MethodHandler : IServerForm2MethodHandler
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

        public ServerForm2MethodHandler() { }

        public virtual void OnStartServer() { }
        public virtual void OnPauseServer() { }
        public virtual void OnResumeServer() { }
        public virtual void OnFinishServer() { }
        public virtual void OnBroadCastMessage(string msg) { }
        public virtual void OnVisUpCastEvent(string msg) { }
        public virtual void OnRobotUpCastEvent(string msg) { }
        public virtual void OnMoverUpCastEvent(string msg) { }
        public virtual void OnClearText() { }
        public virtual void OnException(string point, string msg) { }
    }

    public class ServerForm2MethodHandlerComposite : ServerForm2MethodHandler
    {
        private List<IServerForm2MethodHandler> methodHandlers = new List<IServerForm2MethodHandler>();

        public ServerForm2MethodHandlerComposite() { }

        #region 基本列表操作
        public void Add(IServerForm2MethodHandler handler) { methodHandlers.Add(handler); }
        public void Remove(IServerForm2MethodHandler handler) { methodHandlers.Remove(handler); }
        public void Clear() { methodHandlers.Clear(); } 
        #endregion

        public override void OnStartServer()
        {
            foreach (IServerForm2MethodHandler handler in methodHandlers)
            {
                handler.OnStartServer();
            }
        }

        public override void OnPauseServer()
        {
            foreach (IServerForm2MethodHandler handler in methodHandlers)
            {
                handler.OnPauseServer();
            }
        }

        public override void OnResumeServer()
        {
            foreach (IServerForm2MethodHandler handler in methodHandlers)
            {
                handler.OnResumeServer();
            }
        }

        public override void OnFinishServer()
        {
            foreach (IServerForm2MethodHandler handler in methodHandlers)
            {
                handler.OnFinishServer();
            }            
        }

        public override void OnBroadCastMessage(string msg)
        {
            foreach (IServerForm2MethodHandler handler in methodHandlers)
            {
                handler.OnBroadCastMessage(msg);
            }                 
        }

        public override void OnVisUpCastEvent(string msg)
        {
            foreach (IServerForm2MethodHandler handler in methodHandlers)
            {
                handler.OnVisUpCastEvent(msg);
            }                  
        }

        public override void OnRobotUpCastEvent(string msg)
        {
            foreach (IServerForm2MethodHandler handler in methodHandlers)
            {
                handler.OnRobotUpCastEvent(msg);
            }                      
        }

        public override void OnMoverUpCastEvent(string msg)
        {
            foreach (IServerForm2MethodHandler handler in methodHandlers)
            {
                handler.OnMoverUpCastEvent(msg);
            }  
        }

        public override void OnClearText()
        {
            foreach (IServerForm2MethodHandler handler in methodHandlers)
            {
                handler.OnClearText();
            }  
        }

        public override void OnException(string point, string msg)
        {
            foreach (IServerForm2MethodHandler handler in methodHandlers)
            {
                handler.OnException(point,msg);
            }              
        }
    } 

    #endregion



    [assembly: log4net.Config.XmlConfigurator(Watch = true)]
    public class ServerLog : ServerForm2MethodHandler
    {
        private ILog log = null;
        private string logInfo = "";

        public string LogInfo
        {
            get { return logInfo; }
            set { logInfo = value; }
        }

        public ServerLog()
        {
            log = log4net.LogManager.GetLogger("server.Logging");
            Debug.Assert(log != null);
        }

        public ServerLog(string logname)
        {
            log = log4net.LogManager.GetLogger(logname);
            Debug.Assert(log != null);
        }

        public override void OnStartServer()
        {
            log.Info(startServerFmt);
        }

        public override void OnFinishServer()
        {
            log.Info(finishServerFmt);
        }
        public override void OnPauseServer()
        {
            log.Info(pauseServerFmt);
        }

        public override void OnResumeServer()
        {
            log.Info(resumeServerFmt);
        }

        public override void OnBroadCastMessage(string msg)
        {
            log.Info(String.Format(broadCastMessageFmt,msg));
        }

        public override void OnVisUpCastEvent(string msg)
        {
            log.Info(String.Format(VisUpCastMessageFmt, msg));
        }

        public override void OnRobotUpCastEvent(string msg)
        {
            log.Info(String.Format(robotUpCastMessageFmt, msg));
        }

        public override void OnMoverUpCastEvent(string msg)
        {
            log.Info(String.Format(MoverUpCastMessageFmt, msg));
        }

        public override void OnException(string addr, string msg)
        {
            log.Info(String.Format(exceptionMessageFmt, addr,msg));
        }
    }


    public class ServerMonitor : ServerForm2MethodHandler
    {
        protected string severState = "未启动";
        protected int numOfBroadCastEventObserver = 0;
        protected int numOfVisUpCastEventObserver = 0;
        protected int numOfRobotUpCastEventObserver = 0;
        protected int numOfMoverUpCastEventObserver = 0;
        protected int numberOfServerSend = 0;
        protected int numberOfServerRcv = 0;
        protected int numberOfVisSend = 0;
        protected int numberOfVisRcv = 0;
        protected int numberOfRobotSend = 0;
        protected int numberOfRobotRcv = 0;
        protected int numberOfMoverSend = 0;
        protected int numberOfMoverRcv = 0;
        protected int numberOfException = 0;

        private ServerForm2 frm;
        public ServerMonitor(ServerForm2 _frm)
        {
            frm = _frm;
        }


        #region 属性
        [Category("服务端")]
        [ReadOnly(true)]
        [Description("运行状态时可以正常收发信息；暂停状态时无法正常收发信息。")]
        public string A0_服务端状态
        {
            get { return severState; }
            set { severState = value; }
        }

        [Category("服务端")]
        [ReadOnly(true)]
        [Browsable(false)]
        public int A1_服务端事件观察者个数
        {
            get { return numOfBroadCastEventObserver; }
            set { numOfBroadCastEventObserver = value; }
        }

        [Category("服务端")]
        [ReadOnly(true)]
        public int A2_服务端发送次数
        {
            get { return numberOfServerSend; }
            set { numberOfServerSend = value; }
        }

        [Category("服务端")]
        [ReadOnly(true)]
        [Browsable(false)]
        public int A3_服务端接收次数
        {
            get { return numberOfServerRcv; }
            set { numberOfServerRcv = value; }
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
        [Browsable(false)]
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

        [Category("机器端")]
        [ReadOnly(true)]
        [Browsable(false)]
        public int A1_机器端事件观察者个数
        {
            get { return numOfRobotUpCastEventObserver; }
            set { numOfRobotUpCastEventObserver = value; }
        }

        [Category("机器端")]
        [ReadOnly(true)]
        [Browsable(false)]
        public int A2_机器端发送次数
        {
            get { return numberOfRobotSend; }
            set { numberOfRobotSend = value; }
        }

        [Category("机器端")]
        [ReadOnly(true)]
        public int A3_机器端接收次数
        {
            get { return numberOfRobotRcv; }
            set { numberOfRobotRcv = value; }
        }

        [Category("运动平台端")]
        [ReadOnly(true)]
        [Browsable(false)]
        public int A1_运动平台端事件观察者个数
        {
            get { return numOfMoverUpCastEventObserver; }
            set { numOfMoverUpCastEventObserver = value; }
        }

        [Category("运动平台端")]
        [ReadOnly(true)]
        [Browsable(false)]
        public int A2_运动平台端发送次数
        {
            get { return numberOfMoverSend; }
            set { numberOfMoverSend = value; }
        }

        [Category("运动平台端")]
        [ReadOnly(true)]
        public int A3_运动平台端接收次数
        {
            get { return numberOfMoverRcv; }
            set { numberOfMoverRcv = value; }
        }

        [Category("运行信息")]
        [ReadOnly(true)]
        protected int 异常发生次数
        {
            get { return numberOfException; }
            set { numberOfException = value; }
        }

        #endregion

        #region 使用小函数
        private void RefreshControl()
        {
            frm.PropertyGrid1.Refresh();
        } 
        #endregion


        public override void OnStartServer()
        {
            severState = ServerComm.StateString();
            RefreshControl();
        }

        public override void OnPauseServer()
        {
            severState = ServerComm.StateString();
            RefreshControl();
        }

        public override void OnResumeServer()
        {
            severState = ServerComm.StateString();
            RefreshControl();
        }

        public override void OnFinishServer()
        {
            severState = ServerComm.StateString();
            RefreshControl();
        }
        public override void OnBroadCastMessage(string msg)
        {
            numberOfServerSend++;
            RefreshControl();
        }

        public override void OnVisUpCastEvent(string msg)
        {
            numberOfVisRcv++;
            RefreshControl();
        }

        public override void OnRobotUpCastEvent(string msg)
        {
            numberOfRobotRcv++;
            RefreshControl();
        }

        public override void OnMoverUpCastEvent(string msg)
        {
            numberOfMoverRcv++;
            RefreshControl();
        }
        public override void OnClearText() { }

        public override void OnException(string point, string msg)
        {
            numberOfException++;
            RefreshControl();
        }

    }

    public class ServerUI : ServerForm2MethodHandler
    {
        private ServerForm2 frm;
        public ServerUI(ServerForm2 _frm)
        {
            frm = _frm;
        }



        #region 实用小函数

        /// <summary>
        /// 滚动到当前文本位置
        /// </summary>
        private void ScrollToCaret()
        {
            frm.TxtMessage.SelectionStart = frm.TxtMessage.TextLength;
            frm.TxtMessage.ScrollToCaret();
        }
        protected override string StartServerFmt{get { return startServerFmt+"\r\n"; }}

        protected override string FinishServerFmt{get { return finishServerFmt + "\r\n"; }}

        protected override string PauseServerFmt{get { return pauseServerFmt + "\r\n"; }}

        protected override string ResumeServerFmt{get { return resumeServerFmt + "\r\n"; }}

        protected override string BroadCastMessageFmt{get { return broadCastMessageFmt + "\r\n"; }}

        protected override string VisUpCastMessageFmt{get { return visUpCastMessageFmt + "\r\n"; }}

        protected override string RobotUpCastMessageFmt{get { return robotUpCastMessageFmt + "\r\n"; }}

        protected override string MoverUpCastMessageFmt{get { return moverUpCastMessageFmt + "\r\n"; }}

        protected override string ExceptionMessageFmt{get { return exceptionMessageFmt + "\r\n"; }}

        #endregion


        public override void OnStartServer()
        {
            frm.TxtMessage.AppendText(StartServerFmt);
            ScrollToCaret();
        }

        public override void OnPauseServer()
        {
            frm.TxtMessage.AppendText(PauseServerFmt);
            ScrollToCaret();
        }

        public override void OnResumeServer()
        {
            frm.TxtMessage.AppendText(ResumeServerFmt);
            ScrollToCaret();
        }

        public override void OnFinishServer()
        {
            frm.TxtMessage.AppendText(FinishServerFmt);
            ScrollToCaret();
        }

        public override void OnBroadCastMessage(string msg)
        {
            frm.TxtMessage.AppendText(String.Format(BroadCastMessageFmt,msg));
            ScrollToCaret();
        }

        public override void OnVisUpCastEvent(string msg)
        {
            frm.TxtMessage.AppendText(String.Format(VisUpCastMessageFmt, msg));
            ScrollToCaret();
        }

        public override void OnRobotUpCastEvent(string msg)
        {
            frm.TxtMessage.AppendText(String.Format(RobotUpCastMessageFmt, msg));
            ScrollToCaret();
        }

        public override void OnMoverUpCastEvent(string msg)
        {
            frm.TxtMessage.AppendText(String.Format(MoverUpCastMessageFmt, msg));
            ScrollToCaret();
        }

        public override void OnClearText()
        {
            frm.TxtMessage.Clear();
        }

        public override void OnException(string point, string msg)
        {
            frm.TxtMessage.AppendText(String.Format(ExceptionMessageFmt,point, msg));
            ScrollToCaret();
        }

    }



}
