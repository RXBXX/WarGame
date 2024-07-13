using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using DG.Tweening;

namespace WarGame.UI
{
    public class SmithyPanel : UIBase
    {
        private List<int> _equipsData = new List<int>();
        private GList _equipList;
        private List<IntFloatPair> _attrsData = new List<IntFloatPair>();
        private GList _attrsList;
        private Dictionary<string, CommonAttrItem> _attrsMap = new Dictionary<string, CommonAttrItem>();
        private int _selectEquip = 0;

        public SmithyPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            GetGObjectChild<GLoader>("BG").url = "UI/Background/HeroBG";
            _gCom.GetChild("closeBtn").onClick.Add(OnClickClose);

            _equipList = GetGObjectChild<GList>("equips");
            _equipList.SetVirtual();
            _equipList.itemRenderer = OnEquipRenderer;
            _equipList.onClickItem.Add(OnClickEquip);

            _attrsList = GetGObjectChild<GList>("attrs");
            _attrsList.itemRenderer = OnAttrRenderer;

            GetGObjectChild<GButton>("buyBtn").onClick.Add(OnClickBuy);

            EventDispatcher.Instance.AddListener(Enum.Event.BuyEquipS2C, OnBuyEquipS2C);

            ConfigMgr.Instance.ForeachConfig<EquipmentConfig>("EquipmentConfig", (config) =>
            {
                _equipsData.Add(config.ID);
            });
            _equipList.numItems = _equipsData.Count;

            SelectEquip(_equipsData[0]);
        }

        private void SelectEquip(int id)
        {
            _selectEquip = id;

            _attrsData.Clear();

            var attrsDic = new Dictionary<int, float>();
            var attrs = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", id).Attrs;
            foreach (var v in attrs)
            {
                if (!attrsDic.ContainsKey(v.id))
                    attrsDic.Add(v.id, 0);
                if (ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", v.id).ValueType == Enum.ValueType.Percentage)
                    attrsDic[v.id] += v.value / 100.0f;
                else
                    attrsDic[v.id] += v.value;
            }

            foreach (var v in attrsDic)
                _attrsData.Add(new IntFloatPair(v.Key, v.Value));

            _attrsList.numItems = _attrsData.Count;
        }

        private void OnEquipRenderer(int index, GObject item)
        {
            item.icon = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", _equipsData[index]).Icon;
            item.grayed = !DatasMgr.Instance.ContainEquip(_equipsData[index]);
        }

        private void OnClickEquip(EventContext context)
        {
            var index = _equipList.GetChildIndex((GObject)context.data);
            SelectEquip(_equipsData[index]);
        }

        private void OnAttrRenderer(int index, GObject item)
        {
            if (!_attrsMap.ContainsKey(item.id))
                _attrsMap.Add(item.id, UIManager.Instance.CreateUI<CommonAttrItem>("CommonAttrItem", item));
            _attrsMap[item.id].Update(ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", _attrsData[index].id).Name, _attrsData[index].value.ToString());
        }

        private void OnClickBuy()
        {
            var cost = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", _selectEquip).Cost;
            if (DatasMgr.Instance.GetItem((int)Enum.ItemType.EquipRes) < cost)
            {
                TipsMgr.Instance.Add("×ÊÔ´²»×ã£¡");
                return;
            }
            DatasMgr.Instance.BuyEquipC2S(_selectEquip);
        }

        private void OnBuyEquipS2C(params object[] args)
        {
            _equipList.numItems = _equipsData.Count;
        }

        private void OnClickClose()
        {
            UIManager.Instance.ClosePanel(name);
        }

        public override void Dispose(bool disposeGCom = false)
        {
            base.Dispose(disposeGCom);
            foreach (var v in _attrsMap)
                v.Value.Dispose();
            _attrsMap.Clear();
        }
    }
}
