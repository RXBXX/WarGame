using System;
using UnityEngine;
using UnityEditor;
using DG.Tweening;

namespace WarGame
{
    [Serializable]
    public class HexagonCell
    {
        public int id;

        public Vector3 position;

        public HexagonCellConfig config;

        public GameObject gameObject;

        private Tween _tween;

        public HexagonCell(int id, HexagonCellConfig config)
        {
            this.id = id;
            this.config = config;
        }

        public GameObject CreateGameObject()
        {
            string assetPath = MapTool.Instance.GetHexagonPrefab(config.type);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            gameObject = GameObject.Instantiate(prefab);
            gameObject.transform.position = MapTool.Instance.FromCellPosToWorldPos(position);
            return gameObject;
        }

        public void OnClick()
        {
            var worldPos = MapTool.Instance.FromCellPosToWorldPos(position);
            Debug.Log(position);
            Debug.Log(worldPos);
            if (null != _tween)
            {
                _tween.Kill();
                _tween = null;
            }

            _tween = gameObject.transform.DOMoveY(worldPos.y - 0.1F, 0.1F);
            _tween.onComplete = () =>
                  {
                      _tween = gameObject.transform.DOMoveY(worldPos.y, 0.1F);
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
                    gameObject.GetComponent<Renderer>().material.color = Color.blue;
                    break;
                case Enum.MarkType.Walkable:
                    gameObject.GetComponent<Renderer>().material.color = Color.green;
                    break;
                case Enum.MarkType.Attachable:
                    gameObject.GetComponent<Renderer>().material.color = Color.yellow;
                    break;
                case Enum.MarkType.Target:
                    gameObject.GetComponent<Renderer>().material.color = Color.red;
                    break;
                default:
                    gameObject.GetComponent<Renderer>().material.color = Color.white;
                    break;
            }
        }

        public void Dispose()
        {
            if (null != _tween)
            {
                _tween.Kill();
                _tween = null;
            }
            if (null != gameObject)
            {
                GameObject.Destroy(gameObject);
                gameObject = null;
            }
        }
    }
}
