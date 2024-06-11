using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class LocatingArrow : MapObject
    {
        public bool Active
        {
            get { return _gameObject.activeSelf; }
            set { _gameObject.SetActive(value); }
        }

        public Vector3 Position
        {
            set { _gameObject.transform.position = value; }
        }

        public LocatingArrow()
        {
            _assetID = AssetsMgr.Instance.LoadAssetAsync<GameObject>("Assets/Prefabs/Effects/Arrow.prefab", OnCreate);
        }

        protected override void SmoothNormal()
        {
            Tool.Instance.ApplyProcessingFotOutLine(_gameObject);
        }
    }
}
