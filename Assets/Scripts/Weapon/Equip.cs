using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class Equip : MapObject
    {
        protected EquipmentData _data;
        protected Trail _trail;
        protected Transform _spineRoot;
        private int _viceAssetID;
        protected GameObject _viceGO;
        protected int _effectAssetID;
        protected GameObject _effectGO;
        protected int _bulletAssetID;
        protected GameObject _bulletGO;
        protected Vector3 _targetPos;

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
            _gameObject.transform.localPosition = Vector3.zero;
            _gameObject.transform.localEulerAngles = GetTypeConfig().Rotation;

            if (_gameObject.TryGetComponent(out _trail))
            {
                _trail.enabled = false;
            }
        }

        protected virtual void OnViceCreate(GameObject prefab)
        {
            _viceGO = GameObject.Instantiate(prefab);
            var equipPlaceConfig = GetPlaceConfig();
            var spinePoint = _spineRoot.transform.Find(equipPlaceConfig.ViceSpinePoint);
            _viceGO.transform.SetParent(spinePoint, false);
            _viceGO.transform.localPosition = Vector3.zero;
            _viceGO.transform.localEulerAngles = GetTypeConfig().ViceRotation;
            Tool.Instance.ApplyProcessingFotOutLine(_viceGO);

            if (_viceGO.TryGetComponent(out _trail))
            {
                _trail.enabled = false;
            }
        }

        public override bool Dispose()
        {
            if (null != _viceGO)
            {
                GameObject.Destroy(_viceGO);
                _gameObject = null;
            }
            AssetsMgr.Instance.ReleaseAsset(_viceAssetID);

            if (null != _effectGO)
            {
                GameObject.Destroy(_effectGO);
                _effectGO = null;
            }
            AssetsMgr.Instance.ReleaseAsset(_effectAssetID);

            if (null != _bulletGO)
            {
                GameObject.Destroy(_bulletGO);
                _bulletGO = null;
            }
            AssetsMgr.Instance.ReleaseAsset(_bulletAssetID);

            return base.Dispose();
        }

        public virtual void Attack(Vector3 targetPos)
        {
            _targetPos = targetPos;
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

        protected virtual void EffectTake()
        {
            if (null != _trail)
                _trail.enabled = true;
        }

        protected virtual void EffectEnd()
        {
            if (null != _trail)
                _trail.enabled = false;

            if (null != _effectGO)
            {
                GameObject.Destroy(_effectGO);
                _effectGO = null;
            }
            AssetsMgr.Instance.ReleaseAsset(_effectAssetID);
        }

        protected virtual void BulletTake()
        { 
        
        }

        protected virtual void BulletEnd()
        {
            if (null != _bulletGO)
            {
                GameObject.Destroy(_bulletGO);
                _bulletGO = null;
            }
            AssetsMgr.Instance.ReleaseAsset(_bulletAssetID);
        }

        public string GetAttackEffect()
        {
            return GetConfig().HitEffect;
        }

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
