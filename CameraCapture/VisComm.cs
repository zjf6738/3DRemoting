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
        
        private IBroadCastHandler broadCastHandler; // �ͻ��˴���
        private IBroadCast watch = null; // watch����
        private EventWrapper wrapper = null; // wrapper����
        private IUpCast upCast = null; // upCast Զ�̶���
        private string rcvMsg = ""; //  ���յ�����Ϣ

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

            // ��config�ж�ȡ�������
            string broadCastObjURL = ConfigurationManager.AppSettings["BroadCastObjURL"];
            string upCastObjURL = ConfigurationManager.AppSettings["VisUpCastObjURL"];

            try
            {
                // ��ȡ�㲥Զ�̶���
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
                // ж��Զ�̶�����¼�����
                watch.BroadCastEvent -= new BroadCastEventHandler(wrapper.BroadCasting);
                upCast = null;

                // �ͷ�ͨ��
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
                // ж��Զ�̶�����¼�����
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
                //  ����ע��Զ�̶�����¼�����
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
                throw new Exception("�ͻ��˲���������״̬���޷������շ�!");
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
        public int state = 0; // 0:��ʼ״̬��1������״̬��2����ͣ״̬��3������״̬

        public string GetStateString( )
        {
            switch (state)
            {
                case (int)StateEnum.Starting: return "��ʼ״̬";
                case (int)StateEnum.Running: return "����״̬";
                case (int)StateEnum.Paused: return "��ͣ״̬";
                case (int)StateEnum.Finished: return "����״̬";
                default: return "δ֪״̬";
            }
        }
        public void SetState(int _state) { state = _state; }
        public int GetState(){return state;}

    }

}