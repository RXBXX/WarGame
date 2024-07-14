using UnityEngine;
using DG.Tweening;

namespace WarGame
{
    public class Hexagon : MapObject
    {
        public int ID;

        private int _configId;

        public WGVector3 coor;

        private Tween _tween;

        private bool isReachable;

        private bool _useDrawMeshInstanced = false;

        public GameObject GameObject
        {
            get { return _gameObject; }
        }

        public int ConfigID
        {
            get { return _configId; }
        }

        public Hexagon(int id, int configId, bool isReachable, WGVector3 coor)
        {
            this.ID = id;
            this._configId = configId;
            this.isReachable = isReachable;
            this.coor = coor;

            CreateGO();
        }
        protected override void CreateGO()
        {
            var config = GetConfig();
            if (IsUseDrawMeshInstanced())
            {
                RenderMgr.Instance.AddMeshInstanced(config.Prefab, GetPosition(), Vector3.one, "", 0, (prefab) =>
                {
                    Tool.Instance.ApplyProcessingFotOutLine(prefab);
                });
            }
            else
            {
                _assetID = AssetsMgr.Instance.LoadAssetAsync<GameObject>(config.Prefab, OnCreate);
            }
        }

        protected override void OnCreate(GameObject prefab)
        {
            base.OnCreate(prefab);
            _gameObject.transform.position = MapTool.Instance.GetPosFromCoor(coor);
            var behaviour = _gameObject.GetComponent<HexagonBehaviour>();
            behaviour.ID = ID;
            behaviour.IsReachable = this.isReachable;
            behaviour.enabled = false;
        }

        public virtual void Update(float deltaTime)
        {

        }

        private bool IsUseDrawMeshInstanced()
        {
            return _useDrawMeshInstanced && !IsReachable() && Application.isPlaying;
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

        public HexagonConfig GetConfig()
        {
            return ConfigMgr.Instance.GetConfig<HexagonConfig>("HexagonConfig", _configId);
        }

        /// <summary>
        /// 是否可达
        /// </summary>
        /// <returns></returns>
        public bool IsReachable()
        {
            return isReachable;
            //return GetConfig().Reachable;
        }

        /// <summary>
        /// 阻力
        /// </summary>
        /// <returns></returns>
        public float GetCost()
        {
            return GetConfig().Resistance;
        }

        public float GetVerticalCost()
        {
            return GetConfig().Resistance;
        }

        public void Marking(Enum.MarkType type)
        {
            if (!IsReachable())
                return;

            switch (type)
            {
                case Enum.MarkType.Selected:
                    //_gameObject.transform.position = MapTool.Instance.GetPosFromCoor(coor) + new Vector3(0, 0.1F, 0);
                    _gameObject.GetComponent<Renderer>().material.SetColor("_OutlineColor", Color.white);
                    //_gameObject.GetComponent<Renderer>().material.SetFloat("_AmbientStrength", 20);
                    break;
                case Enum.MarkType.Walkable:
                    //_gameObject.transform.position = MapTool.Instance.GetPosFromCoor(coor) + new Vector3(0, 0.2F, 0);
                    _gameObject.GetComponent<Renderer>().material.SetColor("_OutlineColor", Color.green);
                    //_gameObject.GetComponent<Renderer>().material.SetFloat("_AmbientStrength", 30);
                    //_gameObject.GetComponent<Renderer>().material.color = new Color(92.0f/255.0f, 135.0f / 255.0f, 153.0f / 255.0f);
                    break;
                case Enum.MarkType.Attackable:
                    //_gameObject.transform.position = MapTool.Instance.GetPosFromCoor(coor) + new Vector3(0, 0.3F, 0);
                    _gameObject.GetComponent<Renderer>().material.SetColor("_OutlineColor", Color.red);
                    //_gameObject.GetComponent<Renderer>().material.SetFloat("_AmbientStrength", 40);
                    //_gameObject.GetComponent<Renderer>().material.color = new Color(226.0f / 255.0f, 186.0f / 255.0f, 42.0f / 255.0f);
                    break;
                default:
                    //_gameObject.transform.position = MapTool.Instance.GetPosFromCoor(coor);
                    _gameObject.GetComponent<Renderer>().material.SetColor("_OutlineColor", Color.black);
                    //_gameObject.GetComponent<Renderer>().material.SetFloat("_AmbientStrength", 0);
                    break;
            }
        }


        public override void ChangeToMapSpace()
        {
            base.ChangeToMapSpace();

            _gameObject.transform.position = MapTool.Instance.GetPosFromCoor(coor);
            _gameObject.transform.rotation = Quaternion.identity;
        }

        public override Vector3 GetPosition()
        {
            return MapTool.Instance.GetPosFromCoor(coor);
            //return _gameObject.transform.position;
        }

        public override float GetLoadingProgress()
        {
            if (IsUseDrawMeshInstanced())
                return 1;
            return base.GetLoadingProgress();
        }

        public void SetForward(Vector3 forward)
        {
            _gameObject.transform.forward = forward;
        }

        public override bool Dispose()
        {
            if (null != _tween)
            {
                _tween.Kill();
                _tween = null;
            }

            if (!IsReachable())
            {
                RenderMgr.Instance.RemoveMeshInstanced(GetConfig().Prefab);
            }

            return base.Dispose();
        }
    }
}
