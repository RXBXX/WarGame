using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;

namespace WarGame
{
    public class EventMgr : Singeton<EventMgr>
    {
        public void TriggerEvent(int id, WGArgsCallback callback = null)
        {
            if (id == 0)
            {
                if (null != callback)
                    callback();
                return;
            }
            var eventConfig = ConfigMgr.Instance.GetConfig<EventConfig>("EventConfig", id);
            switch (eventConfig.Type)
            {
                case Enum.EventType.Dialog:
                    DialogMgr.Instance.OpenDialog(eventConfig.Value, (args) =>
                    {
                        OnNextEvent(eventConfig.NextEvents, 0, callback);
                        //TriggerEvent(eventConfig.NextEvent, callback);
                    });
                    break;
                case Enum.EventType.Level:
                    DatasMgr.Instance.ActiveLevelC2S(eventConfig.Value);
                    //TriggerEvent(eventConfig.NextEvent, callback);
                    OnNextEvent(eventConfig.NextEvents, 0, callback);
                    break;
                case Enum.EventType.Hero:
                    var sp = new SourcePair();
                    sp.Type = Enum.SourceType.Hero;
                    sp.id = eventConfig.Value;
                    DatasMgr.Instance.AddItem(sp);
                    WGArgsCallback cb = (args) =>
                    {
                        OnNextEvent(eventConfig.NextEvents, 0, callback);
                        //TriggerEvent(eventConfig.NextEvent, callback);
                    };
                    UIManager.Instance.OpenPanel("Hero", "HeroShowPanel", new object[] { eventConfig.Value, cb });
                    break;
                case Enum.EventType.Story:
                    StoryMgr.Instance.PlayStory(eventConfig.Value, true, (args) =>
                    {
                        OnNextEvent(eventConfig.NextEvents, 0, callback);
                        //TriggerEvent(eventConfig.NextEvent, callback);
                    });
                    break;
                case Enum.EventType.HomeEvent:
                    DatasMgr.Instance.SetHomeEvent(eventConfig.Value);
                    OnNextEvent(eventConfig.NextEvents, 0, callback);
                    break;
            }
        }

        private void OnNextEvent(List<int> events, int index, WGArgsCallback callback)
        {
            if (null == events || events.Count <= 0 || index >= events.Count)
            {
                if (null != callback)
                    callback();
                return;
            }

            TriggerEvent(events[index], (args)=> {
                OnNextEvent(events, index + 1, callback);
            });
        }
    }
}
