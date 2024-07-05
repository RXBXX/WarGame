//Notes:即使设置SetBool(false)之后，AnimationClip事件可能会触发
//ChatGPT给出的原因是：
//    1.过渡条件：检查你的动画状态机中的过渡条件。即使将参数设置为false，如果没有正确配置过渡条件，可能仍会触发动画。例如，如果有一个默认过渡（default transition），它可能会在你设置false之前或之后触发动画。
//    2.动画层权重：如果你在使用多个动画层，确保检查每个层的权重。一个较高权重的层上的动画可能会覆盖你希望禁用的动画。
//    3.混合树（Blend Tree）：如果你的动画状态机中使用了混合树，检查混合参数和权重设置。混合树的参数也可能影响动画的播放。
//    4.动画回退：确保在设置SetBool(false)时没有其他逻辑将参数再次设置为true，或者没有其他参数和过渡逻辑导致动画切换回原来的状态。
//    5.动画播放队列：有时动画可能已经被排队播放，即使参数被设置为false，它仍然会播放完毕。这种情况可以通过确保在适当的时候清除动画状态机的状态来避免。

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
            //DebugManager.Instance.Log("Event:"+eventName +"—" +id);
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Event, new object[] { eventName, id});
        }
    }
}
