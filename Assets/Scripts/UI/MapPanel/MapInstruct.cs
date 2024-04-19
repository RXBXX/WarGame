using FairyGUI;
using UnityEngine;

namespace WarGame.UI
{
    public class MapInstruct : UIBase
    {
        private EventCallback0 _navationDele, _closeDele;

        public MapInstruct(GComponent gCom, string name) : base(gCom, name)
        {
            Debug.Log("Contruct MapInstance");
            _gCom.GetChild("navigationBtn").onClick.Add(Execute);
        }

        public void SetDelegate(EventCallback0 func, EventCallback0 closeFunc)
        {
            this._navationDele = func;
            this._closeDele = closeFunc;
        }

        public void Execute()
        {
            Debug.Log("Instruct:Execute");
            this._navationDele();
        }

        public override void Dispose(bool isPanel = false)
        {
            Debug.Log("MapInstruct.Dispose");
            base.Dispose();

            if (null != _closeDele)
            {
                _closeDele();
            }
        }
    }
}
