using System;

namespace Qzeim.ThrdPrint.BroadCast.Common
{
    public delegate void UpCastEventHandler(string msg);

    public interface IUpCast
    {
        void SendMsg(string msg);
    }
}
