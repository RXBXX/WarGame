using System;
using UnityEngine;
using UnityEditor;
using DG.Tweening;

namespace WarGame
{
    [Serializable]
    public class HexagonCell
    {
        public string ID;

        public Vector3 coordinate;

        public HexagonCellConfig config;

        private GameObject _gameObject;

        private Tween _tween;

        public GameObject GameObject
        {
            get { return _gameObject; }
        }

        public HexagonCell(string id, HexagonCellConfig config)
        {
            this.ID = id;
            this.config = config;
        }

        public void CreateGameObject()
        {
            DebugManager.Instance.Log("HexagonCell.OnCreate");
            string assetPath = MapTool.Instance.GetHexagonPrefab(config.type);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            _gameObject = GameObject.Instantiate(prefab);
            _gameObject.transform.position = MapTool.Instance.GetPosFromCoor(coordinate);

            _gameObject.GetComponent<HexagonCellData>().ID = this.ID;
        }

        public void OnClick()
        {
            var worldPos = MapTool.Instance.GetPosFromCoor(coordinate);
            Debug.Log(coordinate);
            Debug.Log(worldPos);
            if (null != _tween)
            {
                _tween.Kill();
                _tween = null;
            }

            _tween = _gameObject.transform.DOMoveY(worldPos.y - 0.1F, 0.1F);
            _tween.onComplete = () =>
                  {
                      _tween = _gameObject.transform.DOMoveY(worldPos.y, 0.1F);
                      _tween.onComplete = () =>
                      {
                          _tween.Kill();
                          _tween = null;
                      };
                  };
        }

        /// <summary>
        /// 是否可达
        /// </summary>
        /// <returns></returns>
        public bool IsReachable()
        {
            return config.isReachable;
        }

        /// <summary>
        /// 阻力
        /// </summary>
        /// <returns></returns>
        public float GetCost()
        {
            return 1;
        }

        public void Marking(Enum.MarkType type)
        {
            switch (type)
            {
                case Enum.MarkType.Selected:
                    _gameObject.GetComponent<Renderer>().material.color = Color.blue;
                    break;
                case Enum.MarkType.Walkable:
                    _gameObject.GetComponent<Renderer>().material.color = Color.green;
                    break;
                case Enum.MarkType.Attachable:
                    _gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                    break;
                case Enum.MarkType.Target:
                    _gameObject.GetComponent<Renderer>().material.color = Color.red;
                    break;
                default:
                    _gameObject.GetComponent<Renderer>().material.color = Color.white;
                    break;
            }
        }

        public void SetParent(Transform transform)
        {
            _gameObject.transform.SetParent(transform);
        }

        public void Dispose()
        {
            if (null != _tween)
            {
                _tween.Kill();
                _tween = null;
            }
            if (null != _gameObject)
            {
                GameObject.Destroy(_gameObject);
                _gameObject = null;
            }
        }
    }
}
