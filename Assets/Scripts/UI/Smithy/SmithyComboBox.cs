using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using DG.Tweening;

namespace WarGame.UI
{
    public class SmithyComboBox : UIBase
    {
        private GLoader _icon;
        private GList _typeList;
        private List<int> _typesData = new List<int>();
        private Dictionary<int, bool> _selectedTypes = new Dictionary<int, bool>();
        private int _selectedType = 0;
        private Transition _fadeIn;
        private bool _opened = false;

        public SmithyComboBox(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _icon = GetGObjectChild<GLoader>("icon");
            _icon.onClick.Add(OnClick);

            _typeList = GetGObjectChild<GList>("list");
            _typeList.itemRenderer = OnTypeRenderer;
            _typeList.onClickItem.Add(OnTypeClick);

            _typesData.Add(0);
            ConfigMgr.Instance.ForeachConfig<EquipmentTypeConfig>("EquipmentTypeConfig", (config) =>
            {
                _typesData.Add(config.ID);
                _selectedTypes.Add(config.ID, true);
            });
            _typeList.numItems = _typesData.Count;
            _typeList.ResizeToFit();
            _typeList.selectedIndex = 0;

            _fadeIn = GetTransition("fadeIn");

            _icon.url = "ui://Smithy/Equip_All";
        }

        private void OnTypeRenderer(int index, GObject item)
        {
            var btn = ((GButton)item);
            var typeID = _typesData[index];
            if (typeID > 0)
                btn.icon = ConfigMgr.Instance.GetConfig<EquipmentTypeConfig>("EquipmentTypeConfig", _typesData[index]).Icon;
            else
                btn.icon = "ui://Smithy/Equip_All";
        }

        private void OnTypeClick(EventContext context)
        {
            var item = (GButton)context.data;
            var index = _typeList.GetChildIndex(item);
            SelectType(_typesData[index]);
        }

        private void SelectType(int type)
        {
            if (_selectedType == type)
                return;
            _selectedType = type;
            OnTypesChange();
        }

        private void OnTypesChange()
        {
            if (_selectedType > 0)
                _icon.url = ConfigMgr.Instance.GetConfig<EquipmentTypeConfig>("EquipmentTypeConfig", _selectedType).Icon;
            else
                _icon.url = "ui://Smithy/Equip_All";

            _opened = false;
            _fadeIn.PlayReverse();
            //_typeList.visible = false;

            EventDispatcher.Instance.PostEvent(Enum.Event.EquipTypeChange, new object[] { _selectedType });
        }

        private void OnClick()
        {
            _opened = !_opened;

            if (_opened)
                _fadeIn.Play();
            else
                _fadeIn.PlayReverse();
        }

        public override void Dispose(bool disposeGCom = false)
        {

        }
    }
}
