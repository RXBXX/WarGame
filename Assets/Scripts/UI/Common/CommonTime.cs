using FairyGUI;

namespace WarGame.UI
{
    public class CommonTime : UIBase
    {
        private GImage _sun;
        private GImage _moon;
        private float _width;
        private Enum.DayType _day;

        public CommonTime(GComponent gCom, string customName = null, params object[] args) : base(gCom, customName, args)
        {
            _width = GCom.width;// * 2;
            _sun = GetGObjectChild<GImage>("sun");
            _moon = GetGObjectChild<GImage>("moon");

            _day = TimeMgr.Instance.GetDayType();
        }

        public override void Update(float deltaTime)
        {
            var timePer = TimeMgr.Instance.GetGameTimePercent();
            _sun.x = _width * (1 - timePer * 2);
            _moon.x = _width * (2 - timePer * 2);

            var light = TimeMgr.Instance.GetDayType();
            if (_day != light)
            {
                _day = light;
                UIManager.Instance.OpenPanel("Common", "TimePanel", new object[] { _day});
            }
        }
    }
}

