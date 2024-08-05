using FairyGUI;

namespace WarGame.UI
{
    public class LoginPanel : UIBase
    {
        public LoginPanel(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        {
            var mainBtn = (GButton)_gCom.GetChild("mainBtn");
            mainBtn.title = ConfigMgr.Instance.GetTranslation("LoginPanel_Play");
            mainBtn.onClick.Add(OnClickMainBtn);

            _gCom.GetChild("title").text = ConfigMgr.Instance.GetTranslation("Game_Name");
            ((GLoader)_gCom.GetChild("bg")).url = "UI/Background/LoginBG";
        }

        private void OnClickMainBtn()
        {
            UIManager.Instance.ClosePanel(name);
            UIManager.Instance.OpenPanel("Main", "MainPanel");
        }
    }
}
