using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class HUDRole : HUD
    {
        private GProgressBar _hp;
        private GProgressBar _rage;
        private GList _buffList;
        private List<Pair> _buffs;

        public HUDRole(GComponent gCom, string customName, object[] args = null) : base(gCom, customName, args)
        {
            _hp = (GProgressBar)_gCom.GetChild("hp");
            _hp.max = 100;
            _hp.value = 100;
            _rage = (GProgressBar)_gCom.GetChild("rage");
            _rage.max = 100;
            _rage.value = 0;

            _buffList = (GList)_gCom.GetChild("buffList");
            _buffList.itemRenderer = OnItemRenderer;

            ((GTextField)_gCom.GetChild("id")).text = args[0].ToString();

            if (null != args[1])
                _hp.GetController("style").SetSelectedIndex((int)args[1]);
        }

        public void UpdateHP(float hp)
        {
            _hp.value = hp;
        }

        public void UpdateBuffs(List<Pair> buffs)
        {
            _buffs = buffs;
            _buffList.numItems = buffs.Count;
        }

        private void OnItemRenderer(int index, GObject item)
        {
            var buffConfig = ConfigMgr.Instance.GetConfig<BufferConfig>("BufferConfig", _buffs[index].id);
            ((GLabel)item).title = buffConfig.Name + "_" + _buffs[index].value;
        }
    }
}
