using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Qzeim.ThrdPrint.BroadCast.Common;
using Qzeim.ThrdPrint.BroadCast.RemoteObject;

namespace Qzeim.ThrdPrint.BroadCast.Server
{
    public class ServerComm
    {
        public enum StateEnum
        {
            Starting = 0,
            Running,
            Paused,
            Finished
        };

        public static BroadCastObj Obj = null;
        public static int state = 0; // 0:开始状态，1：运行状态，2：暂停状态，3：结束状态

        IUpCastHandler upCastHandler = null;
        private string rcvMsg = "";

        public ServerComm(IUpCastHandler _upCastHandler)
        {
            upCastHandler = _upCastHandler;
        }

        public static string StateString()
        {
            switch ((int)state)
            {
                case (int)StateEnum.Starting:
                    return "开始状态";
                case (int)StateEnum.Running:
                    return "运行状态";
                case (int)StateEnum.Paused:
                    return "暂停状态";
                case (int)StateEnum.Finished:
                    return "结束状态";
                default:
                    return "未知状态";
            }
        }


        public string RcvMsg
        {
            get { return rcvMsg; }
            set { rcvMsg = value; }
        }

        public static int State
        {
            get { return state; }
            set { state = value; }
        }

        /// <summary>
        /// 启动服务器
        /// </summary>
        public void StartServer()
        {
            BinaryServerFormatterSinkProvider serverProvider = new BinaryServerFormatterSinkProvider();
            BinaryClientFormatterSinkProvider clientProvider = new BinaryClientFormatterSinkProvider();
            serverProvider.TypeFilterLevel = TypeFilterLevel.Full;

            // 由config中读取相关数据
            string channelName = ConfigurationManager.AppSettings["ChannelName"];
            string channelType = ConfigurationManager.AppSettings["ChannelType"];
            string channelPort = ConfigurationManager.AppSettings["ChannelPort"];

            IDictionary props = new Hashtable();
            props["name"] = channelName;
            props["port"] = channelPort;
            TcpChannel channel = new TcpChannel(props, clientProvider, serverProvider);
            ChannelServices.RegisterChannel(channel);

            // 客户端订阅服务端广播事件
            // 将远程对象推送到通道中，这样就可以让客户端进行访问
            Obj = new BroadCastObj();
            ConnectBroadCastObj();

            // 客户端访问地址注册
            RegisterClientRemoteObjServiceType();
            // 订阅客户端事件
            RegisterClientRemoteObjEvents();


            State = (int)StateEnum.Running;
        }



        /// <summary>
        /// 结束服务器
        /// </summary>
        public void FinishServer()
        {
            // 如果未初始化，则直接返回
            if (State == (int)StateEnum.Starting) return;
        

            // 广播远程对象
            if (State == (int) StateEnum.Running)
            {
                RemotingServices.Disconnect(Obj);
            }


            // 释放通道
            foreach (IChannel channel in ChannelServices.RegisteredChannels)
            {
                ChannelServices.UnregisterChannel(channel);
            }

            UnregisterClientRemoteObjEvents();

            State = (int)StateEnum.Finished;
        }

        /// <summary>
        /// 服务器暂停，这里的暂停为事件意义上的暂停
        /// </summary>
        public void PauseServer()
        {
            // 如果未初始化，则直接返回
            if (State != (int)StateEnum.Running) return;

            DisconnectBroadCastObj();
            UnregisterClientRemoteObjEvents();

            State = (int)StateEnum.Paused;
        }

        /// <summary>
        /// 恢复服务器，这里的恢复为事件意义上的恢复
        /// </summary>
        public void ResumeServer()
        {
            if (State != (int)StateEnum.Paused) return;

            ConnectBroadCastObj();
            RegisterClientRemoteObjEvents();

            State = (int)StateEnum.Running;
        }

        /// <summary>
        /// 推送广播远程到通道
        /// </summary>
        private static void ConnectBroadCastObj()
        {
            string broadCastObjURI = ConfigurationManager.AppSettings["BroadCastObjURI"];
            ObjRef objRef = RemotingServices.Marshal(Obj, broadCastObjURI);
        }

        /// <summary>
        /// 断开广播远程对象与通道的连接
        /// </summary>
        private static void DisconnectBroadCastObj()
        {
            RemotingServices.Disconnect(Obj);
        }

        /// <summary>
        /// 注册客户端远程对象的服务类别
        /// </summary>
        private static void RegisterClientRemoteObjServiceType()
        {
            string visUpCastObjURI = ConfigurationManager.AppSettings["VisUpCastObjURI"];
            string moverUpCastObjURI = ConfigurationManager.AppSettings["MoverUpCastObjURI"];
            string robotUpCastObjURI = ConfigurationManager.AppSettings["RobotUpCastObjURI"];

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(VisUpCastObj), visUpCastObjURI,WellKnownObjectMode.Singleton);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(RobotUpCastObj), robotUpCastObjURI,WellKnownObjectMode.Singleton);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(MoverUpCastObj), moverUpCastObjURI,WellKnownObjectMode.Singleton);
        }

        /// <summary>
        /// 注册客户端远程对象事件
        /// </summary>
        private void RegisterClientRemoteObjEvents()
        {
            VisUpCastObj.UpCastEvent += upCastHandler.OnVisUpCastEvent;
            RobotUpCastObj.UpCastEvent += upCastHandler.OnRobotUpCastEvent;
            MoverUpCastObj.UpCastEvent += upCastHandler.OnMoverUpCastEvent;
        }

        /// <summary>
        /// 卸载客户端远程对象事件
        /// </summary>
        private void UnregisterClientRemoteObjEvents()
        {
            VisUpCastObj.UpCastEvent -= upCastHandler.OnVisUpCastEvent;
            RobotUpCastObj.UpCastEvent -= upCastHandler.OnRobotUpCastEvent;
            MoverUpCastObj.UpCastEvent -= upCastHandler.OnMoverUpCastEvent;
        }


    }
}
