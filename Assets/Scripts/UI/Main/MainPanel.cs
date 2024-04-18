using FairyGUI;

namespace WarGame.UI
{
    public class MainPanel : UIBase
    {
        public MainPanel(GComponent gCom, string name) : base(gCom, name)
        {
            _gCom.GetChild("mainBtn").onClick.Add(OnClickMainBtn);
        }

        private void OnClickMainBtn()
        {
            UIManager.Instance.OpenPanel("Map", "MapPanel");
        }
    }
}
