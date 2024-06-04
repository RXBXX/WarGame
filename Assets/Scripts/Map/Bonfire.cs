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
        protected override void CreateGO()
        {
            var config = ConfigMgr.Instance.GetConfig<BonfireConfig>("BonfireConfig", _configId);
            _assetID = AssetMgr.Instance.LoadAssetAsync<GameObject>(config.Prefab, OnCreate);
        }

        protected override void OnCreate(GameObject prefab)
        {
            _gameObject = GameObject.Instantiate(prefab);
            _gameObject.transform.SetParent(_parent);
            _gameObject.transform.position = MapManager.Instance.GetHexagon(_hexagonID).GetPosition() + _offset;
            _gameObject.GetComponent<BonfireBehaviour>().ID = _configId;
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

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
