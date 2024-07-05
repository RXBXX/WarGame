//Notes:��ʹ����SetBool(false)֮��AnimationClip�¼����ܻᴥ��
//ChatGPT������ԭ���ǣ�
//    1.���������������Ķ���״̬���еĹ�����������ʹ����������Ϊfalse�����û����ȷ���ù��������������Իᴥ�����������磬�����һ��Ĭ�Ϲ��ɣ�default transition���������ܻ���������false֮ǰ��֮�󴥷�������
//    2.������Ȩ�أ��������ʹ�ö�������㣬ȷ�����ÿ�����Ȩ�ء�һ���ϸ�Ȩ�صĲ��ϵĶ������ܻḲ����ϣ�����õĶ�����
//    3.�������Blend Tree���������Ķ���״̬����ʹ���˻����������ϲ�����Ȩ�����á�������Ĳ���Ҳ����Ӱ�춯���Ĳ��š�
//    4.�������ˣ�ȷ��������SetBool(false)ʱû�������߼��������ٴ�����Ϊtrue������û�����������͹����߼����¶����л���ԭ����״̬��
//    5.�������Ŷ��У���ʱ���������Ѿ����ŶӲ��ţ���ʹ����������Ϊfalse������Ȼ�Ქ����ϡ������������ͨ��ȷ�����ʵ���ʱ���������״̬����״̬�����⡣

using UnityEngine;

namespace WarGame
{
    public class EventBehaviour : MonoBehaviour
    {
        public void PlayEvent(string eventName)
        {
            var id = 0;
            RoleBehaviour rd;
            if (TryGetComponent(out rd))
            {
                id = rd.ID;
            }
            else
            {
                rd = GetComponentInParent<RoleBehaviour>();
                if (null != rd)
                    id = rd.ID;
            }
            //DebugManager.Instance.Log("Event:"+eventName +"��" +id);
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Event, new object[] { eventName, id});
        }
    }
}
