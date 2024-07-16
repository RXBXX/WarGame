using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace WarGame
{
    public class Wand : Equip
    {
        private List<Tweener> _tweeners = new List<Tweener>();

        public Wand(EquipmentData data, Transform spineRoot) : base(data, spineRoot)
        { }

        public void OnAttackOver()
        {
        }

        protected override void EffectTake()
        {
            _effectAssetID = AssetsMgr.Instance.LoadAssetAsync<GameObject>(GetConfig().Effect, (prefab) =>
            {
                _effectGO = GameObject.Instantiate(prefab);
                _effectGO.transform.SetParent(_gameObject.transform.Find("spinePoint"));
                _effectGO.transform.localPosition = Vector3.zero;
            });

            _bulletAssetID = AssetsMgr.Instance.LoadAssetAsync<GameObject>(GetConfig().Bullet, (GameObject prefab) =>
            {
                for (int i = 0; i < _hitPoss.Count; i++)
                {
                    var bullet = GameObject.Instantiate(prefab);
                    bullet.transform.position = _hitPoss[i] + new Vector3(0, 1, 0);
                    _bulletGOs.Add(bullet);
                }
            });
        }

        protected override void EffectEnd()
        {
            base.EffectEnd();
        }

        protected override void BulletTake()
        {
            if (null == _bulletGOs)
                return;

            for (int i = 0; i < _hitPoss.Count; i++)
            {
                _tweeners.Add(_bulletGOs[i].transform.DOMove(_hitPoss[i], 0.18f));
            }
        }

        protected override void BulletEnd()
        {
            foreach (var v in _tweeners)
                v.Kill();
            _tweeners.Clear();
            base.BulletEnd();
        }

        public override bool Dispose()
        {
            foreach (var v in _tweeners)
                v.Kill();
            _tweeners.Clear();

            return base.Dispose();
        }
    }
}
