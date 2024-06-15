using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class EventMgr : Singeton<EventMgr>
    {
        public void TriggerEvent(int id, WGArgsCallback callback = null)
        {
            DebugManager.Instance.Log("TriggerEvent:" + id);
            var eventConfig = ConfigMgr.Instance.GetConfig<EventConfig>("EventConfig", id);
            switch (eventConfig.Type)
            {
                case Enum.EventType.Dialog:
                    DialogMgr.Instance.OpenDialog(eventConfig.Value, callback);
                    break;
                case Enum.EventType.Level:
                    DatasMgr.Instance.ActiveLevelC2S(eventConfig.Value);
                    if (null != callback)
                        callback();
                    break;
                case Enum.EventType.Source:
                    //DatasMgr.Instance.AddItems();
                    if (null != callback)
                        callback();
                    break;
            }
        }
    }
}
