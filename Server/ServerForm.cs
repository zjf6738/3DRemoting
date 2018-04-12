using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Serialization.Formatters;

using Wayfarer.BroadCast.Common;
using Wayfarer.BroadCast.RemoteObject;

namespace Wayfarer.BroadCast.Server
{
	/// <summary>
	/// Form1 的摘要说明。
	/// </summary>
	public class ServerForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ListBox lbMonitor;
		private System.Windows.Forms.Button btnClear;
		private System.Windows.Forms.Button btnBC;
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.Container components = null;	

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
			this.lbMonitor = new System.Windows.Forms.ListBox();
			this.btnClear = new System.Windows.Forms.Button();
			this.btnBC = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// lbMonitor
			// 
			this.lbMonitor.ItemHeight = 12;
			this.lbMonitor.Location = new System.Drawing.Point(32, 24);
			this.lbMonitor.Name = "lbMonitor";
			this.lbMonitor.Size = new System.Drawing.Size(232, 184);
			this.lbMonitor.TabIndex = 0;
			// 
			// btnClear
			// 
			this.btnClear.Location = new System.Drawing.Point(88, 240);
			this.btnClear.Name = "btnClear";
			this.btnClear.TabIndex = 1;
			this.btnClear.Text = "&Clear";
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			// 
			// btnBC
			// 
			this.btnBC.Location = new System.Drawing.Point(192, 240);
			this.btnBC.Name = "btnBC";
			this.btnBC.TabIndex = 2;
			this.btnBC.Text = "&BroadCast";
			this.btnBC.Click += new System.EventHandler(this.btnBC_Click);
			// 
			// ServerForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
			this.ClientSize = new System.Drawing.Size(296, 285);
			this.Controls.Add(this.btnBC);
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.lbMonitor);
			this.Name = "ServerForm";
			this.Text = "FileWatcher";
			this.Load += new System.EventHandler(this.ServerForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// 应用程序的主入口点。
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new ServerForm());
		}

		private void StartServer()
		{
			BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
			BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
			serverProvider.TypeFilterLevel = TypeFilterLevel.Full;

			IDictionary props = new Hashtable();
			props["port"] = 8080;
            HttpChannel channel = new HttpChannel(props,clientProvider,serverProvider);
			ChannelServices.RegisterChannel(channel);

			#region 客户端订阅服务端事件

			Obj = new BroadCastObj();
			ObjRef objRef = RemotingServices.Marshal(Obj,"BroadCastMessage.soap");	

			#endregion
		
			#region 客户端订阅客户端事件

//			RemotingConfiguration.RegisterWellKnownServiceType(
//							typeof(BroadCastObj),"BroadCastMessage.soap",
//							WellKnownObjectMode.Singleton);		

			#endregion
		}		

		private void ServerForm_Load(object sender, System.EventArgs e)
		{
			StartServer();
			lbMonitor.Items.Add("Server started!");
		}

		private void btnClear_Click(object sender, System.EventArgs e)
		{
			lbMonitor.Items.Clear();
		}

		private void btnBC_Click(object sender, System.EventArgs e)
		{			
			BroadCastForm bcForm = new BroadCastForm();
			bcForm.StartPosition = FormStartPosition.CenterParent;
			bcForm.ShowDialog();
		}		
	}
}
