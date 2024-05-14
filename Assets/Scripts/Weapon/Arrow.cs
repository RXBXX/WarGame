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

        public override void OnCreate()
        {
            _animator = _gameObject.GetComponent<Animator>();
        }

        public override void Attack(Vector3 targetPos)
        {
            _targetPos = targetPos;
            _animator.Play("Attack");
        }

        protected override void SpawnBullet()
        {
            var timeDic = Tool.Instance.GetEventTimeForAnimClip(_animator, "Attack01_Combo0102_Arrow");
            var duration = timeDic["Bullet_End"] - timeDic["Bullet_Take"];

            _bullet = GameObject.Instantiate(_gameObject);
            _bullet.transform.position = _gameObject.transform.position;
            _bullet.transform.forward = _targetPos - _gameObject.transform.position;
            _bullet.transform.DOMove(_targetPos, duration);

        }

        protected override void DestroyBullet()
        {
            DOTween.Kill(_bullet);
            GameObject.Destroy(_bullet);
        }
    }
}

