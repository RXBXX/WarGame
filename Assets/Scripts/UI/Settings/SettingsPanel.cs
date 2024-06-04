using WarGame.UI;
using FairyGUI;

namespace WarGame.UI
{
    public class SettingsPanel : UIBase
    {
        public SettingsPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;

            var saveBtn = GetGObjectChild<GButton>("saveBtn");
            saveBtn.onClick.Add(() =>
            {
                EventDispatcher.Instance.PostEvent(Enum.EventType.Save_Data);

                UIManager.Instance.ClosePanel(this.name);
                DatasMgr.Instance.SaveGameData();
            });

            var returnBtn = GetGObjectChild<GButton>("returnBtn");
            returnBtn.onClick.Add(() => {
                UIManager.Instance.ClosePanel(this.name);
                SceneMgr.Instance.DestroyBattleFiled();
            });

            var cancelBtn = GetGObjectChild<GButton>("cancelBtn");
            cancelBtn.onClick.Add(() => {
                UIManager.Instance.ClosePanel(this.name);
            });
        }
    }
}
