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

        protected virtual void SmoothNormal()
        {

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

        //public virtual void SetGrayed(bool isGray, Transform tran = null)
        //{
        //    if (null == tran)
        //        tran = _gameObject.transform;

        //    var childCount = tran.childCount;
        //    for (int i = 0; i < childCount; i++)
        //    {
        //        SetGrayed(isGray, tran.GetChild(i));
        //    }
        //    SkinnedMeshRenderer smr;
        //    if (tran.TryGetComponent<SkinnedMeshRenderer>(out smr))
        //    {
        //        smr.material.SetFloat("_Gray", isGray ? 1 : 0);
        //        return;
        //    }
        //    MeshRenderer mr;
        //    if (tran.TryGetComponent<MeshRenderer>(out mr))
        //    {
        //        mr.material.SetFloat("_Gray", isGray ? 1 : 0);
        //        return;
        //    }
        //}

        public virtual float GetLoadingProgress()
        {
            if (_assetID <= 0)
                return 0;
            return AssetMgr.Instance.GetLoadingProgress(_assetID);
        }

        public void SetParent(Transform transform)
        {
            _parent = transform;
            //_gameObject.transform.SetParent(transform);
        }

        public virtual void Dispose()
        {
            if (null != _gameObject)
            {
                GameObject.Destroy(_gameObject);
                _gameObject = null;
            }
            AssetMgr.Instance.ReleaseAsset(_assetID);
        }
    }
}
