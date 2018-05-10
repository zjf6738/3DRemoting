using System;
using System.Windows.Forms;
using System.Runtime.Remoting.Messaging;
using Qzeim.ThrdPrint.BroadCast.Common;

namespace Qzeim.ThrdPrint.BroadCast.RemoteObject
{
	/// <summary>
	/// Class1 的摘要说明。
	/// </summary>
	public class BroadCastObj:MarshalByRefObject,IBroadCast
	{	
		public event BroadCastEventHandler BroadCastEvent;

		#region IBroadCast 成员

		//[OneWay]
		public void BroadCastingInfo(string info)
		{
            string errText = "";
			if (BroadCastEvent != null)
			{
				BroadCastEventHandler tempEvent = null;

				int index = 1; //记录事件订阅者委托的索引，为方便标识，从1开始。
				foreach (Delegate del in BroadCastEvent.GetInvocationList())
				{
					try
					{
						tempEvent = (BroadCastEventHandler)del;
						tempEvent(info);
					}
					catch(Exception ex)
					{
                        errText += "事件订阅者" + index.ToString() + "发生错误,系统将取消事件订阅!\r\n";
						//MessageBox.Show("事件订阅者" + index.ToString() + "发生错误,系统将取消事件订阅!");
						BroadCastEvent -= tempEvent;
					}
					index++;
				}				
			}
			else
			{
			    errText += "事件未被订阅或订阅发生错误!";
                //MessageBox.Show("事件未被订阅或订阅发生错误!");
			}

		    if (!errText.Equals(""))
		    {
		        throw new Exception(errText);
		    }

		}

		#endregion

		public override object InitializeLifetimeService()
		{
			return null;
		}

	}
}
