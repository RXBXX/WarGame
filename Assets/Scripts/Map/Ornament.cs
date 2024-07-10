using UnityEngine;

namespace WarGame
{
    public class Ornament : MapObject
    {
        private int _id;
        private int _configID;
        private int _hexagonID;
        private float _scale;
        private Quaternion _rotation;

        public Ornament(int id, int configID, int hexagonID, float scale, Quaternion rotation)
        {
            this._id = id;
            this._configID = configID;
            this._hexagonID = hexagonID;
            this._scale = scale;
            this._rotation = rotation;

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
            if (!MapManager.Instance.ContainHexagon(_hexagonID))
                return;
            _gameObject.transform.position = MapManager.Instance.GetHexagon(_hexagonID).GetPosition() + CommonParams.Offset;
            _gameObject.transform.localScale = Vector3.one * _scale;
            _gameObject.transform.localRotation = _rotation;
            _gameObject.GetComponent<OrnamentBehaviour>().ID = _id;
            _gameObject.GetComponent<OrnamentBehaviour>().enabled = false;
        }

        private OrnamentConfig GetConfig()
        {
            return ConfigMgr.Instance.GetConfig<OrnamentConfig>("OrnamentConfig", _configID);
        }
    }
}
