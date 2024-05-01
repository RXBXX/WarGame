using FairyGUI;

namespace WarGame.UI
{
    public class HUDRole : HUD
    {
        private GProgressBar _hp;
        private GProgressBar _rage;
        private GObject _cancelBtn;

        public HUDRole(GComponent gCom, string customName, object[] args = null) : base(gCom, customName, args)
        {
            _hp = (GProgressBar)_gCom.GetChild("hp");
            _hp.max = 100;
            _hp.value = 100;
            _rage = (GProgressBar)_gCom.GetChild("rage");
            _rage.max = 100;
            _rage.value = 0;

            ((GTextField)_gCom.GetChild("id")).text = args[0].ToString();

            if (null != args[1])
                _hp.GetController("style").SetSelectedIndex((int)args[1]);

            _cancelBtn = _gCom.GetChild("cancelBtn");
            _cancelBtn.onClick.Add(() =>
            {
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Cancel);
            });
        }

        public void UpdateHP(float hp)
        {
            DebugManager.Instance.Log("HUDRole.UpdateHP:" + hp);
            _hp.value = hp;
        }

        public void SetFightCancelVisible(bool visible)
        {
            _cancelBtn.visible = visible;
        }
    }
}
