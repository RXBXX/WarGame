using FairyGUI;

namespace WarGame.UI
{
    public class LoginPanel : UIBase
    {
        public LoginPanel(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        {
            var mainBtn = (GButton)_gCom.GetChild("mainBtn");
            mainBtn.title = "PLAY";
            mainBtn.onClick.Add(OnClickMainBtn);

            _gCom.GetChild("title").text = "KA KA KILL";
            ((GLoader)_gCom.GetChild("bg")).url = "UI/Background/MainBG";
        }

        private void OnClickMainBtn()
        {
            UIManager.Instance.ClosePanel(name);
            UIManager.Instance.OpenPanel("Main", "MainPanel");
        }
    }
}