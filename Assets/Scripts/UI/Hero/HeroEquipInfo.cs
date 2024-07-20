using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class HeroEquipInfo : UIBase
    {
        private int _UID;
        private int _owner;
        private int _selectedRoleUID;
        private GList _attrList;
        private Dictionary<string, CommonAttrItem> _attrItemsDic = new Dictionary<string, CommonAttrItem>();
        private List<AttrStruct> _attrsData = new List<AttrStruct>();
        private GTextField _title;
        private GTextField _desc;
        private GTextField _ownerTxt;
        private GButton _activeBtn;

        public HeroEquipInfo(GComponent gCom, string customName = null, object[] args = null) : base(gCom, customName, args)
        {
            _title = GetGObjectChild<GTextField>("title");
            _desc = GetGObjectChild<GTextField>("desc");
            _ownerTxt = GetGObjectChild<GTextField>("owner");
            _attrList = GetGObjectChild<GList>("attrList");
            _attrList.itemRenderer = OnAttrRenderer;
            _activeBtn = GetGObjectChild<GButton>("btn");

            EventDispatcher.Instance.AddListener(Enum.Event.Hero_Show_Attrs, OnShowAttrs);

            Stage.inst.onTouchBegin.Add(OnTouchBegin);
            _activeBtn.onClick.Add(OnClick);
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
            _UID = (int)args[0];
            _owner = (int)args[1];
            _selectedRoleUID = (int)args[2];

            _attrsData.Clear();
            var equipData = DatasMgr.Instance.GetEquipmentData(_UID);
            _title.text = equipData.GetName();
            _desc.text = equipData.GetConfig().GetTranslation("Desc");

            foreach (var v in equipData.GetAttrs())
            {
                _attrsData.Add(new AttrStruct(ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", v.id).GetTranslation("Name"), v.value.ToString()));
            }
            _attrList.numItems = _attrsData.Count;

            if (_selectedRoleUID == _owner)
            {
                _activeBtn.title = "Ð¶ÏÂ";
            }
            else
            {
                _activeBtn.title = "´©´÷";
            }

            SetPosition((Vector2)args[3]);
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

        private void OnClick()
        {
            SetVisible(false);
            if (_owner == _selectedRoleUID)
                DatasMgr.Instance.UnwearEquipC2S(_selectedRoleUID, _UID);
            else
                DatasMgr.Instance.WearEquipC2S(_selectedRoleUID, _UID);
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
