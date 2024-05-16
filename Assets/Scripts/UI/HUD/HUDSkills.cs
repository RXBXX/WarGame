using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class HUDSkills : HUD
    {
        private GButton _skill1;
        private GButton _skill2;

        public HUDSkills(GComponent gCom, string customName, object[] args = null) : base(gCom, customName, args)
        {
            _skill1 = GetUIChild<GButton>("skill 1");
            _skill1.onClick.Add(()=> { ClickSkill(Enum.SkillType.CommonAttack); });

            _skill2 = GetUIChild<GButton>("skill 2");
            _skill2.onClick.Add(() => { ClickSkill(Enum.SkillType.Special); });
        }

        public void UpdateComp(object[] args)
        {
            _skill1.title = (string)args[0];
            _skill2.title = (string)args[1];
        }

        private void ClickSkill(Enum.SkillType type)
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.HUDInstruct_Click_Skill, new object[] { type});
        }
    }
}
