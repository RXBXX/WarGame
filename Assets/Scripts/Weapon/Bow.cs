using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

namespace WarGame
{
    public class Bow : Equip
    {
        private Animator _bowAnimator;
        private Animator _arrowAnimator;
        private List<Tweener> _tweeners = new List<Tweener>();

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

        public override void Attack(List<Vector3> hitPoss)
        {
            _hitPoss = hitPoss;
            if (null != _bowAnimator)
                _bowAnimator.Play("Attack");
            if (null != _arrowAnimator)
                _arrowAnimator.Play("Attack");

            _bulletAssetID = AssetsMgr.Instance.LoadAssetAsync<GameObject>(GetConfig().Bullet, (GameObject prefab) =>
            {
                for (int i = 0; i < _hitPoss.Count; i++)
                {
                    var bullet = GameObject.Instantiate(prefab);
                    _bulletGOs.Add(bullet);
                    Tool.SetLayer(bullet.transform, Enum.Layer.Gray);
                    bullet.SetActive(false);
                }
            });

            AudioMgr.Instance.PlaySound("bow_start.mp3");
    }

        protected override void BulletTake()
        {
            if (null == _bulletGOs)
                return;

            var timeDic = Tool.Instance.GetEventTimeForAnimClip(_arrowAnimator, "Attack01_Combo0102_Arrow");
            var duration = timeDic["Bullet_End"] - timeDic["Bullet_Take"];

            for (int i = 0; i < _hitPoss.Count; i++)
            {
                var bullet = _bulletGOs[i];
                bullet.SetActive(true);
                bullet.transform.position = _viceGO.transform.position;
                bullet.transform.localScale = _viceGO.transform.lossyScale;
                bullet.transform.rotation = _viceGO.transform.rotation;
                foreach (var v in bullet.GetComponentsInChildren<TrailRenderer>())
                {
                    v.enabled = true;
                }
                var forward = (_hitPoss[i] - _gameObject.transform.position).normalized;
                bullet.transform.forward = forward;
                _tweeners.Add(bullet.transform.DOMove(_hitPoss[i] - _viceGO.transform.lossyScale.x * forward, duration));
            }

            AudioMgr.Instance.PlaySound("bow_take.wav");
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

