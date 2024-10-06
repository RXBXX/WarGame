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

        private AudioSource _audioSource;

        private int _soundAssetID;

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

        public int PlaySound(string sound, bool isLoop = false, float minDistance = 6)
        {
            return AudioMgr.Instance.PlaySound(sound, isLoop, _gameObject, 1.0f, minDistance);
            //if (0 != _soundAssetID)
            //{
            //    AssetsMgr.Instance.ReleaseAsset(_soundAssetID);
            //    _soundAssetID = 0;
            //}

            //if (null == _audioSource)
            //{
            //    _audioSource = _gameObject.AddComponent<AudioSource>();
            //    _audioSource.minDistance = 6;
            //    _audioSource.spatialBlend = 1.0F;
            //}
            //AssetsMgr.Instance.LoadAssetAsync<AudioClip>(sound, (clip) => {
            //    _audioSource.clip = clip;
            //    _audioSource.loop = isLoop;
            //    _audioSource.Play();
            //});
        }

        public void StopSound(int soundID)
        {
            if (0 == soundID)
                return;
            AudioMgr.Instance.StopSound(soundID);
            //if (0 != _soundAssetID)
            //{
            //    AssetsMgr.Instance.ReleaseAsset(_soundAssetID);
            //    _soundAssetID = 0;
            //}

            //if (null == _audioSource)
            //    return;
            //_audioSource.Stop();
        }

        public virtual bool Dispose()
        {
            AudioMgr.Instance.ClearSound(_gameObject);

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
