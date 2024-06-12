using UnityEngine;
using DG.Tweening;

namespace WarGame
{
    public class Bow : Equip
    {
        private Animator _bowAnimator;
        private Animator _arrowAnimator;
        private GameObject _bullet;
        private Vector3 _targetPos;

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
            _arrowAnimator = _viceGameObject.GetComponent<Animator>();
        }

        public override void Attack(Vector3 targetPos)
        {
            DebugManager.Instance.Log("Attack");
            _targetPos = targetPos;
            if (null != _bowAnimator)
                _bowAnimator.Play("Attack");
            if (null != _arrowAnimator)
                _arrowAnimator.Play("Attack");
        }

        protected override void BulletTake()
        {
            DebugManager.Instance.Log("EffectTake");
            var timeDic = Tool.Instance.GetEventTimeForAnimClip(_arrowAnimator, "Attack01_Combo0102_Arrow");
            var duration = timeDic["Bullet_End"] - timeDic["Bullet_Take"];

            _bullet = GameObject.Instantiate(_viceGameObject);
            Trail trail;
            if (_bullet.TryGetComponent<Trail>(out trail))
            {
                trail.enabled = true;
            }
            _bullet.transform.position = _gameObject.transform.position;
            var forward = _targetPos - _gameObject.transform.position;
            _bullet.transform.forward = forward;
            var tp = _targetPos - forward.normalized;
            _bullet.transform.DOMove(tp, duration);
        }

        protected override void BulletEnd()
        {
            DebugManager.Instance.Log("EffectEnd");
            DOTween.Kill(_bullet.transform);
            GameObject.Destroy(_bullet);
            _bullet = null;
        }
    }
}

