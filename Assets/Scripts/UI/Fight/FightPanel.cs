using FairyGUI;

namespace WarGame.UI
{
    public class FightPanel : UIBase
    {
        public FightPanel(GComponent gCom, string name) : base(gCom, name)
        {
            _gCom.GetChild("closeBtn").onClick.Add(()=> {
                SceneMgr.Instance.Destroy();
            });
        }
    }
}
