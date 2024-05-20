using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class Equip : MapObject
    {
        protected EquipmentData _data;
        protected Transform _spinePoint;

        public Equip(EquipmentData data)
        {
            this._data = data;
            _assetID = AssetMgr.Instance.LoadAssetAsync<GameObject>(GetConfig().Prefab, OnCreate);
        }

        public virtual void SetSpinePoint(Transform spinePoint)
        {
            _spinePoint = spinePoint;
        }

        protected override void OnCreate(GameObject prefab)
        {
            base.OnCreate(prefab);
            var config = GetConfig();
            _gameObject.transform.SetParent(_spinePoint, false);
            _gameObject.transform.localEulerAngles = config.Rotation;
            Tool.Instance.ApplyProcessingFotOutLine(_gameObject);
        }

        public virtual void Attack(Vector3 targetPos)
        {
        
        }

        public EquipmentConfig GetConfig()
        {
            return ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", _data.configId);
        }


        public EquipmentTypeConfig GetTypeConfig()
        {
            var config = GetConfig();
            return ConfigMgr.Instance.GetConfig<EquipmentTypeConfig>("EquipmentTypeConfig", (int)config.Type);
        }

        public Enum.EquipPlace GetPlace()
        {
            return GetTypeConfig().Place;
        }

        protected virtual void SpawnBullet()
        { 
        
        }

        protected virtual void DestroyBullet()
        {

        }

        public virtual void HandleEvent(string firstName, string secondName)
        {
            if ("Bullet" == firstName)
            {
                if ("Take" == secondName)
                    SpawnBullet();
                else if ("End" == secondName)
                    DestroyBullet();
            }
        }
    }
}
