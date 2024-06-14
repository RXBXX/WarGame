using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class HeroProComp : UIBase
    {
        private int _heroUID;
        private GList _attrList;
        private HeroTalentComp _talentComp;
        private GList _skillList;
        private HeroEquipComp _equipComp;
        private Dictionary<string, CommonSkillItem> _skillItemsDic = new Dictionary<string, CommonSkillItem>();
        private Dictionary<string, CommonAttrItem> _attrItemsDic = new Dictionary<string, CommonAttrItem>();
        private List<AttrStruct> _attrsData = new List<AttrStruct>();
        private List<int> _skillsData = new List<int>();

        public HeroProComp(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            GetChild<HeroTitleItem>("talentTitle").Init("Talents", 0, OnClickTalent);
            GetChild<HeroTitleItem>("skillTitle").Init("Skills", 0, OnClickSkill);
            GetChild<HeroTitleItem>("attrTitle").Init("Attrs", 1, OnClickAttr);
            GetChild<HeroTitleItem>("equipTitle").Init("Equips", 0, OnClickEquip);

            _attrList = GetGObjectChild<GList>("attrList");
            _attrList.itemRenderer = OnAttrRenderer;
            _attrList.visible = true;

            _talentComp = GetChild<HeroTalentComp>("talentComp");
            _talentComp.SetVisible(false);

            _skillList = GetGObjectChild<GList>("skillList");
            _skillList.itemRenderer = OnSkillRenderer;
            _skillList.visible = false;

            _equipComp = GetChild<HeroEquipComp>("equipComp");
            _equipComp.SetVisible(false);

            EventDispatcher.Instance.AddListener(Enum.EventType.HeroTalentActiveS2C, OnHeroTalentActiveS2C);
        }

        public void UpdateComp(int UID)
        {
            _heroUID = UID;

            _attrsData.Clear();
            _skillsData.Clear();

            UpdateAttrList();

            var role = DatasMgr.Instance.GetRoleData(UID);
            _skillsData.Add(role.GetConfig().CommonSkill);
            _skillsData.Add(role.GetConfig().SpecialSkill);
            _skillList.numItems = _skillsData.Count;
            _skillList.ResizeToFit();

            _equipComp.UpdateComp(UID);

            _talentComp.UpdateComp(UID, role.GetConfig().TalentGroup, role.talentDic);
        }

        private void UpdateAttrList()
        {
            _attrsData.Clear();
            var role = DatasMgr.Instance.GetRoleData(_heroUID);
            ConfigMgr.Instance.ForeachConfig<AttrConfig>("AttrConfig", (config) =>
            {
                var value = role.GetAttribute((Enum.AttrType)config.ID);
                if (value != 0)
                {
                    _attrsData.Add(new AttrStruct(config.Name, value.ToString()));
                }
            });
            _attrList.numItems = _attrsData.Count;
            _attrList.ResizeToFit();
        }

        private void OnClickTalent(params object[] args)
        {
            _talentComp.SetVisible((bool)args[0]);
        }

        private void OnClickSkill(params object[] args)
        {
            _skillList.visible = (bool)args[0];
        }

        private void OnClickAttr(params object[] args)
        {
            _attrList.visible = (bool)args[0];
        }

        private void OnClickEquip(params object[] args)
        {
            _equipComp.SetVisible((bool)args[0]);
        }

        private void OnSkillRenderer(int index, GObject item)
        {
            if (!_skillItemsDic.ContainsKey(item.id))
            {
                _skillItemsDic[item.id] = new CommonSkillItem((GComponent)item);
            }
            _skillItemsDic[item.id].Update(_skillsData[index]);
        }

        private void OnAttrRenderer(int index, GObject item)
        {
            if (!_attrItemsDic.ContainsKey(item.id))
            {
                _attrItemsDic[item.id] = new CommonAttrItem((GComponent)item);
            }
            _attrItemsDic[item.id].Update(_attrsData[index].name, _attrsData[index].desc);
        }


        private void OnHeroTalentActiveS2C(params object[] args)
        {
            if (_heroUID != (int)args[0])
                return;
            _talentComp.ActiveTalent((int)args[1]);

            UpdateAttrList();
        }

        public override void Dispose(bool disposeGCom = false)
        {
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HeroTalentActiveS2C, OnHeroTalentActiveS2C);

            foreach (var v in _attrItemsDic)
                v.Value.Dispose();
            _attrItemsDic.Clear();

            foreach (var v in _skillItemsDic)
                v.Value.Dispose();
            _skillItemsDic.Clear();

            base.Dispose(disposeGCom);
        }
    }
}

