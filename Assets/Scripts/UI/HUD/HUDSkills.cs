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
        private Transition _show;

        public HUDSkills(GComponent gCom, string customName, object[] args = null) : base(gCom, customName, args)
        {
            _skill1 = GetGObjectChild<GButton>("skill 1");
            _skill1.onClick.Add(()=> { ClickSkill(_commonSkill); });

            _skill2 = GetGObjectChild<GButton>("skill 2");
            _skill2.onClick.Add(() => { ClickSkill(_specialSkill); });

            _show = GetTransition("show");
        }

        public void UpdateComp(int commonSkill, int specialSkill, bool rageFilled)
        {
            _commonSkill = commonSkill;
            _specialSkill = specialSkill;

            _skill1.title = ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", _commonSkill).GetTranslation("Name");
            if (0 == _specialSkill)
            {
                _skill2.visible = false;
            }
            else
            {
                _skill2.visible = true;
                _skill2.title = ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", _specialSkill).GetTranslation("Name");
            }

            _skill2.grayed = !rageFilled;
            _skill2.touchable = rageFilled;
        }

        public void Show()
        {
            _show.Play();
        }

        private void ClickSkill(int skillId)
        {
            EventDispatcher.Instance.PostEvent(Enum.Event.HUDInstruct_Click_Skill, new object[] { skillId});
        }
    }
}
