using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class CommonAttrItem : UIBase
    {
        private GTextField _name;
        private GTextField _value;

        public CommonAttrItem(GComponent gCom, string customName = null, params object[] args) : base(gCom, customName, args)
        {
            _name = GetGObjectChild<GTextField>("name");
            _value = GetGObjectChild<GTextField>("value");
        }

        public void Update(string name, string value)
        {
            _name.text = name;
            _value.text = value;
        }
    }
}
