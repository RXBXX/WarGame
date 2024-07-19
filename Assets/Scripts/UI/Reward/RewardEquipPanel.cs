using FairyGUI;

namespace WarGame.UI
{
    public class RewardEquipPanel : UIBase
    {
        private WGArgsCallback _callback;

        public RewardEquipPanel(GComponent gCom, string customName, params object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;

            GetGObjectChild<GGraph>("bg").onClick.Add(OnClickBG);

            var equipId = (int)args[0];
            var config = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", equipId);

            GetGObjectChild<GButton>("hero").icon = config.FullLengthIcon;
            GetGObjectChild<GTextField>("name").text = config.GetTranslation("Name");

            _callback = (WGArgsCallback)args[1];
        }

        private void OnClickBG()
        {
            UIManager.Instance.ClosePanel(name);
            if (null != _callback)
                _callback();
        }
    }
}
