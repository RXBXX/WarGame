using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class HeroTalentItem : UIBase
    {
        private int _id;
        private bool _isActive;

        public HeroTalentItem(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            DebugManager.Instance.Log("ActivieTalent");
            _gCom.onClick.Add(OnClick);
        }

        public void UpdateItem(int id, bool isActive)
        {
            _id = id;
            _isActive = isActive;
            _gCom.grayed = !isActive;
        }


        private void OnClick()
        {
            if (_isActive)
                return;

            _isActive = true;
            _gCom.grayed = false;
            EventDispatcher.Instance.PostEvent(Enum.EventType.Hero_Talent_Active, new object[] { _id });
        }
    }
}
