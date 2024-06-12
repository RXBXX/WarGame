using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class Equip : MapObject
    {
        protected EquipmentData _data;
        //protected Transform _spinePoint;
        protected Trail _trail;
        protected Transform _spineRoot;
        private int _viceAssetID;
        protected GameObject _viceGameObject;

        public Equip(EquipmentData data, Transform spineRoot)
        {
            this._data = data;
            this._spineRoot = spineRoot;
            _assetID = AssetsMgr.Instance.LoadAssetAsync<GameObject>(GetConfig().Prefab, OnCreate);
            if (null != GetConfig().VicePrefab)
                _viceAssetID = AssetsMgr.Instance.LoadAssetAsync<GameObject>(GetConfig().VicePrefab, OnViceCreate);
        }

        //public virtual void SetSpinePoint(Transform spinePoint)
        //{
        //    _spinePoint = spinePoint;
        //}

        protected override void OnCreate(GameObject prefab)
        {
            base.OnCreate(prefab);

            var equipPlaceConfig = GetPlaceConfig();
            var spinePoint = _spineRoot.transform.Find(equipPlaceConfig.SpinePoint);
            _gameObject.transform.SetParent(spinePoint, false);
            _gameObject.transform.localEulerAngles = GetTypeConfig().Rotation;

            if (_gameObject.TryGetComponent(out _trail))
            {
                _trail.enabled = false;
            }
        }

        protected virtual void OnViceCreate(GameObject prefab)
        {
            _viceGameObject = GameObject.Instantiate(prefab);
            var equipPlaceConfig = GetPlaceConfig();
            var spinePoint = _spineRoot.transform.Find(equipPlaceConfig.ViceSpinePoint);
            _viceGameObject.transform.SetParent(spinePoint, false);
            _viceGameObject.transform.localEulerAngles = GetTypeConfig().ViceRotation;
            Tool.Instance.ApplyProcessingFotOutLine(_viceGameObject);

            if (_viceGameObject.TryGetComponent(out _trail))
            {
                _trail.enabled = false;
            }
        }

        public override bool Dispose()
        {
            if (null != _viceGameObject)
            {
                GameObject.Destroy(_viceGameObject);
                _gameObject = null;
            }
            AssetsMgr.Instance.ReleaseAsset(_viceAssetID);
            return base.Dispose();
        }

        public virtual void Attack(Vector3 targetPos)
        {

        }

        public EquipmentConfig GetConfig()
        {
            return _data.GetConfig();
        }

        public EquipPlaceConfig GetPlaceConfig()
        {
            return _data.GetPlaceConfig();
        }

        public EquipmentTypeConfig GetTypeConfig()
        {
            return _data.GetTypeConfig();
        }

        public Enum.EquipPlace GetPlace()
        {
            return GetTypeConfig().Place;
        }

        //public virtual void Attack()
        //{ }

        //public virtual void AttackEnd()
        //{ }

        protected virtual void EffectTake()
        {
            if (null != _trail)
                _trail.enabled = true;
        }

        protected virtual void EffectEnd()
        {
            if (null != _trail)
                _trail.enabled = false;
        }

        protected virtual void BulletTake()
        { }

        protected virtual void BulletEnd()
        { }

        public virtual void HandleEvent(string firstName, string secondName)
        {
            if ("Effect" == firstName)
            {
                if ("Take" == secondName)
                {
                    EffectTake();
                }
                else if ("End" == secondName)
                {
                    EffectEnd();
                }
            }
            else if ("Bullet" == firstName)
            {
                if ("Take" == secondName)
                    BulletTake();
                else if ("End" == secondName)
                    BulletEnd();
            }
        }
    }
}
