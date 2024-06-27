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

            GetChild<HUDSkills>("skills").UpdateComp(args);
            EventDispatcher.Instance.AddListener(Enum.Event.HUDInstruct_Click_Skill, OnAttackEvent);
        }


        private void Idle()
        {
            EventDispatcher.Instance.PostEvent(Enum.Event.HUDInstruct_Idle_Event);
        }

        private void Cancel()
        {
            if (0 == _stateC.selectedIndex)
            {
                EventDispatcher.Instance.PostEvent(Enum.Event.HUDInstruct_Cancel_Event);
            }
            else if (1 == _stateC.selectedIndex)
            {
                _stateC.SetSelectedIndex(0);
            }
            else
            {
                _stateC.SetSelectedIndex(1);
                EventDispatcher.Instance.PostEvent(Enum.Event.HUDInstruct_Cancel_Skill);
            }
        }

        private void Attack()
        {
            _stateC.SetSelectedIndex(1);
            EventDispatcher.Instance.PostEvent(Enum.Event.HUDInstruct_Attack);
        }

        private void OnAttackEvent(object[] args)
        {
            _stateC.SetSelectedIndex(2);
        }

        public override void Dispose(bool disposeGComp = false)
        {
            base.Dispose(disposeGComp);
            EventDispatcher.Instance.RemoveListener(Enum.Event.HUDInstruct_Click_Skill, OnAttackEvent);
        }
    }
}
