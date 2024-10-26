using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class FightRoleInfo : UIBase
    {
        private GTextField _name;
        private GList _attrList;
        private GProgressBar _hp;
        private GProgressBar _rage;
        private List<ThreeStrPair> _attrsData = new List<ThreeStrPair>();
        private Dictionary<string, FightAttrItem> _attrsItemDic = new Dictionary<string, FightAttrItem>();
        private GLoader _element;

        public FightRoleInfo(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _name = GetGObjectChild<GTextField>("name");
            _hp = GetGObjectChild<GProgressBar>("hp");
            _rage = GetGObjectChild<GProgressBar>("rage");
            _attrList = GetGObjectChild<GList>("attrList");
            _element = GetGObjectChild<GLoader>("element");
            _attrList.itemRenderer = OnItemRenderer;

            GetGObjectChild<GTextField>("attrTitle").text = ConfigMgr.Instance.GetTranslation("Attr_Title");
            GetGObjectChild<GTextField>("baseTitle").text = ConfigMgr.Instance.GetTranslation("Base_Title");
            GetGObjectChild<GTextField>("elementTitle").text = ConfigMgr.Instance.GetTranslation("Element_Title");
        }

        private void OnItemRenderer(int index, GObject item)
        {
            if (!_attrsItemDic.ContainsKey(item.id))
            {
                _attrsItemDic[item.id] = UIManager.Instance.CreateUI<FightAttrItem>("FightAttrItem", item);
            }
            _attrsItemDic[item.id].UpdateItem(_attrsData[index]);
        }

        public void Show(Vector2 centerPos, int roleID, int levelID)
        {
            if (GCom.visible)
                return;

            var role = RoleManager.Instance.GetRole(roleID);
            _name.text = role.GetName();

            _hp.max = role.GetAttribute(Enum.AttrType.HP);
            _hp.value = role.GetHP();

            _rage.max = role.GetAttribute(Enum.AttrType.Rage);
            _rage.value = role.GetRage();

            ConfigMgr.Instance.ForeachConfig<AttrConfig>("AttrConfig", (config) =>
            {
                if ((Enum.AttrType)config.ID == Enum.AttrType.HP || (Enum.AttrType)config.ID == Enum.AttrType.Rage)
                {
                    return;
                }

                var value = role.GetAttribute((Enum.AttrType)config.ID);
                if (value > 0)
                {
                    var elementAdd = BattleMgr.Instance.GetElementAdd(ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", levelID).Element, roleID);
                    _attrsData.Add(new ThreeStrPair(config.GetTranslation("Name"), BattleMgr.Instance.GetAttributeStr(config.ID, value), BattleMgr.Instance.GetAttributeStr(config.ID, value * elementAdd)));
                }
            });

            _attrList.numItems = _attrsData.Count;
            _attrList.ResizeToFit();

            if (centerPos.y + GCom.height > GRoot.inst.height)
            {
                centerPos.y = GRoot.inst.height - GCom.height;
            }
            SetPosition(centerPos);

            _element.url = role.GetElementConfig().Icon;

            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
            _attrsData.Clear();
        }

        public override void Dispose(bool disposeGCom = false)
        {
            foreach (var v in _attrsItemDic)
                v.Value.Dispose();
            _attrsItemDic.Clear();

            base.Dispose(disposeGCom);
        }
    }
}
