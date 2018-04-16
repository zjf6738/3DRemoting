using System;

namespace Wayfarer.BroadCast.Common
{
    public delegate void UpCastEventHandler(string msg);

    public interface IUpCast
    {
        void SendMsg(string msg);
    }
}
