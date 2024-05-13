using System.Collections;
using System.Collections.Generic;
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

            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Event, new object[] { eventName, id});
        }
    }
}
