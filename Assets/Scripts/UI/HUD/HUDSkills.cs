using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class HUDSkills : HUD
    {
        private int _commonSkill, _specialSkill;
        private GButton _skill1;
        private GButton _skill2;

        public HUDSkills(GComponent gCom, string customName, object[] args = null) : base(gCom, customName, args)
        {
            _skill1 = GetGObjectChild<GButton>("skill 1");
            _skill1.onClick.Add(()=> { ClickSkill(_commonSkill); });

            _skill2 = GetGObjectChild<GButton>("skill 2");
            _skill2.onClick.Add(() => { ClickSkill(_specialSkill); });
        }

        public void UpdateComp(object[] args)
        {
            _commonSkill = (int)args[0];
            _specialSkill = (int)args[1];

            _skill1.title = ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", _commonSkill).Name;
            if (0 == _specialSkill)
            {
                _skill2.visible = false;
            }
            else
            {
                _skill2.visible = true;
                _skill2.title = ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", _specialSkill).Name;
            }

            var rageFilled = (bool)args[2];
            _skill2.grayed = !rageFilled;
            _skill2.touchable = rageFilled;
        }

        private void ClickSkill(int skillId)
        {
            EventDispatcher.Instance.PostEvent(Enum.Event.HUDInstruct_Click_Skill, new object[] { skillId});
        }
    }
}
