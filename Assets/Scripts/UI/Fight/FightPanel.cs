using FairyGUI;

namespace WarGame.UI
{
    public class FightPanel : UIBase
    {
        private GTextField _round;

        public FightPanel(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        {
            _gCom.GetChild("closeBtn").onClick.Add(()=> {
                SceneMgr.Instance.DestroyBattleFiled();
            });

            _round = (GTextField)_gCom.GetChild("round");
            _round.text = "0";

            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_RoundOver_Event, OnUpdateRound);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_RoundChange_Event, OnStartEnemyTurn);
        }

        private void OnUpdateRound(object[] args)
        {
            _round.text = "Round:" + ((int)args[0]).ToString();
            GetChild<FightTips>("tips").ShowTips("Hero Turn", (RoundFunc)args[1]);
        }

        private void OnStartEnemyTurn(object[] args)
        {
            GetChild<FightTips>("tips").ShowTips("EnemyTurn", (RoundFunc)args[0]);
        }

        public override void Dispose(bool disposeGCom = false)
        {
            base.Dispose(disposeGCom);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_RoundOver_Event, OnUpdateRound);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_RoundChange_Event, OnStartEnemyTurn);
        }
    }
}
