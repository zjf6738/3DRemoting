using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Configuration;
using log4net;

namespace CSharpModBusExample
{
    internal class SocketWrapper : IDisposable
    {
        private static string IP = ConfigurationManager.AppSettings["PLC_IP"];
        private static int Port = Int32.Parse(ConfigurationManager.AppSettings["PLC_Port"]);
        private static int TimeOut = Int32.Parse(ConfigurationManager.AppSettings["PLC_SocketTimeOut"]);

        public ILog Logger { get; set; }

        public bool IsConnected
        {
            get
            {
                return isConnected;
            }

            set
            {
                isConnected = value;
            }
        }

        private Socket socket = null;
        bool isConnected = false;

        public void Connect()
        {
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, TimeOut);

            IPEndPoint ip = new IPEndPoint(IPAddress.Parse(IP), Port);

            if(!IsConnected)
            {
                this.socket.Connect(ip);
                IsConnected = true;
            }

        }

        public byte[] Read(int length)
        {
            byte[] data = new byte[length];
            this.socket.Receive(data);
            this.Log("Receive:", data);
            return data;
        }

        public void Write(byte[] data)
        {
            this.Log("Send:", data);
            this.socket.Send(data);
        }

        private void Log(string type, byte[] data)
        {
            if (this.Logger != null)
            {
                StringBuilder logText = new StringBuilder(type);
                foreach (byte item in data)
                {
                    logText.Append(item.ToString() + " ");
                }

                //this.Logger.Write(logText.ToString());
            }
        }

        #region IDisposable 成员
        public void Dispose()
        {
            if (this.socket != null)
            {
                this.socket.Close();
            }
        }
        #endregion
    }
}
