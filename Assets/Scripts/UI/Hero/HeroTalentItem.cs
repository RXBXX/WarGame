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
        private GLoader _icon;

        public HeroTalentItem(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _icon = GetGObjectChild<GLoader>("icon");
            _active = GetTransition("active");
            _gCom.onClick.Add(OnClick);
        }

        public void UpdateItem(int heroUID, int id, string icon)
        {
            _heroUID = heroUID;
            _id = id;
            _isActive = DatasMgr.Instance.IsHeroTalentActive(heroUID, id);
            _gCom.grayed = !_isActive;
            _icon.url = icon;
        }


        private void OnClick(EventContext context)
        {
            var pos = GRoot.inst.GlobalToLocal(context.inputEvent.position);
            EventDispatcher.Instance.PostEvent(Enum.Event.Hero_Show_Talent, new object[] {_heroUID, _id, pos });
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
