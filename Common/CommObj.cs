using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Newtonsoft.Json;

namespace Qzeim.ThrdPrint.BroadCast.Common
{
    public class CommObj
    {
        private Int32 srcId;
        private Int32 destId;
        private string sendTime;
        private string rcvTime = "";
        private string dataType;
        private string dataBody;
        private string dataCmd = "TransData";

        public CommObj()
        {

        }

        public CommObj(Int32 _srcId,Int32 _destId,string _sendTime, string _dataType,string _dataBody,string _dataCmd = "TransData")
        {
            srcId = _srcId;
            destId = _destId;
            sendTime = _sendTime;
            dataType = _dataType;
            dataBody = _dataBody;
            dataCmd = _dataCmd;
        }


        /// <summary>
        /// 源，对于每一个子系统分配一个id给它
        /// </summary>
        public int SrcId
        {
            get { return srcId; }
            set { srcId = value; }
        }

        /// <summary>
        /// 目标，对于每一个子系统分配一个id给它
        /// </summary>
        public int DestId
        {
            get { return destId; }
            set { destId = value; }
        }

        /// <summary>
        /// 发送时间,精确到毫秒
        /// </summary>
        public string SendTime
        {
            get { return sendTime; }
            set { sendTime = value; }
        }

        /// <summary>
        /// 接收事件，精确到毫秒
        /// </summary>
        public string RcvTime
        {
            get { return rcvTime; }
            set { rcvTime = value; }
        }

        /// <summary>
        /// 消息的数据类型
        /// </summary>
        public string DataType
        {
            get { return dataType; }
            set { dataType = value; }
        }

        /// <summary>
        /// 消息的主体，为json字符串
        /// </summary>
        public string DataBody
        {
            get { return dataBody; }
            set { dataBody = value; }
        }

        /// <summary>
        /// 消息的用途，通常用以表征传递文件或者传递数据等命令
        /// </summary>
        public string DataCmd
        {
            get { return dataCmd; }
            set { dataCmd = value; }
        }

        public string ToString()
        {
            string str = String.Format(
                "\r\n---------------------------\r\n" +
                "srcID:{0:x4},\r\ndestID:{1:x4},\r\nsendTime:{2},\r\nrcv Time:{3},\r\ndataCmd:{4},\r\ndataType:{5},\r\ndataBody:{6}\r\n",
                srcId,destId,sendTime,rcvTime,dataCmd,dataType,dataBody);

            return str;
        }

        public static string ToJson(CommObj obj)
        {
            string json1 = JsonConvert.SerializeObject(obj);
            return json1;
        }

        public static CommObj FromJson(string json1)
        {
            CommObj obj = JsonConvert.DeserializeObject<CommObj>(json1);
            return obj;
        }

    }

}
