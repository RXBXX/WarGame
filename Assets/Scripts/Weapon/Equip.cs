using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class Equip
    {
        protected EquipmentData _data;
        protected GameObject _gameObject;

        public Equip(EquipmentData data)
        {
            this._data = data;
        }

        public virtual void SetSpinePoint(Transform spinePoint)
        {
            var config = GetConfig();
            _gameObject = GameObject.Instantiate<GameObject>(AssetMgr.Instance.LoadAsset<GameObject>(config.Prefab));
            _gameObject.transform.SetParent(spinePoint, false);
            _gameObject.transform.localEulerAngles = config.Rotation;

            OnCreate();
        }

        public virtual void OnCreate()
        { 
        
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

        public virtual void Dispose()
        { }
    }
}
