using System;
using System.Net;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using System.Runtime.Serialization.Formatters;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;

using Wayfarer.BroadCast.Common;

namespace Wayfarer.BroadCast.Client
{
	/// <summary>
	/// Form1 ��ժҪ˵����
	/// </summary>
	public class ClientForm : System.Windows.Forms.Form
	{
		/// <summary>
		/// ����������������
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button btnClear;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnCancle;
		private IBroadCast watch = null;
		private System.Windows.Forms.TextBox txtMessage;
		private EventWrapper wrapper = null;

		public ClientForm()
		{
			//
			// Windows ���������֧���������
			//
			InitializeComponent();

			//
			// TODO: �� InitializeComponent ���ú�����κι��캯������
			//
		}

		/// <summary>
		/// ������������ʹ�õ���Դ��
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

		#region Windows ������������ɵĴ���
		/// <summary>
		/// �����֧������ķ��� - ��Ҫʹ�ô���༭���޸�
		/// �˷��������ݡ�
		/// </summary>
		private void InitializeComponent()
		{
			this.btnClear = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.btnCancle = new System.Windows.Forms.Button();
			this.txtMessage = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// btnClear
			// 
			this.btnClear.Location = new System.Drawing.Point(224, 248);
			this.btnClear.Name = "btnClear";
			this.btnClear.TabIndex = 1;
			this.btnClear.Text = "&Clear";
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(32, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(120, 23);
			this.label1.TabIndex = 3;
			this.label1.Text = "�յ�����˵Ĺ㲥:";
			// 
			// btnCancle
			// 
			this.btnCancle.Location = new System.Drawing.Point(112, 248);
			this.btnCancle.Name = "btnCancle";
			this.btnCancle.TabIndex = 4;
			this.btnCancle.Text = "Cancel";
			this.btnCancle.Click += new System.EventHandler(this.btnCancle_Click);
			// 
			// txtMessage
			// 
			this.txtMessage.Location = new System.Drawing.Point(32, 40);
			this.txtMessage.Multiline = true;
			this.txtMessage.Name = "txtMessage";
			this.txtMessage.Size = new System.Drawing.Size(272, 184);
			this.txtMessage.TabIndex = 5;
			this.txtMessage.Text = "";
			// 
			// ClientForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
			this.ClientSize = new System.Drawing.Size(336, 285);
			this.Controls.Add(this.txtMessage);
			this.Controls.Add(this.btnCancle);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnClear);
			this.Name = "ClientForm";
			this.Text = "ClientWatcher";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.ClientForm_Closing);
			this.Load += new System.EventHandler(this.ClientForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Ӧ�ó��������ڵ㡣
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new ClientForm());
		}

		private void ClientForm_Load(object sender, System.EventArgs e)
		{
			BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
			BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
			serverProvider.TypeFilterLevel = TypeFilterLevel.Full;

			IDictionary props = new Hashtable();
			props["port"] = 0;
			HttpChannel channel = new HttpChannel(props,clientProvider,serverProvider);
			ChannelServices.RegisterChannel(channel);

			watch = (IBroadCast)Activator.GetObject(
				typeof(IBroadCast),"http://localhost:8080/BroadCastMessage.soap");

			wrapper = new EventWrapper();	
			wrapper.LocalBroadCastEvent += new BroadCastEventHandler(BroadCastingMessage);
			watch.BroadCastEvent += new BroadCastEventHandler(wrapper.BroadCasting);				
		}

		private void btnClear_Click(object sender, System.EventArgs e)
		{
			txtMessage.Text = "";
		}

		public void BroadCastingMessage(string message)
		{
			txtMessage.Text += "I got it:" + message;				
			txtMessage.Text += System.Environment.NewLine;
		}

		private void btnCancle_Click(object sender, System.EventArgs e)
		{
			watch.BroadCastEvent -= new BroadCastEventHandler(wrapper.BroadCasting);
			MessageBox.Show("ȡ�����Ĺ㲥�ɹ�!");
		}

		private void ClientForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//watch.BroadCastEvent -= new BroadCastEventHandler(wrapper.BroadCasting);
		}		
	}
}
