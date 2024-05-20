using DG.Tweening;
using UnityEngine;

namespace WarGame
{
    public class MapObject
    {
        protected int _assetID;

        protected GameObject _gameObject;

        protected int _layer = 0;

        protected virtual void OnCreate(GameObject prefab)
        {
            _gameObject = GameObject.Instantiate(prefab);
            SmoothNormal();
        }

        protected virtual void SmoothNormal()
        {

        }

        public virtual void ChangeToArenaSpace(Vector3 pos, float duration)
        {
            _gameObject.transform.DOMove(pos, duration);
            SetLayer(8);
        }

        public virtual void ChangeToMapSpace()
        {
            RecoverLayer();
        }

        public void SetLayer(int layer)
        {
            SetLayerRecursion(_gameObject.transform, layer);
        }

        public void RecoverLayer()
        {
            SetLayerRecursion(_gameObject.transform, _layer);
        }

        private void SetLayerRecursion(Transform tran, int layer)
        {
            var childCount = tran.childCount;
            for (int i = 0; i < childCount; i++)
            {
                SetLayerRecursion(tran.GetChild(i), layer);
            }
            tran.gameObject.layer = layer;
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
