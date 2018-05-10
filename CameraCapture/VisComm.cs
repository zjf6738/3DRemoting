using System;
using System.Collections;
using System.Configuration;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using System.Windows.Forms;
using Qzeim.ThrdPrint.BroadCast.Common;

namespace CameraCapture
{
    public class VisComm
    {
        
        private IBroadCastHandler broadCastHandler; // 客户端窗口
        private IBroadCast watch = null; // watch对象
        private EventWrapper wrapper = null; // wrapper对象
        private IUpCast upCast = null; // upCast 远程对象
        private string rcvMsg = ""; //  接收到的消息

        private VisCommState visCommState = null;

        public VisComm(IBroadCastHandler _broadCastHandler)
        {
            broadCastHandler = _broadCastHandler;
            visCommState = new VisCommState(); 
        }

        public string RcvMsg
        {
            get { return rcvMsg; }
            set { rcvMsg = value; }
        }

        public VisCommState CommState
        {
            get { return visCommState; }
            set { visCommState = value; }
        }

        public void StartClient()
        {
            BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
            BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
            serverProvider.TypeFilterLevel = TypeFilterLevel.Full;

            IDictionary props = new Hashtable();
            props["port"] = 0;
            TcpChannel channel = new TcpChannel(props, clientProvider, serverProvider);
            ChannelServices.RegisterChannel(channel);

            // 由config中读取相关数据
            string broadCastObjURL = ConfigurationManager.AppSettings["BroadCastObjURL"];
            string upCastObjURL = ConfigurationManager.AppSettings["VisUpCastObjURL"];

            try
            {
                // 获取广播远程对象
                watch = (IBroadCast)Activator.GetObject(typeof(IBroadCast), broadCastObjURL);
                wrapper = new EventWrapper();
                wrapper.LocalBroadCastEvent += new BroadCastEventHandler(broadCastHandler.OnBroadCastingInfo);
                watch.BroadCastEvent += new BroadCastEventHandler(wrapper.BroadCasting);

                // upcast
                upCast = (IUpCast)Activator.GetObject(typeof(IUpCast), upCastObjURL);

                visCommState.SetState((int)VisCommState.StateEnum.Running);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public void FinishClient()
        {
            if (CommState.GetState() == (int)VisCommState.StateEnum.Starting) return;

            try
            {
                // 卸载远程对象的事件函数
                watch.BroadCastEvent -= new BroadCastEventHandler(wrapper.BroadCasting);
                upCast = null;

                // 释放通道
                foreach (IChannel channel in ChannelServices.RegisteredChannels)
                {
                    ChannelServices.UnregisterChannel(channel);
                }

                visCommState.SetState((int)VisCommState.StateEnum.Finished);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public void PauseClient()
        {
            if (CommState.GetState() == (int)VisCommState.StateEnum.Paused) return;

            try
            {
                // 卸载远程对象的事件函数
                watch.BroadCastEvent -= new BroadCastEventHandler(wrapper.BroadCasting);
                upCast = null;
                visCommState.SetState((int)VisCommState.StateEnum.Paused);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public void ResumeClient()
        {
            if (CommState.GetState() == (int)VisCommState.StateEnum.Running) return;

            string upCastObjURL = ConfigurationManager.AppSettings["VisUpCastObjURL"];

            try
            {
                //  重新注册远程对象的事件函数
                watch.BroadCastEvent += new BroadCastEventHandler(wrapper.BroadCasting);
                upCast = (IUpCast)Activator.GetObject(typeof(IUpCast), upCastObjURL);
                visCommState.SetState((int)VisCommState.StateEnum.Running);
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public void SendToServer(string json)
        {
            if (CommState.GetState() != (int) VisCommState.StateEnum.Running)
            {
                throw new Exception("客户端不处于运行状态，无法正常收发!");
            }

            try
            {
                upCast.SendMsg(json);
            }
            catch (Exception)
            {
                throw;
            }

        }
    }

    public class VisCommState
    {
        public enum StateEnum
        {
            Starting = 0,
            Running,
            Paused,
            Finished
        };
        public int state = 0; // 0:开始状态，1：运行状态，2：暂停状态，3：结束状态

        public string GetStateString( )
        {
            switch (state)
            {
                case (int)StateEnum.Starting: return "开始状态";
                case (int)StateEnum.Running: return "运行状态";
                case (int)StateEnum.Paused: return "暂停状态";
                case (int)StateEnum.Finished: return "结束状态";
                default: return "未知状态";
            }
        }
        public void SetState(int _state) { state = _state; }
        public int GetState(){return state;}

    }

}