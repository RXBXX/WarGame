using FairyGUI;

namespace WarGame.UI
{
    public class TimePanel : UIBase
    {
        public TimePanel(GComponent gCom, string customName, params object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.AlertLayer;

            var sunToMoon = GetTransition("sunToMoon");
            var config = ConfigMgr.Instance.GetConfig<DayConfig>("DayConfig", (int)args[0]);
            GetGObjectChild<GTextField>("desc").text = config.GetTranslation("Desc");

            if ((Enum.DayType)args[0] == Enum.DayType.Day)
            {
                GetGObjectChild<GLoader>("now").url = "ui://Common/Moon";
                GetGObjectChild<GLoader>("next").url = "ui://Common/Sun";
                sunToMoon.Play(()=> {
                        UIManager.Instance.ClosePanel(name);
                });
            }
            else
            {
                GetGObjectChild<GLoader>("now").url = "ui://Common/Sun";
                GetGObjectChild<GLoader>("next").url = "ui://Common/Moon";
                sunToMoon.Play(() => {
                    UIManager.Instance.ClosePanel(name);
                });
            }

            AudioMgr.Instance.PlaySound("Assets/Audios/Clock.wav");
        }
    }
}
