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
        private Dictionary<string, HUDBuff> _hudBuffDic = new Dictionary<string, HUDBuff>();
        private float _hpChangeDuration = 0.2F;

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

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            foreach (var v in _hudBuffDic)
                v.Value.Update(deltaTime);
        }

        public void UpdateHP(float hp)
        {
            if (hp == _hp.value)
                return;

            _hp.TweenValue(hp, _hpChangeDuration);
        }

        public void UpdateBuffs(List<Pair> buffs)
        {
            _buffs = buffs;
            _buffList.numItems = buffs.Count;
        }

        private void OnItemRenderer(int index, GObject item)
        {
            if (!_hudBuffDic.ContainsKey(item.id))
            {
                _hudBuffDic[item.id] = new HUDBuff((GComponent)item, "HUDBuff");
            }
            _hudBuffDic[item.id].UpdateBuff(_buffs[index].id, _buffs[index].value);
        }

        public override void Dispose(bool disposeGComp = false)
        {
            foreach (var v in _hudBuffDic)
                v.Value.Dispose(false);
            _hudBuffDic.Clear();

            base.Dispose(disposeGComp);
        }
    }
}
