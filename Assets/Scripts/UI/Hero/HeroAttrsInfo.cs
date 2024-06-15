using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class HeroAttrsInfo : UIBase
    {
        private GList _attrList;
        private Dictionary<string, CommonAttrItem> _attrItemsDic = new Dictionary<string, CommonAttrItem>();
        private List<AttrStruct> _attrsData = new List<AttrStruct>();

        public HeroAttrsInfo(GComponent gCom, string customName = null, object[] args = null) : base(gCom, customName, args)
        {
            _attrList = GetGObjectChild<GList>("attrList");
            _attrList.itemRenderer = OnAttrRenderer;

            EventDispatcher.Instance.AddListener(Enum.Event.Hero_Show_Attrs, OnShowAttrs);

            Stage.inst.onTouchBegin.Add(OnTouchBegin);
        }

        private void OnAttrRenderer(int index, GObject item)
        {
            if (!_attrItemsDic.ContainsKey(item.id))
            {
                _attrItemsDic[item.id] = new CommonAttrItem((GComponent)item);
            }
            _attrItemsDic[item.id].Update(_attrsData[index].name, _attrsData[index].desc);
        }

        private void OnShowAttrs(params object[] args)
        {
            _attrsData = (List<AttrStruct>)args[0];
            _attrList.numItems = _attrsData.Count;

            SetPosition((Vector2)args[1]);
            SetVisible(true);
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
            EventDispatcher.Instance.RemoveListener(Enum.Event.Hero_Show_Attrs, OnShowAttrs);
            Stage.inst.onTouchBegin.Remove(OnTouchBegin);
            foreach (var v in _attrItemsDic)
                v.Value.Dispose();
            _attrItemsDic.Clear();

            _attrsData.Clear();

            base.Dispose(disposeGCom);
        }
    }
}
