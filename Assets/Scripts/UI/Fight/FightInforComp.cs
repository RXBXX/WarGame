using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class FightInforComp: UIBase
    {
        private Transition _vsC;
        private GTextField _attrs;

        public FightInforComp(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _vsC = GetTransition("show");
            _attrs = GetGObjectChild<GTextField>("attrs");
        }

        public void Show(int id, Vector3 pos)
        {
            pos = CameraMgr.Instance.MainCamera.WorldToScreenPoint(pos);
            pos.y = Screen.height - pos.y;
            pos = GRoot.inst.GlobalToLocal(pos);
            SetPosition(pos);

            var role = RoleManager.Instance.GetRole(id);
            var attrs = "";
            ConfigMgr.Instance.ForeachConfig<AttrConfig>("AttrConfig", (config) =>
            {
                attrs += ((AttrConfig)config).Name + ":" + role.GetAttribute((Enum.AttrType)config.ID) + "\n";
            });
            _attrs.text = attrs;

            _vsC.Play();
        }

        public void Hide()
        {
            _vsC.PlayReverse();
        }
    }
}
