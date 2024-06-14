using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace WarGame
{
    public class Wand : Equip
    {
        private Tweener _tweener;

        public Wand(EquipmentData data, Transform spineRoot) : base(data, spineRoot)
        { }

        public override void Attack(Vector3 targetPos)
        {

        }

        public void OnAttackOver()
        {
        }

        protected override void EffectTake()
        {
            DebugManager.Instance.Log(GetConfig().Effect);
            _effectAssetID = AssetsMgr.Instance.LoadAssetAsync<GameObject>(GetConfig().Effect, (prefab) =>
            {
                _effectGO = GameObject.Instantiate(prefab);
                _effectGO.transform.SetParent(_gameObject.transform.Find("spinePoint"));
                _effectGO.transform.localPosition = Vector3.zero;
            });
        }

        protected override void EffectEnd()
        {
            if (null != _tweener)
            {
                _tweener.Kill();
                _tweener = null;
            }
            base.EffectEnd();
        }

        protected override void BulletTake()
        {
            _bulletAssetID = AssetsMgr.Instance.LoadAssetAsync<GameObject>(GetConfig().Bullet, (GameObject prefab) => {
                _bulletGO = GameObject.Instantiate(prefab);
                var spinePoint = _gameObject.transform.Find("spinePoint");
                _bulletGO.transform.position = _gameObject.transform.Find("spinePoint").position;
                _tweener = _bulletGO.transform.DOMove(_targetPos, 0.5f);
            });
        }

        protected override void BulletEnd()
        {
            if (null != _tweener)
            {
                _tweener.Kill();
                _tweener = null;
            }
            base.BulletEnd();
        }

        public override bool Dispose()
        {
            if (null != _tweener)
            {
                _tweener.Kill();
                _tweener = null;
            }

            return base.Dispose();
        }
    }
}
