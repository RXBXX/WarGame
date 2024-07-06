using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class HUDRole : HUD
    {
        private GProgressBar _hp;
        private GProgressBar _rage;
        private GList _buffList;
        private List<BuffPair> _buffs;
        private Dictionary<string, HUDBuff> _hudBuffDic = new Dictionary<string, HUDBuff>();
        private float _hpChangeDuration = 1F;
        private Controller _stateC;
        private Controller _followingC;
        private GLoader _elementLoader;

        public HUDRole(GComponent gCom, string customName, object[] args = null) : base(gCom, customName, args)
        {
            _hp = (GProgressBar)_gCom.GetChild("hp");
            _hp.max = 100;
            _hp.value = 100;
            _rage = (GProgressBar)_gCom.GetChild("rage");
            _stateC = GetController("state");
            _followingC = GetController("following");
            _buffList = GetGObjectChild<GList>("buffList");
            _buffList.itemRenderer = OnItemRenderer;
            _elementLoader = GetGObjectChild<GLoader>("element");

            ((GTextField)_gCom.GetChild("id")).text = args[0].ToString();
            _hp.GetController("style").SetSelectedIndex((int)args[1]);

            Init((float)args[2], (float)args[3], (float)args[4], (float)args[5], (Enum.Element)args[6]);
        }

        private void Init(float HP, float totalHP, float rage, float totalRage, Enum.Element element)
        {
            _hp.max = totalHP;
            _hp.value = HP;

            _rage.max = totalRage;
            _rage.value = rage;

            _elementLoader.url = ConfigMgr.Instance.GetConfig<ElementConfig>("ElementConfig", (int)element).Icon;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            foreach (var v in _hudBuffDic)
                v.Value.Update(deltaTime);
        }

        protected override void UpdatePosition()
        {
            var cameraFor = CameraMgr.Instance.GetMainCamForward();
            cameraFor.y = 0;

            var ownerFor = _gameObject.transform.forward;
            ownerFor.y = 0;

            var angle = Mathf.Acos(Vector3.Dot(cameraFor.normalized, ownerFor.normalized)) / 2 / Mathf.PI * 360;
            if (Vector3.Cross(cameraFor.normalized, ownerFor.normalized).y > 0)
                angle = 360 - angle;
            _gCom.rotationY = angle;
        }

        public void UpdateHP(float hp)
        {
            if (hp == _hp.value)
                return;
            GTween.Kill(_hp);
            _hp.TweenValue(hp, _hpChangeDuration);
        }

        public void UpdateRage(float rage)
        {
            //DebugManager.Instance.Log("UpdateRage:"+rage);
            if (rage == _rage.value)
                return;
            GTween.Kill(_rage);
            _rage.TweenValue(rage, _hpChangeDuration);
        }

        public void UpdateBuffs(List<BuffPair> buffs)
        {
            _buffs = buffs;
            _buffList.numItems = buffs.Count;
            _buffList.ResizeToFit();
        }

        private void OnItemRenderer(int index, GObject item)
        {
            if (!_hudBuffDic.ContainsKey(item.id))
            {
                _hudBuffDic[item.id] = new HUDBuff((GComponent)item, "HUDBuff");
            }
            _hudBuffDic[item.id].UpdateBuff(_buffs[index].id, _buffs[index].value);
        }

        public void SetState(int state)
        {
            _stateC.SetSelectedIndex(state);
        }

        public void SetFollowing(bool following)
        {
            _followingC.SetSelectedIndex(following ? 1 :0);
        }

        public override void Dispose(bool disposeGComp = false)
        {
            GTween.Kill(_hp);
            GTween.Kill(_rage);
            foreach (var v in _hudBuffDic)
                v.Value.Dispose(false);
            _hudBuffDic.Clear();

            base.Dispose(disposeGComp);
        }
    }
}
