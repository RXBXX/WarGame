using FairyGUI;
using UnityEngine;

namespace WarGame.UI
{
    public class MapInstruct : HUD
    {
        public MapInstruct(GComponent gCom, string name) : base(gCom, name)
        {
            Debug.Log("Contruct MapInstance");
            _gCom.GetChild("moveBtn").onClick.Add(Move);
            _gCom.GetChild("cancelBtn").onClick.Add(Cancel);
        }


        public void Move()
        {
            Debug.Log("Instruct:Execute");
            EventDispatcher.Instance.Dispatch("MapInstruct_Move_Event");
        }

        public void Cancel()
        {
            Debug.Log("Instruct:Execute");
            EventDispatcher.Instance.Dispatch("MapInstruct_Cancel_Event");
        }

        public override void Dispose(bool isPanel = false)
        {
            Debug.Log("MapInstruct.Dispose");
            base.Dispose();
        }
    }
}
