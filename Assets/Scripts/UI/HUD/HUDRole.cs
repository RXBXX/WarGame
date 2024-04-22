using FairyGUI;

namespace WarGame.UI
{
    public class HUDRole : HUD
    {
        private GProgressBar _hp;
        public HUDRole(GComponent gCom, string customName, object[] args = null):base(gCom, customName, args)
        {
            _hp = (GProgressBar)_gCom.GetChild("hp");
            _hp.max = 100;
            _hp.value = 100;
        }

        public void UpdateHP(float hp)
        {
            DebugManager.Instance.Log("HUDRole.UpdateHP:" + hp);
            _hp.value = hp;
        }
    }
}
