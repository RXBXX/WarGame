using WarGame.UI;
using FairyGUI;

namespace WarGame.UI
{
    public class SettingsPanel : UIBase
    {
        public SettingsPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;

            var saveBtn = GetUIChild<GButton>("saveBtn");
            saveBtn.onClick.Add(() =>
            {
                UIManager.Instance.ClosePanel(this.name);
                DatasMgr.Instance.SaveGameData();
            });

            var returnBtn = GetUIChild<GButton>("returnBtn");
            returnBtn.onClick.Add(() => {
                UIManager.Instance.ClosePanel(this.name);
                SceneMgr.Instance.DestroyBattleFiled();
            });

            var cancelBtn = GetUIChild<GButton>("cancelBtn");
            cancelBtn.onClick.Add(() => {
                UIManager.Instance.ClosePanel(this.name);
            });
        }
    }
}
