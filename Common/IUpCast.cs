using System;

namespace Qzeim.ThrdPrint.BroadCast.Common
{
    public delegate void UpCastEventHandler(string msg);

    public interface IUpCast
    {
        void SendMsg(string msg);
    }

    public interface IUpCastHandler
    {
        void OnVisUpCastEvent(string info);
        void OnRobotUpCastEvent(string info);
        void OnMoverUpCastEvent(string info);
    }
}
