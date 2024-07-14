using UnityEngine;

namespace WarGame
{
    public class Ornament : MapObject
    {
        private int _id;
        private int _configID;
        private int _hexagonID;
        private float _scale;
        private WGVector3 _rotation;

        public Ornament(int id, int configID, int hexagonID, float scale, WGVector3 rotation)
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
            _gameObject.transform.localRotation =  Quaternion.Euler(new Vector3(_rotation.x, _rotation.y, _rotation.z));
            var ob = _gameObject.GetComponent<OrnamentBehaviour>();
            ob.ID = _id;
            ob.enabled = false;

            var particles = _gameObject.GetComponentsInChildren<ParticleSystem>();
            foreach (var v in particles)
                v.Play();
        }

        private OrnamentConfig GetConfig()
        {
            return ConfigMgr.Instance.GetConfig<OrnamentConfig>("OrnamentConfig", _configID);
        }
    }
}
