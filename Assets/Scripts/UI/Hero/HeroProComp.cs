using FairyGUI;

namespace WarGame.UI
{
    public class HeroProComp : UIBase
    {
        private GTextField _nameText;
        private GTextField _descText;
        private GButton _equipBtn, _skillBtn;

        public HeroProComp(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _nameText = (GTextField)_gCom.GetChild("name");
            _descText = (GTextField)_gCom.GetChild("desc");

            _equipBtn = (GButton)_gCom.GetChild("equipBtn");
            _skillBtn = (GButton)_gCom.GetChild("skillBtn");
            _equipBtn.onClick.Add(OnClickEquip);
            _skillBtn.onClick.Add(OnClickSkill);
        }

        public void UpdateComp(int UID)
        {
            var role = DatasMgr.Instance.GetRoleData(UID);
;            _nameText.text = role.GetConfig().Name;
            var attrs = "";
            ConfigMgr.Instance.ForeachConfig<AttrConfig>("AttrConfig", (config) =>
            {
                attrs += ((AttrConfig)config).Name + ":" + role.GetAttribute((Enum.AttrType)config.ID) +"\n";
            });
            _descText.text = attrs;
        }

        private void OnClickEquip()
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.Hero_Open_Equip);
        }

        private void OnClickSkill()
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.Hero_Open_Skill);
        }
    }
}

