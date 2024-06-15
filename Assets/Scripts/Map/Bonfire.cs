using UnityEngine;
using DG.Tweening;

namespace WarGame
{
    public class Bonfire : MapObject
    {
        public int ID;

        private int _configId;

        private string _hexagonID;

        private Vector3 _offset = new Vector3(0, 0.4F, 0);

        private Light _pointLight;

        private Vector3 _pointLightPos;

        private float _pointShakeInterval;

        public GameObject GameObject
        {
            get { return _gameObject; }
        }

        public int ConfigID
        {
            get { return _configId; }
        }

        public Bonfire(int id, int configId, string hexagonID)
        {
            this.ID = id;
            this._configId = configId;
            this._hexagonID = hexagonID;

            CreateGO();
        }

        public override bool Dispose()
        {
            DOTween.Kill(_pointLight.transform);

            return base.Dispose();
        }

        protected override void CreateGO()
        {
            var config = ConfigMgr.Instance.GetConfig<BonfireConfig>("BonfireConfig", _configId);
            _assetID = AssetsMgr.Instance.LoadAssetAsync<GameObject>(config.Prefab, OnCreate);
        }

        protected override void OnCreate(GameObject prefab)
        {
            base.OnCreate(prefab);
            _gameObject.transform.position = MapManager.Instance.GetHexagon(_hexagonID).GetPosition() + _offset;
            _gameObject.GetComponent<BonfireBehaviour>().ID = _configId;
            _pointLight = _gameObject.transform.Find("Point Light").GetComponent<Light>();
            _pointLightPos = _pointLight.transform.position;
        }

        public void Update(float deltaTime)
        {
            if (null == _pointLight)
                return;

            if (null == SceneMgr.Instance.BattleField)
                return;

            if (null == SceneMgr.Instance.BattleField.weather)
                return;

            var intensity = 1 - SceneMgr.Instance.BattleField.weather.GetLightIntensity() + 3F; ///Error Trace
            _pointLight.intensity = intensity;
            _pointLight.range = intensity;

            if (_pointShakeInterval <= 0)
            {
                _pointShakeInterval = Random.Range(0, 0.3F);
                if (_pointLight.transform.position == _pointLightPos)
                {
                    _pointLight.transform.DOMove(_pointLight.transform.position + new Vector3(Random.Range(-0.1F, 0.1F), Random.Range(-0.1F, 0.1F), Random.Range(-0.1F, 0.1F)), _pointShakeInterval);
                }
                else
                {
                    _pointLight.transform.DOMove(_pointLightPos, _pointShakeInterval);
                }
                }
                _pointShakeInterval -= deltaTime;
        }

        public BonfireConfig GetConfig()
        {
            return ConfigMgr.Instance.GetConfig<BonfireConfig>("BonfireConfig", _configId);
        }

        public void UpdateRound(int round)
        {
            var hexagon = MapManager.Instance.GetHexagon(_hexagonID);
            foreach (var v in MapManager.Instance.Dicections)
            {
                var hexagonKey = MapTool.Instance.GetHexagonKey(hexagon.coor + v);
                var roleID = RoleManager.Instance.GetRoleIDByHexagonID(hexagonKey);
                if (0 != roleID)
                {
                    var role = RoleManager.Instance.GetRole(roleID);
                    role.AddEffects(GetConfig().Effects);
                }
            }
        }
    }
}
