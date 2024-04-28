using FairyGUI;

namespace WarGame.UI
{
    public class FightPanel : UIBase
    {
        private GTextField _round;
        public FightPanel(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        {
            _gCom.GetChild("closeBtn").onClick.Add(()=> {
                SceneMgr.Instance.DestroyScene();
            });

            _round = (GTextField)_gCom.GetChild("round");
            _round.text = "Round:0";
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Round_Event, UpdateRound);
        }

        private void UpdateRound(object[] args)
        {
            _round.text = "Round:" + ((int)args[0]).ToString();
        }

        public override void Dispose(bool disposeGCom = false)
        {
            base.Dispose(disposeGCom);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Round_Event, UpdateRound);
        }
    }
}
