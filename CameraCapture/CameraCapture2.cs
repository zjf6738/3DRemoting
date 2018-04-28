using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CameraCapture
{
    public partial class CameraCapture2 : Form
    {
        private ServerMonitor serverMonitor;


        public CameraCapture2()
        {
            InitializeComponent();

            serverMonitor = new ServerMonitor();
            propertyGrid1.SelectedObject = serverMonitor;
        }
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
    }
}
