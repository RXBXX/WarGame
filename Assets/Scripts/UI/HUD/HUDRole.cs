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
        private Controller _stateC;
        private Controller _followingC;
        private Controller _hpVisibleC;
        private GLoader _elementLoader;
        private Transition _warning;
        private float _hpValue;

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
            _hpVisibleC = GetController("hpVisible");
            _warning = GetTransition("warning");

            ((GTextField)_gCom.GetChild("id")).text = args[0].ToString();
            _hp.GetController("style").SetSelectedIndex((int)args[1]);

            Init((float)args[2], (float)args[3], (float)args[4], (float)args[5], (Enum.Element)args[6]);
        }

        private void Init(float HP, float totalHP, float rage, float totalRage, Enum.Element element)
        {
            _hpValue = HP;
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
            //Vector3.Dot(cameraFor.normalized, ownerFor.normalized) 的值超出 - 1 到 1 之间：由于浮点运算的不精确性，两个归一化向量的点积可能会略微超出理论上的范围[-1, 1]。
            //Mathf.Acos 函数对于超出这个范围的输入值会返回 NaN。
            if (cameraFor != Vector3.zero)
            {
                var ownerFor = _gameObject.transform.forward;
                ownerFor.y = 0;
                ownerFor = ownerFor.normalized;
                var cameraForXZ = cameraFor - new Vector3(0, cameraFor.y, 0);
                cameraForXZ = cameraForXZ.normalized;
                if (ownerFor != Vector3.zero)
                {
                    var angle = Mathf.Acos(Vector3.Dot(cameraForXZ, ownerFor)) / 2 / Mathf.PI * 360;
                    if (Vector3.Cross(cameraForXZ, ownerFor).y > 0)
                        angle = 360 - angle;

                    if (!float.IsNaN(angle))
                        _gCom.rotationY = angle;
                }

                var xAngle = Mathf.Acos(Vector3.Dot(cameraFor, cameraForXZ)) / 2 / Mathf.PI * 360;
                if (!float.IsNaN(xAngle))
                    _gCom.rotationX = xAngle;
            }
        }

        public void UpdateHP(float hp)
        {
            if (hp == _hpValue)
                return;
            GTween.Kill(_hp);
            _hpValue = hp;
            float duration = (float)(Mathf.Abs(_hpValue - (float)_hp.value) / _hp.max) * 0.2F;
            _hp.TweenValue(_hpValue, duration);
        }

        public void UpdateRage(float rage)
        {
            if (rage == _rage.value)
                return;

            GTween.Kill(_rage);
            float duration = (float)(Mathf.Abs(rage - (float)_rage.value) / _rage.max) * 0.2F;
            _rage.TweenValue(rage, duration);
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
            _followingC.SetSelectedIndex(following ? 1 : 0);
        }

        public void SetHPVisible(bool visible)
        {
            _hpVisibleC.SetSelectedIndex(visible ? 1 : 0);
        }

        public float Warning()
        {
            _warning.Play();
            return _warning.totalDuration;
        }

        public void Preview(float deltaHP)
        {
            if (0 == deltaHP)
                return;
            GTween.Kill(_hp);
            var targetHP = _hpValue - deltaHP;
            float duration = (float)(Mathf.Abs(targetHP - (float)_hp.value) / _hp.max) * 0.2F;
            _hp.TweenValue(targetHP, duration);
        }

        public void CancelPreview()
        {
            GTween.Kill(_hp);
            _hp.value = _hpValue;
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
