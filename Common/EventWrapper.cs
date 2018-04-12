using System;
using System.Runtime.Remoting.Messaging;

namespace Wayfarer.BroadCast.Common
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
			LocalBroadCastEvent(message);
		}

		public override object InitializeLifetimeService()
		{
			return null;
		}

	}
}
