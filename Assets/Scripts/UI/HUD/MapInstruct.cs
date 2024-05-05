using FairyGUI;

namespace WarGame.UI
{
    public class HUDInstruct : HUD
    {
        private Controller _stateC;
        public HUDInstruct(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        {
            var idleBtn = _gCom.GetChild("idleBtn");
            var attackBtn = _gCom.GetChild("attackBtn");
            var cancelBtn = _gCom.GetChild("cancelBtn");
            _stateC = _gCom.GetController("state");

            idleBtn.onClick.Add(Idle);
            cancelBtn.onClick.Add(Cancel);
            attackBtn.onClick.Add(Attack);

            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Attack_Event, OnAttackEvent);
        }


        private void Idle()
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.HUDInstruct_Idle_Event);
        }

        private void Cancel()
        {
            if (0 == _stateC.selectedIndex)
            {
                EventDispatcher.Instance.PostEvent(Enum.EventType.HUDInstruct_Cancel_Event);
            }
            else if (1 == _stateC.selectedIndex)
            {
                _stateC.SetSelectedIndex(0);
            }
            else
            {
                _stateC.SetSelectedIndex(1);
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Cancel);
            }
        }

        private void Attack()
        {
            _stateC.SetSelectedIndex(1);
        }

        private void OnAttackEvent(object[] args)
        {
            _stateC.SetSelectedIndex(2);
        }

        public override void Dispose(bool disposeGComp = false)
        {
            base.Dispose(disposeGComp);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Attack_Event, OnAttackEvent);
        }
    }
}
