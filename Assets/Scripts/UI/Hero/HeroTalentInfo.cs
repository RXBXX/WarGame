using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class HeroTalentInfo : UIBase
    {
        private int _heroUID = 0;
        private int _talentID = 0;
        private GTextField _title;
        private GTextField _desc;
        private GList _attrList;
        private Dictionary<string, CommonAttrItem> _attrItemsDic = new Dictionary<string, CommonAttrItem>();
        private List<AttrStruct> _attrsData = new List<AttrStruct>();
        private GButton _activeBtn;
        private Controller _type;
        private GTextField _costTxt;

        public HeroTalentInfo(GComponent gCom, string customName = null, object[] args = null) : base(gCom, customName, args)
        {
            _title = GetGObjectChild<GTextField>("title");
            _desc = GetGObjectChild<GTextField>("desc");
            _attrList = GetGObjectChild<GList>("attrList");
            _attrList.itemRenderer = OnAttrRenderer;

            EventDispatcher.Instance.AddListener(Enum.Event.Hero_Show_Talent, OnShowTalent);

            Stage.inst.onTouchBegin.Add(OnTouchBegin);
            _activeBtn = GetGObjectChild<GButton>("btn");
            _activeBtn.onClick.Add(OnClick);

            _type = GetController("type");
            _costTxt = GetGObjectChild<GTextField>("cost");
        }

        private void OnAttrRenderer(int index, GObject item)
        {
            if (!_attrItemsDic.ContainsKey(item.id))
            {
                _attrItemsDic[item.id] = new CommonAttrItem((GComponent)item);
            }
            _attrItemsDic[item.id].Update(_attrsData[index].name, _attrsData[index].desc);
        }

        private void OnShowTalent(params object[] args)
        {
            _heroUID = (int)args[0];
            _talentID = (int)args[1];

            var talentConfig = ConfigMgr.Instance.GetConfig<TalentConfig>("TalentConfig", _talentID);

            _title.text = talentConfig.GetTranslation("Name");
            _desc.text = talentConfig.GetTranslation("Desc");

            _attrsData.Clear();
            foreach (var v in talentConfig.Attrs)
            {
                _attrsData.Add(new AttrStruct(ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", v.id).GetTranslation("Name"), v.value.ToString()));
            }
            _attrList.numItems = _attrsData.Count;
            _attrList.ResizeToFit();

            SetPosition((Vector2)args[2]);
            SetVisible(true);

            var resCount = DatasMgr.Instance.GetItem((int)Enum.ItemType.TalentRes);
            if (DatasMgr.Instance.IsHeroTalentActive(_heroUID, _talentID))
                _type.SetSelectedIndex(0);
            else
            {
                if (!DatasMgr.Instance.CanHeroTalentActive(_heroUID, _talentID))
                    _type.SetSelectedIndex(3);
                else
                {
                    if (resCount >= talentConfig.Cost)
                        _type.SetSelectedIndex(1);
                    else
                        _type.SetSelectedIndex(2);
                }
            }
            _costTxt.text = string.Format("{0}/{1}", resCount, talentConfig.Cost);
        }

        private void OnClick()
        {
            SetVisible(false);
            DatasMgr.Instance.HeroTalentActiveC2S(_heroUID, _talentID);
        }

        private void OnTouchBegin()
        {
            var isTouch = false;
            var touchTarget = Stage.inst.touchTarget;
            while (null != touchTarget)
            {
                if (touchTarget == GCom.displayObject)
                {
                    isTouch = true;
                    break;
                }
                touchTarget = touchTarget.parent;
            }
            if (!isTouch)
            {
                SetVisible(false);
            }
        }

        public override void Dispose(bool disposeGCom = false)
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.Hero_Show_Talent, OnShowTalent);
            Stage.inst.onTouchBegin.Remove(OnTouchBegin);
            foreach (var v in _attrItemsDic)
                v.Value.Dispose();
            _attrItemsDic.Clear();
            base.Dispose(disposeGCom);
        }
    }
}
