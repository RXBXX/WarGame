using FairyGUI;

namespace WarGame.UI
{
    public class HUDInstruct : HUD
    {
        public HUDInstruct(GComponent gCom, string name) : base(gCom, name)
        {
            _gCom.GetChild("moveBtn").onClick.Add(Move);
            _gCom.GetChild("cancelBtn").onClick.Add(Cancel);
        }


        public void Move()
        {
            EventDispatcher.Instance.Dispatch(Enum.EventType.HUDInstruct_Move_Event);
        }

        public void Cancel()
        {
            EventDispatcher.Instance.Dispatch(Enum.EventType.HUDInstruct_Cancel_Event);
        }

        public void Attack()
        {
            EventDispatcher.Instance.Dispatch(Enum.EventType.HUDInstruct_Attack_Event);
        }
    }
}
