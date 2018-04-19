using System;

namespace Qzeim.ThrdPrint.BroadCast.Common
{
	public delegate void BroadCastEventHandler(string info);	

	public interface IBroadCast
	{
		event BroadCastEventHandler BroadCastEvent;
		void BroadCastingInfo(string info);
	}
}
