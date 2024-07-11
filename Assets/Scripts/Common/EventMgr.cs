using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;

namespace WarGame
{
    public class EventMgr : Singeton<EventMgr>
    {
        //private Queue<EventPair> _eventQueue = new Queue<EventPair>();

        //private class EventPair
        //{
        //    public int id;
        //    public WGArgsCallback callback;
        //    public object[] args;

        //    public EventPair(int id, WGArgsCallback callback, params object[] args)
        //    {
        //        this.id = id;
        //        this.callback = callback;
        //        this.args = args;
        //    }
        //}

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
                    });
                    break;
                case Enum.EventType.Level:
                    DatasMgr.Instance.ActiveLevelC2S(eventConfig.Value);
                    OnNextEvent(eventConfig.NextEvents, 0, callback);
                    break;
                case Enum.EventType.Hero:
                    DatasMgr.Instance.AddHero(eventConfig.Value);
                    WGArgsCallback cb = (args) =>
                    {
                        OnNextEvent(eventConfig.NextEvents, 0, callback);
                    };
                    UIManager.Instance.OpenPanel("Reward", "RewardHeroPanel", new object[] { eventConfig.Value, cb });
                    break;
                case Enum.EventType.Story:
                    StoryMgr.Instance.PlayStory(eventConfig.Value, true, (args) =>
                    {
                        OnNextEvent(eventConfig.NextEvents, 0, callback);
                    });
                    break;
                case Enum.EventType.HomeEvent:
                    DatasMgr.Instance.SetHomeEvent(eventConfig.Value);
                    OnNextEvent(eventConfig.NextEvents, 0, callback);
                    break;
                case Enum.EventType.Equip:
                    DatasMgr.Instance.AddEquip(eventConfig.Value);
                    WGArgsCallback cb1 = (args) =>
                    {
                        OnNextEvent(eventConfig.NextEvents, 0, callback);
                    };
                    UIManager.Instance.OpenPanel("Reward", "RewardEquipPanel", new object[] { eventConfig.Value, cb1 });
                    break;
                //case Enum.EventType.Items:
                //    var rewardConfig = ConfigMgr.Instance.GetConfig<RewardConfig>("RewardConfig", eventConfig.Value);
                //    DatasMgr.Instance.AddItems(rewardConfig.Rewards);
                //    WGArgsCallback cb2 = (args) =>
                //    {
                //        OnNextEvent(eventConfig.NextEvents, 0, callback);
                //    };
                //    UIManager.Instance.OpenPanel("Reward", "RewardItemsPanel", new object[] {eventConfig.Value, cb2 });
                //    break;
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
