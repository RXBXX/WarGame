using UnityEngine;

namespace WarGame
{
    public class Ornament : MapObject
    {
        private string _id;
        private int _configID;
        private string _hexagonID;

        public Ornament(string id, int configID, string hexagonID)
        {
            this._id = id;
            this._configID = configID;
            this._hexagonID = hexagonID;

            CreateGO();
        }

        protected override void CreateGO()
        {
            var config = GetConfig();
            _assetID = AssetsMgr.Instance.LoadAssetAsync<GameObject>(config.Prefab, OnCreate);
        }

        protected override void OnCreate(GameObject prefab)
        {
            base.OnCreate(prefab);
            DebugManager.Instance.Log(_configID+"_"+_hexagonID);
            _gameObject.transform.position = MapManager.Instance.GetHexagon(_hexagonID).GetPosition();
            _gameObject.GetComponent<OrnamentBehaviour>().ID = _id;
            _gameObject.GetComponent<OrnamentBehaviour>().enabled = false;
        }

        private OrnamentConfig GetConfig()
        {
            return ConfigMgr.Instance.GetConfig<OrnamentConfig>("OrnamentConfig", _configID);
        }
    }
}
