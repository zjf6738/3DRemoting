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
//using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using Qzeim.ThrdPrint.BroadCast.RemoteObject;
using Qzeim.ThrdPrint.BroadCast.Common;
using log4net;

namespace Qzeim.ThrdPrint.BroadCast.Server
{
	/// <summary>
	/// Form1 的摘要说明。
	/// </summary>

    [assembly: log4net.Config.XmlConfigurator(Watch = true)]
	public class ServerForm : System.Windows.Forms.Form
    {
		private System.Windows.Forms.Button btnClear;
		private System.Windows.Forms.Button btnBC;
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.Container components = null;
        private Label label1;
        private Label label2;
        private TextBox txtBCMsg;
        private TextBox txtMessage;
        string rcvMsg = "";

		#region 客户端订阅服务端事件

		public static BroadCastObj Obj = null;

		#endregion

		public ServerForm()
		{
			//
			// Windows 窗体设计器支持所必需的
			//
			InitializeComponent();

			//
			// TODO: 在 InitializeComponent 调用后添加任何构造函数代码
			//
		}

		/// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows 窗体设计器生成的代码
		/// <summary>
		/// 设计器支持所需的方法 - 不要使用代码编辑器修改
		/// 此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
            this.btnClear = new System.Windows.Forms.Button();
            this.btnBC = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtBCMsg = new System.Windows.Forms.TextBox();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(32, 239);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 1;
            this.btnClear.Text = "&Clear";
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnBC
            // 
            this.btnBC.Location = new System.Drawing.Point(145, 239);
            this.btnBC.Name = "btnBC";
            this.btnBC.Size = new System.Drawing.Size(75, 23);
            this.btnBC.TabIndex = 2;
            this.btnBC.Text = "&BroadCast";
            this.btnBC.Click += new System.EventHandler(this.btnBC_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "服务器收发信息";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(32, 275);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "服务器广播信息";
            // 
            // txtBCMsg
            // 
            this.txtBCMsg.Location = new System.Drawing.Point(34, 307);
            this.txtBCMsg.Name = "txtBCMsg";
            this.txtBCMsg.Size = new System.Drawing.Size(472, 21);
            this.txtBCMsg.TabIndex = 4;
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(34, 40);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessage.Size = new System.Drawing.Size(472, 184);
            this.txtMessage.TabIndex = 6;
            // 
            // ServerForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(539, 359);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.txtBCMsg);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnBC);
            this.Controls.Add(this.btnClear);
            this.Name = "ServerForm";
            this.Text = "FileWatcher";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ServerForm_FormClosing);
            this.Load += new System.EventHandler(this.ServerForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

        ///// <summary>
        ///// 应用程序的主入口点。
        ///// </summary>
        //[STAThread]
        //static void Main() 
        //{
        //    Application.Run(new ServerForm());
        //}

        #region 窗体的加载、关闭等操作
        private void ServerForm_Load(object sender, System.EventArgs e)
        {
            StartServer();
            InitLog4net();
            //lbMonitor.Items.Add("Server started!");
            txtMessage.Text += "Server started!\r\n";
        }


        private void btnClear_Click(object sender, System.EventArgs e)
        {
            //lbMonitor.Items.Clear();
            txtMessage.Text = "";
        }

        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 释放通道
            //foreach (IChannel channel in ChannelServices.RegisteredChannels)
            //{
            //    ChannelServices.UnregisterChannel(channel);
            //}
            FinishServer();
        } 
        #endregion

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
            string moverUpCastObjURI = ConfigurationManager.AppSettings["MoverUpCastObjURI"];
            string robotUpCastObjURI = ConfigurationManager.AppSettings["RobotUpCastObjURI"];


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
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(RobotUpCastObj), robotUpCastObjURI, WellKnownObjectMode.Singleton);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(MoverUpCastObj), moverUpCastObjURI, WellKnownObjectMode.Singleton);

            UpCastObj.UpCastEvent += OnUpCastEvent;
            VisUpCastObj.UpCastEvent += OnVisUpCastEvent;
            RobotUpCastObj.UpCastEvent += OnRobotUpCastEvent;
            MoverUpCastObj.UpCastEvent += OnMoverUpCastEvent;

        }

	    private void FinishServer()
	    {
            // 释放通道
            foreach (IChannel channel in ChannelServices.RegisteredChannels)
            {
                ChannelServices.UnregisterChannel(channel);
            }

            UpCastObj.UpCastEvent -= OnUpCastEvent;

	    }



        // 响应客户端的事件
        public void OnUpCastEvent(string msg)
        {
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

            //lbMonitor.Items.Add(rcvMsg);
            new Thread(Check).Start();

            ILog log = log4net.LogManager.GetLogger("server.Logging");
            log.Info("OnUpCastEvent--" + rcvMsg);

        }

        public void Check()
        {
            lock (this)
                Invoke(new MethodInvoker(delegate()
                {
                    txtMessage.Text += rcvMsg;
                }));
        }

        public void OnVisUpCastEvent(string msg)
        {



            CommObj commObj = CommObj.FromJson(msg);

            if (commObj == null)
            {
                rcvMsg = "Json解析错误";
            }
            else
            {
                commObj.RcvTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                rcvMsg = commObj.ToString();

                Obj.BroadCastingInfo(msg);

            }

            new Thread(Check).Start();

            ILog log = log4net.LogManager.GetLogger("server.Logging");
            log.Info("OnVisUpCastEvent--" + rcvMsg);

        }

        public void OnRobotUpCastEvent(string msg)
        {
            
        }

        public void OnMoverUpCastEvent(string msg)
        {

        }

        // 广播事件
        private void btnBC_Click(object sender, System.EventArgs e)
        {
            //BroadCastForm bcForm = new BroadCastForm();
            //bcForm.StartPosition = FormStartPosition.CenterParent;
            //bcForm.ShowDialog();

            if (txtBCMsg.Text != string.Empty)
            {
                CommObj commObj = new CommObj();
                commObj.SrcId = 0x00000001;
                commObj.DestId = 0x00000000;
                commObj.SendTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                commObj.DataType = "String";
                commObj.DataBody = txtBCMsg.Text;

                string json = CommObj.ToJson(commObj);

                Obj.BroadCastingInfo(json);
                ILog log = log4net.LogManager.GetLogger("server.Logging");
                log.Info("Obj.BroadCastingInfo--" + json);
                // log.Error("error", new Exception("发生了一个异常"));
            }
            else
            {
                MessageBox.Show("请输入信息！");
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
}
