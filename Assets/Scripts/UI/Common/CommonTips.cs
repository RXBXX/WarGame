using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class CommonTips : UIBase
    {
        public CommonTips(GComponent gCom, string customName = null, params object[] args) : base(gCom, customName, args)
        {
            GetGObjectChild<GTextField>("txt").text = ConfigMgr.Instance.GetTranslation("CommonTips");
        }
    }
}
