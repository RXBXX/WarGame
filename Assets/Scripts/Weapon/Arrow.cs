using UnityEngine;
using DG.Tweening;

namespace WarGame
{
    public class Arrow : Equip
    {
        private Animator _animator;
        private GameObject _bullet;
        private Vector3 _targetPos;

        public Arrow(EquipmentData data) : base(data)
        {
           
        }

        protected override void OnCreate(GameObject prefab)
        {
            base.OnCreate(prefab);
            _animator = _gameObject.GetComponent<Animator>();
        }

        public override void Attack(Vector3 targetPos)
        {
            _targetPos = targetPos;
            _animator.Play("Attack");
        }

        protected override void EffectTake()
        {
            var timeDic = Tool.Instance.GetEventTimeForAnimClip(_animator, "Attack01_Combo0102_Arrow");
            var duration = timeDic["Bullet_End"] - timeDic["Bullet_Take"];

            _bullet = GameObject.Instantiate(_gameObject);
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

        protected override void EffectEnd()
        {
            DOTween.Kill(_bullet.transform);
            GameObject.Destroy(_bullet);
        }
    }
}

