using System;
using System.Runtime.Remoting.Messaging;

namespace Qzeim.ThrdPrint.BroadCast.Common
{
	/// <summary>
	/// EventClass ��ժҪ˵����
	/// </summary>
	public class EventWrapper:MarshalByRefObject
	{
		public event BroadCastEventHandler LocalBroadCastEvent;

		//[OneWay]
		public void BroadCasting(string message)
		{
		    if (LocalBroadCastEvent != null)
		    {
		        LocalBroadCastEvent(message);
		    }

		}

		public override object InitializeLifetimeService()
		{
			return null;
		}

	}
}
