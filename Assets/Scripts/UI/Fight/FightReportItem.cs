using FairyGUI;
using System.Collections;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class FightReportItem : UIBase
    {
        private GTextField _PA, _MA, _Cure, _PD, _MD;
        private GButton _hero;
        public FightReportItem(GComponent gCom, string customName = null, params object[] args) : base(gCom, customName, args)
        {
            _hero = GetGObjectChild<GButton>("hero");
            _PA = GetGObjectChild<GTextField>("PA");
            _MA = GetGObjectChild<GTextField>("MA");
            _Cure = GetGObjectChild<GTextField>("Cure");
            _PD = GetGObjectChild<GTextField>("PD");
            _MD = GetGObjectChild<GTextField>("MD");
        }

        public void UpdateItem(string icon, Dictionary<Enum.AttrType, float> attrsDic)
        {
            _hero.icon = icon;

            _PA.text = attrsDic.ContainsKey(Enum.AttrType.PhysicalAttack) ? attrsDic[Enum.AttrType.PhysicalAttack].ToString() : "0";
            _MA.text = attrsDic.ContainsKey(Enum.AttrType.MagicAttack) ? attrsDic[Enum.AttrType.MagicAttack].ToString() : "0";
            _Cure.text = attrsDic.ContainsKey(Enum.AttrType.Cure) ? attrsDic[Enum.AttrType.Cure].ToString() : "0";
            _PD.text = attrsDic.ContainsKey(Enum.AttrType.PhysicalDefense) ? attrsDic[Enum.AttrType.PhysicalDefense].ToString() : "0";
            _MD.text = attrsDic.ContainsKey(Enum.AttrType.MagicDefense) ? attrsDic[Enum.AttrType.MagicDefense].ToString() : "0";
        }
    }
}
