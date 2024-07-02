using UnityEngine;

namespace WarGame
{
    public class Ornament : MapObject
    {
        private string _id;
        private int _configID;
        private string _hexagonID;
        private float _scale;

        public Ornament(string id, int configID, string hexagonID, float scale)
        {
            this._id = id;
            this._configID = configID;
            this._hexagonID = hexagonID;
            this._scale = scale;

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
            _gameObject.transform.position = MapManager.Instance.GetHexagon(_hexagonID).GetPosition() + CommonParams.Offset;
            _gameObject.transform.localScale = Vector3.one * _scale;
            _gameObject.GetComponent<OrnamentBehaviour>().ID = _id;
            _gameObject.GetComponent<OrnamentBehaviour>().enabled = false;
        }

        private OrnamentConfig GetConfig()
        {
            return ConfigMgr.Instance.GetConfig<OrnamentConfig>("OrnamentConfig", _configID);
        }
    }
}
