using System;
using System.Net;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Configuration;

using System.Runtime.Serialization.Formatters;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using log4net;
using Qzeim.ThrdPrint.BroadCast.Common;
using Qzeim.ThrdPrint.BroadCast.Server;

namespace Qzeim.ThrdPrint.BroadCast.Client
{
	/// <summary>
	/// Form1 ��ժҪ˵����
	/// </summary>
    [assembly: log4net.Config.XmlConfigurator(Watch = true)]
	public class ClientForm : System.Windows.Forms.Form
	{
		/// <summary>
		/// ����������������
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button btnClear;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnCancle;
		private System.Windows.Forms.TextBox txtMessage;
        private Button btnSubscribe;
	    private bool isSubscribe = false;
        private Button btn_Send;


	    private void SetSubscribe(bool flag){isSubscribe = flag;}

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
            this.btnSubscribe = new System.Windows.Forms.Button();
            this.btn_Send = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(229, 248);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
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
            this.btnCancle.Location = new System.Drawing.Point(133, 248);
            this.btnCancle.Name = "btnCancle";
            this.btnCancle.Size = new System.Drawing.Size(75, 23);
            this.btnCancle.TabIndex = 4;
            this.btnCancle.Text = "Cancel";
            this.btnCancle.Click += new System.EventHandler(this.btnCancle_Click);
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(32, 40);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessage.Size = new System.Drawing.Size(272, 184);
            this.txtMessage.TabIndex = 5;
            // 
            // btnSubscribe
            // 
            this.btnSubscribe.Location = new System.Drawing.Point(34, 248);
            this.btnSubscribe.Name = "btnSubscribe";
            this.btnSubscribe.Size = new System.Drawing.Size(75, 23);
            this.btnSubscribe.TabIndex = 4;
            this.btnSubscribe.Text = "Subscribe";
            this.btnSubscribe.Click += new System.EventHandler(this.btnSubscribe_Click);
            // 
            // btn_Send
            // 
            this.btn_Send.Location = new System.Drawing.Point(34, 293);
            this.btn_Send.Name = "btn_Send";
            this.btn_Send.Size = new System.Drawing.Size(75, 23);
            this.btn_Send.TabIndex = 4;
            this.btn_Send.Text = "Send";
            this.btn_Send.Click += new System.EventHandler(this.btn_Send_Click);
            // 
            // ClientForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(336, 328);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.btn_Send);
            this.Controls.Add(this.btnSubscribe);
            this.Controls.Add(this.btnCancle);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnClear);
            this.Name = "ClientForm";
            this.Text = "ClientWatcher";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.ClientForm_Closing);
            this.Load += new System.EventHandler(this.ClientForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

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
		    StartClient();
		}

        private void ClientForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            FinishClient();
        }

        private void btnClear_Click(object sender, System.EventArgs e)
        {
            txtMessage.Text = "";
        }

		private void btnCancle_Click(object sender, System.EventArgs e)
		{
			watch.BroadCastEvent -= new BroadCastEventHandler(wrapper.BroadCasting);
            SetSubscribe(false);
            MessageBox.Show("ȡ�����Ĺ㲥�ɹ�!");
		}

	    private void btnSubscribe_Click(object sender, EventArgs e)
        {
            if (!isSubscribe)
            {
                watch.BroadCastEvent += new BroadCastEventHandler(wrapper.BroadCasting);
                SetSubscribe(true);
                MessageBox.Show("���¶��Ĺ㲥�ɹ�!");
            }
            else
            {
                MessageBox.Show("���Ѷ��Ĺ㲥�������ظ�����!");
            }
        }

        #region ͨ�Ų���

        private IBroadCast watch = null;
        private EventWrapper wrapper = null;
        private string rcvMsg = "";

        // �����ͻ���
        private void StartClient()
        {
            BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
            BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
            serverProvider.TypeFilterLevel = TypeFilterLevel.Full;

            IDictionary props = new Hashtable();
            props["port"] = 0;
            TcpChannel channel = new TcpChannel(props, clientProvider, serverProvider);
            ChannelServices.RegisterChannel(channel);

            //// ����ͨ���������ʽ
            //string cfg = "Client.config";
            //RemotingConfiguration.Configure(cfg);

            // ��config�ж�ȡ�������
            string broadCastObjURL = ConfigurationManager.AppSettings["BroadCastObjURL"];

            // ��ȡ�㲥Զ�̶���
            watch = (IBroadCast)Activator.GetObject(typeof(IBroadCast), broadCastObjURL);

            wrapper = new EventWrapper();
            wrapper.LocalBroadCastEvent += new BroadCastEventHandler(BroadCastingMessage);
            watch.BroadCastEvent += new BroadCastEventHandler(wrapper.BroadCasting);
            SetSubscribe(true);
        }

        // �����ͻ���
        private void FinishClient()
        {
            if (isSubscribe)
            {
                watch.BroadCastEvent -= new BroadCastEventHandler(wrapper.BroadCasting);
            }
        }

        // ��Ӧ�㲥��Ϣ
        public void BroadCastingMessage(string message)
        {
            CommObj commObj = CommObj.FromJson(message);

            if (commObj == null)
            {
                rcvMsg = "Json��������";
            }
            else
            {
                commObj.RcvTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                rcvMsg = commObj.ToString();
            }

            new Thread(Check).Start();

            ILog log = log4net.LogManager.GetLogger("server.Logging");
            log.Info("BroadCastingMessage--" + rcvMsg);

        }

        public void Check()
        {
            lock (this)
                Invoke(new MethodInvoker(delegate()
                {
                    txtMessage.Text += "I got it:" + rcvMsg;
                    txtMessage.Text += System.Environment.NewLine;
                }));
        }

        // ������Ϣ�������
        private void btn_Send_Click(object sender, EventArgs e)
        {
            SendToServerForm ssForm = new SendToServerForm();
            ssForm.StartPosition = FormStartPosition.CenterParent;
            ssForm.ShowDialog();
        }
        #endregion

	}
}
