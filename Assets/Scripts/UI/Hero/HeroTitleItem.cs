using FairyGUI;

namespace WarGame.UI
{
    public class HeroTitleItem : UIBase
    {
        private GTextField _title;
        private int _state;
        private Controller _stateC;
        private WGArgsCallback _callback;

        public HeroTitleItem(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _title = GetUIChild<GTextField>("title");
            _stateC = GetController("state");
            GetUIChild<GButton>("btn").onClick.Add(OnClickBtn);
        }

        public void Init(string title, int state, WGArgsCallback callback)
        {
            _title.text = title;
            _state = state;
            _stateC.SetSelectedIndex(_state);
            _callback = callback;
        }

        private void OnClickBtn()
        {
            _state = _state == 0 ? 1 : 0;
            _stateC.SetSelectedIndex(_state);
            _callback(new object[] { _state == 1});
        }
    }
}
