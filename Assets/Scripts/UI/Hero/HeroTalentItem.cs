using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class HeroTalentItem : UIBase
    {
        private int _heroUID;
        private int _id;
        private bool _isActive;
        private Transition _active;

        public HeroTalentItem(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _active = GetTransition("active");
            _gCom.onClick.Add(OnClick);
        }

        public void UpdateItem(int heroUID, int id, bool isActive)
        {
            _heroUID = heroUID;
            _id = id;
            _isActive = isActive;
            _gCom.grayed = !isActive;
        }


        private void OnClick(EventContext context)
        {
            var pos = context.inputEvent.position;
            //pos.y = Screen.height - pos.y;
            pos = GRoot.inst.GlobalToLocal(pos);
            DebugManager.Instance.Log("ONClick");
            EventDispatcher.Instance.PostEvent(Enum.EventType.Hero_Show_Talent, new object[] {_heroUID, _id, pos });
            //if (_isActive)
            //    return;

            //_isActive = true;
            //_gCom.grayed = false;
            //EventDispatcher.Instance.PostEvent(Enum.EventType.Hero_Talent_Active, new object[] { _id });
        }

        public int GetID()
        {
            return _id;
        }

        public void Active()
        {
            _isActive = true;
            _gCom.grayed = !_isActive;
            _active.Play();
        }
    }
}
