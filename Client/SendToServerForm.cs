using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using System.Runtime.Serialization.Formatters;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using log4net;
using Qzeim.ThrdPrint.BroadCast.Common;

//using Wayfarer.BroadCast.Common;
//using Wayfarer.BroadCast.RemoteObject;

namespace Qzeim.ThrdPrint.BroadCast.Server
{
	/// <summary>
	/// SendToServerForm 的摘要说明。
	/// </summary>
    [assembly: log4net.Config.XmlConfigurator(Watch = true)]
	public class SendToServerForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnSend;
		private System.Windows.Forms.TextBox txtInfo;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnClose;
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.Container components = null;	

		#region 服务端订阅客户端事件

	    private IUpCast upCast = null;
//		private IBroadCast bc = null;
	    private TcpChannel channel = null;

		#endregion

		public SendToServerForm()
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
				if(components != null)
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
            this.btnSend = new System.Windows.Forms.Button();
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(208, 104);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 23);
            this.btnSend.TabIndex = 3;
            this.btnSend.Text = "&Send";
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtInfo
            // 
            this.txtInfo.Location = new System.Drawing.Point(24, 56);
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.Size = new System.Drawing.Size(264, 21);
            this.txtInfo.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(24, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 23);
            this.label1.TabIndex = 4;
            this.label1.Text = "发送信息：";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(104, 104);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "&Close";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // SendToServerForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(312, 149);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.txtInfo);
            this.Name = "SendToServerForm";
            this.Text = "SendToServerForm";
            this.Load += new System.EventHandler(this.SendToServerForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void btnClose_Click(object sender, System.EventArgs e)
		{

		    if (channel != null)
		    {
		        ChannelServices.UnregisterChannel(channel);
		    }

			this.Close();
		}

		private void btnSend_Click(object sender, System.EventArgs e)
		{
			if (txtInfo.Text != string.Empty)
            {
                CommObj commObj = new CommObj(0x10,
                                              0x01,
                                              DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                              "string",
                                              txtInfo.Text);

                string json = CommObj.ToJson(commObj);
                upCast.SendMsg(json);

                ILog log = log4net.LogManager.GetLogger("server.Logging");
                log.Info("upCast.SendMsg--" + json);
	
			}
			else
			{
				MessageBox.Show("请输入信息！");
			}

		}

        private void SendToServerForm_Load(object sender, System.EventArgs e)
		{
			#region 客户端订阅客户端事件

            BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
            BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
            serverProvider.TypeFilterLevel = TypeFilterLevel.Full;
//			
            IDictionary props = new Hashtable();
            props["port"] = 0;
            props["name"] = "ClientTcp";
            channel = new TcpChannel(props, clientProvider, serverProvider);
            ChannelServices.RegisterChannel(channel);
//			
//			bc = (IBroadCast)Activator.GetObject(
//				typeof(IBroadCast),"http://localhost:8086/BroadCastMessage.soap");

            //HttpChannel channel = new HttpChannel(1);
            //ChannelServices.RegisterChannel(channel);

            upCast = (IUpCast)Activator.GetObject(typeof(IUpCast),"tcp://localhost:8086/UpCastObj.soap");


			#endregion
		
		}
	}
}
