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
        }


        private void Idle()
        {
            DebugManager.Instance.Log("MapInstruct:Idle");
            EventDispatcher.Instance.PostEvent(Enum.EventType.HUDInstruct_Idle_Event);
        }

        private void Cancel()
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.HUDInstruct_Cancel_Event);
        }

        private void Attack()
        {
            if (0 == _stateC.selectedIndex)
                OpenSkills();
            else
                CloseSkills();
        }

        private void OpenSkills()
        {
            _stateC.SetSelectedIndex(1);
        }

        private void CloseSkills()
        {
            _stateC.SetSelectedIndex(0);
        }
    }
}
