using DG.Tweening;
using UnityEngine;

namespace WarGame
{
    public class MapObject
    {
        protected int _assetID;

        protected GameObject _gameObject;

        protected Enum.Layer _layer = Enum.Layer.Default;

        protected Transform _parent;

        protected virtual void CreateGO()
        {
        }

        protected virtual void OnCreate(GameObject prefab)
        {
            _gameObject = GameObject.Instantiate(prefab);
            _gameObject.transform.SetParent(_parent);
            SmoothNormal();
        }

        public bool IsCreated()
        {
            return null != _gameObject;
        }

        protected virtual void SmoothNormal()
        {
            Tool.Instance.ApplyProcessingFotOutLine(_gameObject);
        }

        public virtual Tweener ChangeToArenaSpace(Vector3 pos, float duration)
        {
            SetLayer(Enum.Layer.Gray);
            return _gameObject.transform.DOMove(pos, duration);
        }

        public virtual void ChangeToMapSpace()
        {
            RecoverLayer();
        }

        public void SetLayer(Enum.Layer layer)
        {
            Tool.SetLayer(_gameObject.transform, layer);
        }

        public void RecoverLayer()
        {
            Tool.SetLayer(_gameObject.transform, _layer);
        }

        public virtual void HighLight() { }

        public virtual void ResetHighLight() { }

        public virtual float GetLoadingProgress()
        {
            if (_assetID <= 0)
                return 0;
            return AssetsMgr.Instance.GetLoadingProgress(_assetID);
        }

        public void SetParent(Transform transform)
        {
            _parent = transform;
            //_gameObject.transform.SetParent(transform);
        }

        public virtual Vector3 GetPosition()
        {
            return Vector3.zero;
        }

        //是否在屏幕视野内
        public bool InScreen()
        {
            var screenPos = CameraMgr.Instance.MainCamera.WorldToScreenPoint(GetPosition());
            if (screenPos.x < 0 || screenPos.y < 0 || screenPos.x > Screen.width || screenPos.y > Screen.height)
                return false;
            return true;
        }

        public virtual bool Dispose()
        {
            if (null != _gameObject)
            {
                GameObject.Destroy(_gameObject);
                _gameObject = null;
            }
            AssetsMgr.Instance.ReleaseAsset(_assetID);
            return true;
        }
    }
}
