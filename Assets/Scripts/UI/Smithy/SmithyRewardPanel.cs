using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using DG.Tweening;

namespace WarGame.UI
{
    public class SmithyRewardPanel : UIBase
    {
        private bool _canClose = false;

        public SmithyRewardPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;
            GetGObjectChild<GGraph>("mask").onClick.Add(OnClick);
            GetGObjectChild<GLabel>("title").title = ConfigMgr.Instance.GetTranslation("SmithyRewardPanel_Title");

            var equip = GetGObjectChild<GButton>("equip");
            var equipData = DatasMgr.Instance.GetEquipmentData((int)args[0]);
            var config = equipData.GetConfig();
            equip.icon = config.Icon;
            equip.title = config.GetTranslation("Name");

            var forge = GetTransition("forge");
            forge.SetHook("Forge", ()=> { AudioMgr.Instance.PlaySound("Assets/Audios/Forge.wav"); });
            forge.SetHook("Success", () => { AudioMgr.Instance.PlaySound("Assets/Audios/Equip_Buy.wav"); });
            forge.Play(()=> { _canClose = true; });
        }


        private void OnClick(EventContext context)
        {
            if (!_canClose)
                return;

            UIManager.Instance.ClosePanel(name);
        }

        public override void Dispose(bool disposeGCom = false)
        {
            base.Dispose(disposeGCom);
        }
    }
}
