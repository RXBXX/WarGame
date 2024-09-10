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
        private GTextField _title;
        private GTextField _level;

        public HeroTalentItem(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _icon = GetGObjectChild<GLoader>("icon");
            _active = GetTransition("active");
            _title = GetGObjectChild<GTextField>("title");
            _level = GetGObjectChild<GTextField>("level");
            _gCom.onClick.Add(OnClick);
        }

        public void UpdateItem(int heroUID, int id)
        {
            _heroUID = heroUID;
            _id = id;
            _isActive = DatasMgr.Instance.IsHeroTalentActive(heroUID, id);
            _gCom.grayed = !_isActive;
            var talentConfig = ConfigMgr.Instance.GetConfig<TalentConfig>("TalentConfig", id);
            _title.text = talentConfig.GetTranslation("Name");
            _level.text = "Lv." + talentConfig.Level;
            //_icon.url = icon;
        }


        private void OnClick(EventContext context)
        {
            var pos = GRoot.inst.GlobalToLocal(context.inputEvent.position);
            var talentConfig = ConfigMgr.Instance.GetConfig<TalentConfig>("TalentConfig", _id);
            List<AttrStruct> _attrsData = new List<AttrStruct>();
            foreach (var v in talentConfig.Attrs)
            {
                _attrsData.Add(new AttrStruct(ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", v.id).GetTranslation("Name"), v.value.ToString()));
            }


            var btnVisible = false;
            var resCount = DatasMgr.Instance.GetItem((int)Enum.ItemType.TalentRes);
            if (!DatasMgr.Instance.IsHeroTalentActive(_heroUID, _id))
            {
                if (DatasMgr.Instance.CanHeroTalentActive(_heroUID, _id))
                {
                    if (resCount >= talentConfig.Cost)
                        btnVisible = true;
                }
            }

            var resTxt = "";
            if (resCount >= talentConfig.Cost)
                resTxt = "[color=#00a8ed]" + string.Format("{0}/{1}", resCount, talentConfig.Cost) + "[/color]";
            else
                resTxt = "[color=#ce4a35]" + string.Format("{0}/{1}", resCount, talentConfig.Cost) + "[/color]";

            WGCallback callback = OnActiveCallback;
            var args = new object[] {
                talentConfig.GetTranslation("Name"),
                "",//talentConfig.GetTranslation("Desc"),
                _attrsData,
                pos,
                btnVisible,
                ConfigMgr.Instance.GetTranslation("HeroPanel_ActiveTalent"),
                resTxt,
                true,
                callback
            };
            EventDispatcher.Instance.PostEvent(Enum.Event.Hero_Show_Talent, args);
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

        private void OnActiveCallback()
        {
            var talentConfig = ConfigMgr.Instance.GetConfig<TalentConfig>("TalentConfig", _id);
            var resCount = DatasMgr.Instance.GetItem((int)Enum.ItemType.TalentRes);
            if (resCount < talentConfig.Cost)
            {
                TipsMgr.Instance.Add("µÀ¾ß²»×ã£¡");
                return;
            }

            DatasMgr.Instance.HeroTalentActiveC2S(_heroUID, _id);
        }
    }
}
