using FairyGUI;

namespace WarGame.UI
{
    public class HUDInstruct : HUD
    {
        public HUDInstruct(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        {
            var idleBtn = _gCom.GetChild("idleBtn");
            var attackBtn = _gCom.GetChild("attackBtn");
            var cancelBtn = _gCom.GetChild("cancelBtn");

            idleBtn.onClick.Add(Idle);
            cancelBtn.onClick.Add(Cancel);
            attackBtn.onClick.Add(Attack);
        }


        private void Idle()
        {
            EventDispatcher.Instance.Dispatch(Enum.EventType.HUDInstruct_Idle_Event);
        }

        private void Cancel()
        {
            EventDispatcher.Instance.Dispatch(Enum.EventType.HUDInstruct_Cancel_Event);
        }

        private void Attack()
        {
            EventDispatcher.Instance.Dispatch(Enum.EventType.HUDInstruct_Attack_Event);
        }
    }
}
