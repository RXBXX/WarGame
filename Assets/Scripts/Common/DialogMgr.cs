using UnityEngine;
using WarGame.UI;

namespace WarGame
{
    public class DialogMgr : Singeton<DialogMgr>
    {
        public override bool Init()
        {
            return true;
        }

        public void OpenDialog(int dialogGroup, WGEventCallback callback)
        {
            UIManager.Instance.OpenPanel("Dialog", "DialogPanel", new object[] { dialogGroup, callback});
        }

        public void CloseDialog()
        {
            UIManager.Instance.ClosePanel("DialogPanel");
        }
    }
}
