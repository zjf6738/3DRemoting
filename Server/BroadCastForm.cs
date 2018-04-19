using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

using System.Runtime.Serialization.Formatters;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;

using Qzeim.ThrdPrint.BroadCast.Common;
using Qzeim.ThrdPrint.BroadCast.RemoteObject;

namespace Qzeim.ThrdPrint.BroadCast.Server
{
	/// <summary>
	/// BroadCastForm 的摘要说明。
	/// </summary>
	public class BroadCastForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button btnSend;
		private System.Windows.Forms.TextBox txtInfo;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnClose;
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.Container components = null;	

		#region 客户端订阅客户端事件

//		private IBroadCast bc = null;

		#endregion

		public BroadCastForm()
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
            this.label1.Text = "广播信息：";
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
            // BroadCastForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(312, 149);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.txtInfo);
            this.Name = "BroadCastForm";
            this.Text = "SendToServerForm";
            this.Load += new System.EventHandler(this.BroadCastForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void btnClose_Click(object sender, System.EventArgs e)
		{	
			#region 客户端订阅客户端事件

            foreach (IChannel channel in ChannelServices.RegisteredChannels)
            {
                ChannelServices.UnregisterChannel(channel);
            }

			#endregion
			this.Close();
		}

		private void btnSend_Click(object sender, System.EventArgs e)
		{
			if (txtInfo.Text != string.Empty)
			{ 					
				#region 客户端订阅服务端事件

				ServerForm.Obj.BroadCastingInfo("send time--"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+"--:   "+txtInfo.Text);
				#endregion

				#region 客户端订阅客户端事件

				//				bc.BroadCastingInfo(txtInfo.Text);

				#endregion				
			}
			else
			{
				MessageBox.Show("请输入信息！");
			}
		}

		private void BroadCastForm_Load(object sender, System.EventArgs e)
		{
			#region 客户端订阅客户端事件

//			BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
//			BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
//			serverProvider.TypeFilterLevel = TypeFilterLevel.Full;
//			
//			IDictionary props = new Hashtable();
//			props["port"] = 0;
//			props["name"] = "ClientHttp";
//			HttpChannel channel = new HttpChannel(props,clientProvider,serverProvider);
//			ChannelServices.RegisterChannel(channel);
//			
//			bc = (IBroadCast)Activator.GetObject(
//				typeof(IBroadCast),"http://localhost:8080/BroadCastMessage.soap");

			#endregion
		
		}
	}
}
