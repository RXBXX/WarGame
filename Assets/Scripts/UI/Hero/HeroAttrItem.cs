using FairyGUI;

namespace WarGame.UI
{
    public class HeroAttrItem : UIBase
    {
        private GTextField _name;
        private GTextField _levelValue;
        private GTextField _talentValue;
        private GTextField _equipValue;

        public HeroAttrItem(GComponent gCom, string customName = null, params object[] args) : base(gCom, customName, args)
        {
            _name = GetGObjectChild<GTextField>("name");
            _levelValue = GetGObjectChild<GTextField>("levelValue");
            _talentValue = GetGObjectChild<GTextField>("talentValue");
            _equipValue = GetGObjectChild<GTextField>("equipValue");
        }

        public void UpdateItem(FourStrPair pair)
        {
            _name.text = pair.name;
            _levelValue.text = pair.value1;
            _talentValue.text = pair.value2;
            _equipValue.text = pair.value3;
        }
    }
}
