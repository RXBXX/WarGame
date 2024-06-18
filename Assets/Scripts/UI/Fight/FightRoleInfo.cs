using System.Collections;
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
        private List<StringStringPair> _attrsData = new List<StringStringPair>();
        private Dictionary<string, CommonAttrItem> _attrsItemDic = new Dictionary<string, CommonAttrItem>();

        public FightRoleInfo(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _name = GetGObjectChild<GTextField>("name");
            _hp = GetGObjectChild<GProgressBar>("hp");
            _rage = GetGObjectChild<GProgressBar>("rage");
            _attrList = GetGObjectChild<GList>("attrList");
            _attrList.itemRenderer = OnItemRenderer;
        }

        private void OnItemRenderer(int index, GObject item)
        {
            if (!_attrsItemDic.ContainsKey(item.id))
            {
                _attrsItemDic[item.id] = UIManager.Instance.CreateUI<CommonAttrItem>("CommonAttrItem", item);
            }
            _attrsItemDic[item.id].Update(_attrsData[index].id, _attrsData[index].value);
        }

        public void Show(Vector2 centerPos, int roleID)
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
                    var valueStr = string.Format("{0}  +{1}", value, value * AttributeMgr.Instance.GetElementAdd(roleID));
                    _attrsData.Add(new StringStringPair(config.Name, valueStr));
                }
            });

            _attrList.numItems = _attrsData.Count;
            _attrList.ResizeToFit();

            if (centerPos.y + GCom.height > GRoot.inst.height)
            {
                centerPos.y = GRoot.inst.height - GCom.height;
            }
            SetPosition(centerPos);

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
