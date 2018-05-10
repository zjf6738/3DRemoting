using System;
using System.Windows.Forms;
using System.Runtime.Remoting.Messaging;
using Qzeim.ThrdPrint.BroadCast.Common;

namespace Qzeim.ThrdPrint.BroadCast.RemoteObject
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
            string errText = "";
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
					catch(Exception ex)
					{
                        errText += "�¼�������" + index.ToString() + "��������,ϵͳ��ȡ���¼�����!\r\n";
						//MessageBox.Show("�¼�������" + index.ToString() + "��������,ϵͳ��ȡ���¼�����!");
						BroadCastEvent -= tempEvent;
					}
					index++;
				}				
			}
			else
			{
			    errText += "�¼�δ�����Ļ��ķ�������!";
                //MessageBox.Show("�¼�δ�����Ļ��ķ�������!");
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
