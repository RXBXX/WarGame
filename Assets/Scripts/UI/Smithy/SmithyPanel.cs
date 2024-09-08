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
        private Dictionary<string, SmithyEquipItem> _equipDic = new Dictionary<string, SmithyEquipItem>();
        private List<IntFloatPair> _attrsData = new List<IntFloatPair>();
        private GList _attrsList;
        private Dictionary<string, CommonAttrItem> _attrsMap = new Dictionary<string, CommonAttrItem>();
        private int _selectEquip = 0;
        private CommonResComp _resComp;
        private GTextField _name;
        private GTextField _cost;
        private GTextField _desc;

        public SmithyPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            GetGObjectChild<GLoader>("BG").url = "UI/Background/CommonBG";
            _gCom.GetChild("closeBtn").onClick.Add(OnClickClose);

            _equipList = GetGObjectChild<GList>("equips");
            _equipList.SetVirtual();
            _equipList.itemRenderer = OnEquipRenderer;
            _equipList.onClickItem.Add(OnClickEquip);

            _attrsList = GetGObjectChild<GList>("attrs");
            _attrsList.itemRenderer = OnAttrRenderer;

            _resComp = GetChild<CommonResComp>("resComp");
            _resComp.InitComp(new List<TwoIntPair> {
                new TwoIntPair((int)Enum.ItemType.EquipRes, DatasMgr.Instance.GetItem((int)Enum.ItemType.EquipRes)),
            });

            var forgeBtn = GetGObjectChild<GButton>("buyBtn");
            forgeBtn.title = ConfigMgr.Instance.GetTranslation("SmithyPanel_Forge");
            forgeBtn.onClick.Add(OnClickBuy);
                
            _name = GetGObjectChild<GTextField>("name");
            _cost = GetGObjectChild<GTextField>("costTxt");
            _desc = GetGObjectChild<GTextField>("desc");

            EventDispatcher.Instance.AddListener(Enum.Event.BuyEquipS2C, OnBuyEquipS2C);

            ConfigMgr.Instance.ForeachConfig<EquipmentConfig>("EquipmentConfig", (config) =>
            {
                _equipsData.Add(config.ID);
            });
            _equipList.numItems = _equipsData.Count;

            _equipList.selectedIndex = 0;
            SelectEquip(_equipsData[0]);
        }

        public override void Update(float deltaTime)
        {
            _resComp.Update(deltaTime);
        }

        private void SelectEquip(int id)
        {
            _selectEquip = id;

            _attrsData.Clear();

            var attrsDic = new Dictionary<int, float>();
            var equipConfig = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", id);
            foreach (var v in equipConfig.Attrs)
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

            _name.text = equipConfig.GetTranslation("Name");
            var ownNum = DatasMgr.Instance.GetItem((int)Enum.ItemType.EquipRes);
            if (ownNum >= equipConfig.Cost)
            {
                _cost.text = "[color=#5C8799]" + ownNum + "/" + equipConfig.Cost + "[/color]";
            }
            else
            {
                _cost.text = "[color=#CE4A35]" + ownNum + "/" + equipConfig.Cost + "[/color]";
            }
            _desc.text = equipConfig.GetTranslation("Desc");
        }

        private void OnEquipRenderer(int index, GObject item)
        {
            if (!_equipDic.ContainsKey(item.id))
            {
                _equipDic[item.id] = UIManager.Instance.CreateUI<SmithyEquipItem>("SmithyEquipItem", item);
            }
            _equipDic[item.id].UpdateItem(_equipsData[index]);
        }

        private void OnClickEquip(EventContext context)
        {
            var index = _equipList.GetChildIndex((GObject)context.data);
            index = _equipList.ChildIndexToItemIndex(index);
            SelectEquip(_equipsData[index]);
        }

        private void OnAttrRenderer(int index, GObject item)
        {
            if (!_attrsMap.ContainsKey(item.id))
                _attrsMap.Add(item.id, UIManager.Instance.CreateUI<CommonAttrItem>("CommonAttrItem", item));
            _attrsMap[item.id].Update(ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", _attrsData[index].id).GetTranslation("Name"), _attrsData[index].value.ToString());
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
            if ((Enum.ErrorCode)args[0] != Enum.ErrorCode.Success)
                return;

            _resComp.UpdateComp(new List<TwoIntPair> {
                new TwoIntPair((int)Enum.ItemType.EquipRes, DatasMgr.Instance.GetItem((int)Enum.ItemType.EquipRes)),
            });
            UIManager.Instance.OpenPanel("Smithy", "SmithyRewardPanel", new object[] { args[1]});
        }

        private void OnClickClose()
        {
            UIManager.Instance.ClosePanel(name);
        }

        public override void Dispose(bool disposeGCom = false)
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.BuyEquipS2C, OnBuyEquipS2C);

            base.Dispose(disposeGCom);
            foreach (var v in _attrsMap)
                v.Value.Dispose();
            _attrsMap.Clear();
        }
    }
}
