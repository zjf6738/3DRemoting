using System;
using System.Drawing;
using System.Collections;
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
    public partial class ServerForm2 : Form
    {
        public static BroadCastObj Obj = null;

        private ServerMonitor serverMonitor = null;
        private string rcvMsg = ""; // 线程安全中间变量


        public ServerForm2()
        {
            InitializeComponent();

            #region 数据初始化

            serverMonitor = new ServerMonitor(this);
            
            #endregion


            #region 控件数据绑定

            propertyGrid1.SelectedObject = serverMonitor;

            #endregion

        }

        private void ServerForm2_Load(object sender, EventArgs e)
        {
            StartServer();
            InitLog4net();

            #region 信息监控
            txtMessage.Text += "Server started!\r\n";

            serverMonitor.Update();
            #endregion
        }

        private void ServerForm2_FormClosing(object sender, FormClosingEventArgs e)
        {
            FinishServer();
        }

        #region 通信部分

        // 初始化及配置
        private void StartServer()
        {
            BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
            BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
            serverProvider.TypeFilterLevel = TypeFilterLevel.Full;

            // 由config中读取相关数据
            string channelName = ConfigurationManager.AppSettings["ChannelName"];
            string channelType = ConfigurationManager.AppSettings["ChannelType"];
            string channelPort = ConfigurationManager.AppSettings["ChannelPort"];
            string broadCastObjURI = ConfigurationManager.AppSettings["BroadCastObjURI"];
            string upCastObjURI = ConfigurationManager.AppSettings["UpCastObjURI"];
            string visUpCastObjURI = ConfigurationManager.AppSettings["VisUpCastObjURI"];


            IDictionary props = new Hashtable();
            props["name"] = channelName;
            props["port"] = channelPort;
            TcpChannel channel = new TcpChannel(props, clientProvider, serverProvider);
            ChannelServices.RegisterChannel(channel);

            // 客户端订阅服务端广播事件
            // 将远程对象推送到通道中，这样就可以让客户端进行访问
            Obj = new BroadCastObj();
            ObjRef objRef = RemotingServices.Marshal(Obj, broadCastObjURI);

            // 服务端订阅客户端事件
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(UpCastObj), upCastObjURI, WellKnownObjectMode.Singleton);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(VisUpCastObj), visUpCastObjURI, WellKnownObjectMode.Singleton);

            UpCastObj.UpCastEvent += OnUpCastEvent;
            VisUpCastObj.UpCastEvent += OnVisUpCastEvent;

            #region 信息监控


            serverMonitor.Update();
            #endregion


        }

        private void FinishServer()
        {
            // 释放通道
            foreach (IChannel channel in ChannelServices.RegisteredChannels)
            {
                ChannelServices.UnregisterChannel(channel);
            }

            UpCastObj.UpCastEvent -= OnUpCastEvent;
            VisUpCastObj.UpCastEvent -= OnUpCastEvent;

        }



        // 响应客户端的事件
        private void DefaultUpCastEventHandler(string msg)
        {

            // 接收并提取信息
            CommObj commObj = CommObj.FromJson(msg);

            if (commObj == null)
            {
                rcvMsg = "Json解析错误";
            }
            else
            {
                commObj.RcvTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                rcvMsg = commObj.ToString();
            }

            // 线程安全
            new Thread(Check).Start();

            // 广播信息
            switch (commObj.DestId)
            {
                case 0: // 

                    break;
            }

        }

        public void Check()
        {
            lock (this)
                Invoke(new MethodInvoker(delegate()
                {
                    #region 信息监控

                    txtMessage.Text += rcvMsg;
                    serverMonitor.Update();
                    log.Info("OnUpCastEvent--" + rcvMsg);

                    #endregion
                }));
        }


        public void OnUpCastEvent(string msg)
        {
            DefaultUpCastEventHandler(msg);
        }

        

        public void OnVisUpCastEvent(string msg)
        {
            DefaultUpCastEventHandler(msg);
            //CommObj commObj = CommObj.FromJson(msg);

            //if (commObj == null)
            //{
            //    rcvMsg = "Json解析错误";
            //}
            //else
            //{
            //    commObj.RcvTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            //    rcvMsg = commObj.ToString();
            //}

            //new Thread(Check).Start();

            //ILog log = log4net.LogManager.GetLogger("server.Logging");
            //log.Info("OnVisUpCastEvent--" + rcvMsg);

        }



        // 广播事件
        private void 广播ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            string text = "这是广播的测试信息";


            try
            {
                CommObj commObj = new CommObj();
                commObj.SrcId = 0x00000001;
                commObj.DestId = 0x00000000;
                commObj.SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                commObj.DataType = "String";
                commObj.DataBody = text;

                string json = CommObj.ToJson(commObj);

                Obj.BroadCastingInfo(json);
                ILog log = log4net.LogManager.GetLogger("server.Logging");
                log.Info("Obj.BroadCastingInfo--" + json);
            }
            catch (Exception)
            {
                
                throw;
            }

        }


        #endregion


        #region 日志模块
        ILog log = null;

        private void InitLog4net()
        {
            log = log4net.LogManager.GetLogger("server.Logging");
            Debug.Assert(log != null);
        }

        #endregion
    }

    public class ServerMonitor
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

        //protected string severState = "已启动";
        //protected int numOfBroadCastEventObserver = 3;
        //protected int numOfVisUpCastEventObserver = 1;
        //protected int numOfRobotUpCastEventObserver = 1;
        //protected int numOfMoverUpCastEventObserver = 1;
        //protected int numberOfServerSend = 5;
        //protected int numberOfServerRcv = 5;
        //protected int numberOfVisSend = 3;
        //protected int numberOfVisRcv = 5;
        //protected int numberOfRobotSend = 2;
        //protected int numberOfRobotRcv = 5;
        //protected int numberOfMoverSend = 0;
        //protected int numberOfMoverRcv = 5;



        private ServerForm2 frm;
        public ServerMonitor(ServerForm2 _frm)
        {
            frm = _frm;
        }


        #region 属性
        [Category("服务端")]
        [ReadOnly(true)]
        public string A0_服务端状态
        {
            get { return severState; }
            set { severState = value; }
        }

        [Category("服务端")]
        [ReadOnly(true)]
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
        public int A3_服务端接收次数
        {
            get { return numberOfServerRcv; }
            set { numberOfServerRcv = value; }
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

        [Category("机器端")]
        [ReadOnly(true)]
        public int A1_机器端事件观察者个数
        {
            get { return numOfRobotUpCastEventObserver; }
            set { numOfRobotUpCastEventObserver = value; }
        }

        [Category("机器端")]
        [ReadOnly(true)]
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
        public int A1_运动平台端事件观察者个数
        {
            get { return numOfMoverUpCastEventObserver; }
            set { numOfMoverUpCastEventObserver = value; }
        }

        [Category("运动平台端")]
        [ReadOnly(true)]
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


        #endregion



        public void Update()
        {

        }



    }


}
