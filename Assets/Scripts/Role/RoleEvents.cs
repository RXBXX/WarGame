using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class RoleEvents : MonoBehaviour
    {
        public void PlayEvent(string eventName)
        {
            //Debug.Log("PlayEvent");
            //var id = GetComponent<RoleData>().ID;
            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Event, new object[] { eventName, GetComponent<RoleBehaviour>().ID });
        }
    }
}