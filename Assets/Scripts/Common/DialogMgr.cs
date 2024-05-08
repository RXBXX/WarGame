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

        public void OpenDialog(int dialogGroup)
        {
            UIManager.Instance.OpenPanel("Dialog", "DialogPanel", new object[] { dialogGroup});
        }

        public void CloseDialog()
        {
            UIManager.Instance.ClosePanel("DialogPanel");
        }
    }
}
