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

        public HeroTalentInfo(GComponent gCom, string customName = null, object[] args = null) : base(gCom, customName, args)
        {
            _title = GetGObjectChild<GTextField>("title");
            _desc = GetGObjectChild<GTextField>("desc");
            _attrList = GetGObjectChild<GList>("attrList");
            _attrList.itemRenderer = OnAttrRenderer;

            EventDispatcher.Instance.AddListener(Enum.EventType.Hero_Show_Talent, OnShowTalent);

            Stage.inst.onTouchBegin.Add(OnTouchBegin);
            GetGObjectChild<GButton>("btn").onClick.Add(OnClick);
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

            _title.text = talentConfig.Name;
            _desc.text = talentConfig.Desc;

            _attrsData.Clear();
            foreach (var v in talentConfig.Attrs)
            {
                _attrsData.Add(new AttrStruct(ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", v.id).Name, v.value.ToString()));
            }
            _attrList.numItems = _attrsData.Count;
            _attrList.ResizeToFit();

            SetPosition((Vector2)args[2]);
            SetVisible(true);
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
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Hero_Show_Talent, OnShowTalent);
            Stage.inst.onTouchBegin.Remove(OnTouchBegin);
            foreach (var v in _attrItemsDic)
                v.Value.Dispose();
            _attrItemsDic.Clear();
            base.Dispose(disposeGCom);
        }
    }
}
