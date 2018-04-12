using System;
using System.Windows.Forms;
using System.Runtime.Remoting.Messaging;
using Wayfarer.BroadCast.Common;

namespace Wayfarer.BroadCast.RemoteObject
{
	/// <summary>
	/// Class1 ��ժҪ˵����
	/// </summary>
	public class BroadCastObj:MarshalByRefObject,IBroadCast
	{	
		public event BroadCastEventHandler BroadCastEvent;

		#region IBroadCast ��Ա

		//[OneWay]
		public void BroadCastingInfo(string info)
		{
			if (BroadCastEvent != null)
			{
				BroadCastEventHandler tempEvent = null;

				int index = 1; //��¼�¼�������ί�е�������Ϊ�����ʶ����1��ʼ��
				foreach (Delegate del in BroadCastEvent.GetInvocationList())
				{
					try
					{
						tempEvent = (BroadCastEventHandler)del;
						tempEvent(info);
					}
					catch
					{						
						MessageBox.Show("�¼�������" + index.ToString() + "��������,ϵͳ��ȡ���¼�����!");
						BroadCastEvent -= tempEvent;
					}
					index++;
				}				
			}
			else
			{
                MessageBox.Show("�¼�δ�����Ļ��ķ�������!");
			}
		}

		#endregion

		public override object InitializeLifetimeService()
		{
			return null;
		}

	}
}
