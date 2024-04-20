using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame
{
    public delegate void WGEventCallback(params object[] args);

    public class EventDispatcher : Singeton<EventDispatcher>
    {
        private Dictionary<string, List<WGEventCallback>> _eventDic = new Dictionary<string, List<WGEventCallback>>();

        public override bool Init()
        {
            base.Init();
            return true;
        }

        public override bool Dispose()
        {
            base.Init();

            foreach (var list in _eventDic)
            {
                list.Value.Clear();
            }
            _eventDic.Clear();
            return true;
        }

        public void AddListener(string eventName, WGEventCallback callback)
        { 
            if(!_eventDic.ContainsKey(eventName))
            {
                _eventDic.Add(eventName, new List<WGEventCallback>());
            }
            _eventDic[eventName].Add(callback);
        }

        public void RemoveListener(string eventName, WGEventCallback callback)
        {
            if (!_eventDic.ContainsKey(eventName))
                return;

            for (int i = _eventDic[eventName].Count - 1; i >= 0; i--)
            {
                if (_eventDic[eventName][i] == callback)
                {
                    _eventDic[eventName].Remove(callback);
                    break;
                }
            }
        }

        public void Dispatch(string eventName)
        {
            if (!_eventDic.ContainsKey(eventName))
                return;

            for (int i = _eventDic[eventName].Count - 1; i >= 0; i--)
            {
                if (null != _eventDic[eventName][i])
                {
                    _eventDic[eventName][i]();
                }
            }
        }
    }
}
