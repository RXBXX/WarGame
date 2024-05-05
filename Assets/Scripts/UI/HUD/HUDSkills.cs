using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class HUDSkills : HUD
    {
        public HUDSkills(GComponent gCom, string customName, object[] args = null) : base(gCom, customName, args)
        {
            _gCom.GetChild("skill 1").onClick.Add(ClickSkill);
            _gCom.GetChild("skill 2").onClick.Add(ClickSkill);
        }

        private void ClickSkill()
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.HUDInstruct_Attack_Event);
        }
    }
}
