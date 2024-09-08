using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class HeroLevelItem : UIBase
    {
        private GTextField _name;
        private Controller _type;
        private GButton _levelUPBtn;
        private int _roleUID;
        private int _level;

        public HeroLevelItem(GComponent gCom, string customName = null, object[] args = null) : base(gCom, customName, args)
        {
            _name = GetGObjectChild<GTextField>("name");
            _type = GetController("type");
            _levelUPBtn = GetGObjectChild<GButton>("levelUpBtn");

            _levelUPBtn.onClick.Add(OnClickLevelUp);
            _levelUPBtn.title = ConfigMgr.Instance.GetTranslation("HeroPanel_LevelUP");
        }

        public void UpdateItem(int roleUID, int index, int level)
        {
            _roleUID = roleUID;
            _level = index;

            _name.text = "Lv." + index;
            if (index < level)
            {
                _type.SetSelectedIndex(0);
            }
            else if (index == level)
            {
                var cost = DatasMgr.Instance.GetRoleData(roleUID).GetStarConfig().Cost;
                var own = DatasMgr.Instance.GetItem((int)Enum.ItemType.LevelRes);
                if (own >= cost)
                    _type.SetSelectedIndex(1);
                else
                    _type.SetSelectedIndex(2);
            }
            else
            {
                _type.SetSelectedIndex(3);
            }
        }

        private void OnClickLevelUp(EventContext context)
        {
            var pos = context.inputEvent.position;
            pos = GRoot.inst.GlobalToLocal(pos);

            var starConfig = DatasMgr.Instance.GetRoleData(_roleUID).GetStarConfig();
            var attrs = starConfig.Attrs;
            List<AttrStruct> attrsData = new List<AttrStruct>();
            foreach (var v in attrs)
            {
                attrsData.Add(new AttrStruct(ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", v.id).GetTranslation("Name"), v.value.ToString()));
            }

            var resCount = DatasMgr.Instance.GetItem((int)Enum.ItemType.LevelRes);
            var resTxt = "";
            if (resCount >= starConfig.Cost)
                resTxt = "[color=#00a8ed]" + string.Format("{0}/{1}", resCount, starConfig.Cost) + "[/color]";
            else
                resTxt = "[color=#ce4a35]" + string.Format("{0}/{1}", resCount, starConfig.Cost) + "[/color]";

            WGCallback callback = OnLevelUp;
            var args = new object[] {
                "Lv."+_level,
                "",
                attrsData,
                pos,
                true,
                ConfigMgr.Instance.GetTranslation("HeroPanel_LevelUP"),
                resTxt,
                true,
                callback
            };

            EventDispatcher.Instance.PostEvent(Enum.Event.Hero_Show_Talent, args);
            //DatasMgr.Instance.HeroLevelUpC2S(_roleUID, _level);
        }

        private void OnLevelUp()
        {
            var starConfig = DatasMgr.Instance.GetRoleData(_roleUID).GetStarConfig();
            var resCount = DatasMgr.Instance.GetItem((int)Enum.ItemType.LevelRes);
            if (resCount < starConfig.Cost)
            {
                TipsMgr.Instance.Add("µÀ¾ß²»×ã£¡");
                return;
            }

            DatasMgr.Instance.HeroLevelUpC2S(_roleUID, _level);
        }
    }
}
