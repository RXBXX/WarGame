using FairyGUI;

namespace WarGame.UI
{
    public class FightPanel : UIBase
    {
        public FightPanel(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        {
            _gCom.GetChild("closeBtn").onClick.Add(()=> {
                SceneMgr.Instance.DestroyScene();
            });
        }
    }
}
