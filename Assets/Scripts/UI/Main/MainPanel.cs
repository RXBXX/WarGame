using FairyGUI;

namespace WarGame.UI
{
    public class MainPanel : UIBase
    {
        public MainPanel(GComponent gCom, string name) : base(gCom, name)
        {
            _gCom.GetChild("mainBtn").onClick.Add(OnClickMainBtn);
            ((GLoader)_gCom.GetChild("bg")).url = "UI/Background/MainBG";
        }

        private void OnClickMainBtn()
        {
            UIManager.Instance.ClosePanel(name);
            UIManager.Instance.OpenPanel("Map", "MapPanel");
        }
    }
}
