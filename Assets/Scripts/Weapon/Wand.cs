using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace WarGame
{
    public class Wand : Equip
    {
        private GameObject _prefab;
        private Tweener _tweener;

        public Wand(EquipmentData data) : base(data)
        { }

        public override void Attack(Vector3 targetPos)
        {
            AssetMgr.Instance.LoadAssetAsync<GameObject>("Assets/Prefabs/Effects/WandEffect.prefab", (GameObject prefab) =>{
                _prefab = GameObject.Instantiate(_prefab);
                var spinePoint = _gameObject.transform.Find("spinePoint");
                _prefab.transform.position = spinePoint.position;
                _tweener = _prefab.transform.DOMove(targetPos, 0.5f);
                _tweener.OnComplete(() => { OnAttackOver(); });
            });
        }

        public void OnAttackOver()
        {
            AssetMgr.Instance.Destroy(_prefab);
        }

        public override void Dispose()
        {
            if (null != _tweener)
            {
                _tweener.Kill();
                _tweener = null;
            }
            AssetMgr.Instance.Destroy(_prefab);
        }
    }
}
