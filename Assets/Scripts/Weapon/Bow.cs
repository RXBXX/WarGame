using UnityEngine;
using DG.Tweening;

namespace WarGame
{
    public class Bow : Equip
    {
        private Animator _bowAnimator;
        private Animator _arrowAnimator;
        private Tween _tweener;

        public Bow(EquipmentData data, Transform spineRoot) : base(data, spineRoot)
        { }

        protected override void OnCreate(GameObject prefab)
        {
            base.OnCreate(prefab);
            _bowAnimator = _gameObject.GetComponent<Animator>();
        }

        protected override void OnViceCreate(GameObject prefab)
        {
            base.OnViceCreate(prefab);
            _arrowAnimator = _viceGO.GetComponent<Animator>();
        }

        public override void Attack(Vector3 targetPos)
        {
            //DebugManager.Instance.Log("Attack");
            _targetPos = targetPos;
            if (null != _bowAnimator)
                _bowAnimator.Play("Attack");
            if (null != _arrowAnimator)
                _arrowAnimator.Play("Attack");
        }

        protected override void BulletTake()
        {
            var timeDic = Tool.Instance.GetEventTimeForAnimClip(_arrowAnimator, "Attack01_Combo0102_Arrow");
            var duration = timeDic["Bullet_End"] - timeDic["Bullet_Take"];

            _bulletAssetID = AssetsMgr.Instance.LoadAssetAsync<GameObject>(GetConfig().Bullet, (prefab) =>
            {
                _bulletGO = GameObject.Instantiate(prefab);
                Trail trail;
                if (_bulletGO.TryGetComponent<Trail>(out trail))
                {
                    trail.enabled = true;
                }
                _bulletGO.transform.position = _gameObject.transform.position;
                var forward = _targetPos - _gameObject.transform.position;
                _bulletGO.transform.forward = forward;
                var tp = _targetPos - forward.normalized;
                _tweener = _bulletGO.transform.DOMove(tp, duration);
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
    }
}

