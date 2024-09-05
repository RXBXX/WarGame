using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using DG.Tweening;

namespace WarGame.UI
{
    public class SmithyRewardPanel : UIBase
    {
        private SmithyReward _equip;
        private float _interval = 3.0f;

        public SmithyRewardPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;

            GetGObjectChild<GLabel>("title").title = ConfigMgr.Instance.GetTranslation("SmithyRewardPanel_Title");
            _equip = GetChild<SmithyReward>("equip");
            _equip.UpdateComp((int)args[0]);
        }

        public override void Update(float deltaTime)
        {
            _interval -= deltaTime;
            if (_interval <= 0)
                UIManager.Instance.ClosePanel(name);
        }

        public override void Dispose(bool disposeGCom = false)
        {
            _equip.Dispose();
            base.Dispose(disposeGCom);
        }
    }
}
