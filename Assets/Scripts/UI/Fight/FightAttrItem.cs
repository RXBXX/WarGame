using FairyGUI;

namespace WarGame.UI
{
    public class FightAttrItem : UIBase
    {
        private GTextField _name;
        private GTextField _baseValue;
        private GTextField _elementValue;

        public FightAttrItem(GComponent gCom, string customName, params object[] args) : base(gCom, customName, args)
        {
            _name = GetGObjectChild<GTextField>("name");
            _baseValue = GetGObjectChild<GTextField>("baseValue");
            _elementValue = GetGObjectChild<GTextField>("elementValue");
        }

        public void UpdateItem(ThreeStrPair pair)
        {
            _name.text = pair.name;
            _baseValue.text = pair.value1;
            _elementValue.text = pair.value2;
        }
    }
}
