using UnityEngine;
using DG.Tweening;

namespace WarGame
{
    public class Hexagon:MapObject
    {
        public string ID;

        private int _configId;

        public Vector3 coor;

        private Tween _tween;

        public GameObject GameObject
        {
            get { return _gameObject; }
        }

        public Hexagon(string id, int configId, Vector3 coor)
        {
            this.ID = id;
            this._configId = configId;
            this.coor = coor;

            CreateGO();
        }
        public void CreateGO()
        {
            var config = ConfigMgr.Instance.GetConfig<HexagonConfig>("HexagonConfig", _configId);
            GameObject prefab = AssetMgr.Instance.LoadAsset<GameObject>(config.Prefab);
            _gameObject = GameObject.Instantiate(prefab);


            OnCreate();
        }

        protected override void OnCreate()
        {
            //base.OnCreate();
            _gameObject.transform.position = MapTool.Instance.GetPosFromCoor(coor);
            _gameObject.GetComponent<HexagonBehaviour>().ID = ID;
        }

        public void SetParent(Transform transform)
        {
            _gameObject.transform.SetParent(transform);
        }

        public void OnClick()
        {
            var worldPos = MapTool.Instance.GetPosFromCoor(coor);
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
            var config = ConfigMgr.Instance.GetConfig<HexagonConfig>("HexagonConfig", _configId);
            return config.Reachable;
        }

        /// <summary>
        /// 阻力
        /// </summary>
        /// <returns></returns>
        public float GetCost()
        {
            var config = ConfigMgr.Instance.GetConfig<HexagonConfig>("HexagonConfig", _configId);
            return config.Resistance;
        }

        public void Marking(Enum.MarkType type)
        {
            switch (type)
            {
                case Enum.MarkType.Selected:
                    _gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                    break;
                case Enum.MarkType.Walkable:
                    _gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
                    break;
                case Enum.MarkType.Attackable:
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


        public override void ChangeToMapSpace()
        {
            _gameObject.transform.position = MapTool.Instance.GetPosFromCoor(coor);
        }

        public Vector3 GetPosition()
        {
            return _gameObject.transform.position;
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
