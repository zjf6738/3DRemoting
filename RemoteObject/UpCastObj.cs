using System;
using System.Windows.Forms;
using System.Runtime.Remoting.Messaging;
using Qzeim.ThrdPrint.BroadCast.Common;

namespace Qzeim.ThrdPrint.BroadCast.RemoteObject
{
    public class UpCastObj : MarshalByRefObject, IUpCast
    {
        public static event UpCastEventHandler UpCastEvent;

        #region

        public void SendMsg(string msg)
        {
            if (UpCastEvent != null)
            {
                UpCastEvent(msg);
            }
        }

        #endregion

        public override object InitializeLifetimeService()
        {
            return null;
        }

    }
}
