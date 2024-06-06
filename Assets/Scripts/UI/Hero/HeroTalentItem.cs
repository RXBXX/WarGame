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
            var pos = GRoot.inst.GlobalToLocal(context.inputEvent.position);
            EventDispatcher.Instance.PostEvent(Enum.EventType.Hero_Show_Talent, new object[] {_heroUID, _id, pos });
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
