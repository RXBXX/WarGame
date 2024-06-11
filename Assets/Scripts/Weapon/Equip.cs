using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class Equip : MapObject
    {
        protected EquipmentData _data;
        protected Transform _spinePoint;
        protected Trail _trail;

        public Equip(EquipmentData data)
        {
            this._data = data;
            _assetID = AssetsMgr.Instance.LoadAssetAsync<GameObject>(GetConfig().Prefab, OnCreate);
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

            if (_gameObject.TryGetComponent(out _trail))
            {
                _trail.enabled = false;
            }
        }

        public virtual void Attack(Vector3 targetPos)
        {
        
        }

        public EquipmentConfig GetConfig()
        {
            return _data.GetConfig();
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
        
        }

        protected virtual void EffectEnd()
        {

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
        }
    }
}
