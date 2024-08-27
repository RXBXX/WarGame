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
        private WGCallback _callback;

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
                _attrItemsDic[item.id] = new CommonSmallAttrItem((GComponent)item);
            }
            _attrItemsDic[item.id].Update(_attrsData[index].name, _attrsData[index].desc);
        }

        /// <summary>
        /// title
        /// desc
        /// attrs
        /// pos
        /// btnVisible
        /// btnTitle
        /// value
        /// valueVisible
        /// btnCallback
        /// </summary>
        /// <param name="args"></param>
        private void OnShowTalent(params object[] args)
        {
            _title.text = args[0].ToString();
            _desc.text = args[1].ToString();

            _attrsData = (List<AttrStruct>)args[2];
            _attrList.numItems = _attrsData.Count;
            _attrList.ResizeToFit();

            SetPosition((Vector2)args[3]);
            SetVisible(true);

            _activeBtn.visible = (bool)args[4];
            _activeBtn.title = args[5].ToString();

            _costTxt.text = args[6].ToString();
            _costTxt.visible = (bool)args[7];

            _callback = (WGCallback)args[8];
        }

        private void OnClick()
        {
            SetVisible(false);
            if (null != _callback)
                _callback.Invoke();
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
