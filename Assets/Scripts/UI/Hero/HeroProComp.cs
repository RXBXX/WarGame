using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class HeroProComp : UIBase
    {
        private int _heroUID;
        private HeroTalentComp _talentComp;
        private GList _skillList;
        private HeroEquipComp _equipComp;
        private HeroAttrComp _attrComp;
        private Dictionary<string, CommonSkillItem> _skillItemsDic = new Dictionary<string, CommonSkillItem>();
        private List<int> _skillsData = new List<int>();
        private Controller _type;
        private HeroLevelComp _levelComp;

        public HeroProComp(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _type = GetController("type");
            GetChild<HeroTitleItem>("levelTitle").Init(ConfigMgr.Instance.GetTranslation("HeroPanel_LevelTitle"), 0, OnClickLevel);
            GetChild<HeroTitleItem>("talentTitle").Init(ConfigMgr.Instance.GetTranslation("HeroPanel_TalentTitle"), 0, OnClickTalent);
            GetChild<HeroTitleItem>("skillTitle").Init(ConfigMgr.Instance.GetTranslation("HeroPanel_SkillTitle"), 0, OnClickSkill);
            GetChild<HeroTitleItem>("attrTitle").Init(ConfigMgr.Instance.GetTranslation("HeroPanel_AttrTitle"), 1, OnClickAttr);
            GetChild<HeroTitleItem>("equipTitle").Init(ConfigMgr.Instance.GetTranslation("HeroPanel_EquipTitle"), 0, OnClickEquip);

            _talentComp = GetChild<HeroTalentComp>("talentComp");

            _skillList = GetGObjectChild<GList>("skillList");
            _skillList.itemRenderer = OnSkillRenderer;

            _equipComp = GetChild<HeroEquipComp>("equipComp");

            _levelComp = GetChild<HeroLevelComp>("levelComp");

            _attrComp = GetChild<HeroAttrComp>("attrComp");

            EventDispatcher.Instance.AddListener(Enum.Event.HeroTalentActiveS2C, OnHeroTalentActiveS2C);
            EventDispatcher.Instance.AddListener(Enum.Event.HeroLevelUpS2C, OnHeroLevelUpS2C);
        }

        public void UpdateComp(int UID)
        {
            _heroUID = UID;

            _skillsData.Clear();

            _attrComp.UpdateComp(UID);
            var role = DatasMgr.Instance.GetRoleData(UID);
            _skillsData.Add(role.GetConfig().CommonSkill);
            _skillsData.Add(role.GetConfig().SpecialSkill);
            _skillList.numItems = _skillsData.Count;
            _skillList.ResizeToFit();

            _levelComp.UpdateComp(UID);
            _equipComp.UpdateComp(UID);

            _talentComp.UpdateComp(UID);
        }

        private void OnClickLevel(params object[] args)
        {
            _type.SetSelectedIndex((bool)args[0] ? 1 : 0);
        }

        private void OnClickTalent(params object[] args)
        {
            _type.SetSelectedIndex((bool)args[0] ? 2 : 0);
        }

        private void OnClickSkill(params object[] args)
        {
            _skillList.visible = (bool)args[0];
        }

        private void OnClickAttr(params object[] args)
        {
            _attrComp.SetVisible((bool)args[0]);
        }

        private void OnClickEquip(params object[] args)
        {
            _type.SetSelectedIndex((bool)args[0] ? 3 : 0);
        }

        private void OnSkillRenderer(int index, GObject item)
        {
            if (!_skillItemsDic.ContainsKey(item.id))
            {
                _skillItemsDic[item.id] = new CommonSkillItem((GComponent)item);
            }
            _skillItemsDic[item.id].Update(_skillsData[index]);
        }

        private void OnHeroTalentActiveS2C(params object[] args)
        {
            if (_heroUID != (int)args[0])
                return;
            _talentComp.ActiveTalent((int)args[1]);

            _attrComp.UpdateComp(_heroUID);
        }

        private void OnHeroLevelUpS2C(params object[] args)
        {
            if (_heroUID != (int)args[0])
                return;
            _levelComp.LevelUp((int)args[1]);
        }

        public override void Dispose(bool disposeGCom = false)
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.HeroTalentActiveS2C, OnHeroTalentActiveS2C);
            EventDispatcher.Instance.RemoveListener(Enum.Event.HeroLevelUpS2C, OnHeroLevelUpS2C);

            _attrComp.Dispose();

            foreach (var v in _skillItemsDic)
                v.Value.Dispose();
            _skillItemsDic.Clear();

            base.Dispose(disposeGCom);
        }
    }
}

