using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using DG.Tweening;

namespace WarGame.UI
{
    public class SmithyReward : UIBase
    {
        private GButton _equip;

        public SmithyReward(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _equip = GetGObjectChild<GButton>("equip");
        }

        public void UpdateComp(int equipID)
        {
            DebugManager.Instance.Log(equipID);
            var equipData = DatasMgr.Instance.GetEquipmentData(equipID);
            var config = equipData.GetConfig();
            _equip.icon = config.Icon;
            _equip.title = config.GetTranslation("Name");
        }

        public override void Dispose(bool disposeGCom = false)
        {
            _equip.Dispose();
            base.Dispose(disposeGCom);
        }
    }
}
