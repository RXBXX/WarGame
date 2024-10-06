using UnityEngine;
using DG.Tweening;
using FairyGUI;
using WarGame.UI;

namespace WarGame
{
    public class Bonfire : MapObject
    {
        private BonfireData _data;

        private Light _pointLight;

        private Vector3 _pointLightPos;

        private float _pointShakeInterval;

        private GameObject _hudPoint;

        private string _hudKey;

        private bool _isFired = false;

        private GameObject _fireGO;

        private int _soundID;

        public Bonfire(BonfireData data)
        {
            _data = data;

            _isFired = _data.IsFired();

            CreateGO();
        }

        public override bool Dispose()
        {
            DOTween.Kill(_pointLight.transform);

            if (null != _hudKey)
            {
                HUDManager.Instance.RemoveHUD(_hudKey);
                _hudKey = null;
            }

            return base.Dispose();
        }

        public int GetID()
        {
            return _data.UID;
        }

        protected override void CreateGO()
        {
            var config = _data.GetConfig();
            _assetID = AssetsMgr.Instance.LoadAssetAsync<GameObject>(config.Prefab, OnCreate);
        }

        protected override void OnCreate(GameObject prefab)
        {
            base.OnCreate(prefab);
            _gameObject.transform.position = MapManager.Instance.GetHexagon(_data.Hexagon).GetPosition() + CommonParams.Offset;
            _gameObject.GetComponent<BonfireBehaviour>().ID = GetID();
            _pointLight = _gameObject.transform.Find("Point Light").GetComponent<Light>();
            _pointLightPos = _pointLight.transform.position;
            _hudPoint = _gameObject.transform.Find("hudPoint").gameObject;
            _fireGO = _gameObject.transform.Find("Fire").gameObject;

            if (Application.isPlaying)
            {
                CreateHUD();
                UpdateFire();
            }
        }

        protected virtual void CreateHUD()
        {
            UIManager.Instance.AddPackage("HUD");
            var uiPanel = _hudPoint.AddComponent<UIPanel>();
            uiPanel.packagePath = "UI/HUD";
            uiPanel.packageName = "HUD";
            uiPanel.componentName = "HUDFire";
            uiPanel.container.renderMode = RenderMode.WorldSpace;
            uiPanel.ui.scale = new Vector2(0.012F, 0.012F);

            var id = GetID();
            _hudKey = id + "_Fire";
            var args = new object[] { id, GetConfig().Duration};
            HUDManager.Instance.AddHUD<HUDFire>("HUDFire", _hudKey, _hudPoint.GetComponent<UIPanel>().ui, _hudPoint, args);
        }

        public void Update(float deltaTime)
        {
            //if (null == SceneMgr.Instance.BattleField)
            //    return;

            //if (null == SceneMgr.Instance.BattleField.weather)
            //    return;

            //var lightIntensity = SceneMgr.Instance.BattleField.weather.GetLightIntensity();
            //_pointLight.intensity = 2 - lightIntensity;
            //_pointLight.range = 4 - lightIntensity;

            if (null != _pointLight)
            {
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

            if (_isFired && !_data.IsFired())
            {
                OutFire();
            }
        }

        public BonfireConfig GetConfig()
        {
            return ConfigMgr.Instance.GetConfig<BonfireConfig>("BonfireConfig", _data.ConfigID);
        }

        public void UpdateRound(int round)
        {
            if (_isFired)
            {
                var hexagon = MapManager.Instance.GetHexagon(GetID());
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

        public void Fire()
        {
            if (_isFired)
                return;

            _isFired = true;
            _data.Fire();
            UpdateFire();
        }

        private void OutFire()
        {
            if (!_isFired)
                return;

            _isFired = false;
            UpdateFire();
        }

        private void UpdateFire()
        {
            _fireGO.SetActive(_isFired);
            _pointLight.enabled = _isFired;

            if (_isFired)
            {
                _soundID = PlaySound("Assets/Audios/Fire.mp3", true);

                if (null != _hudKey)
                {
                    var hud = HUDManager.Instance.GetHUD<HUDFire>(_hudKey);
                    hud.Fire(_data.FiredTime);
                }
            }
            else
            {
                if (0 != _soundID)
                {
                    StopSound(_soundID);
                    _soundID = 0;
                }

                if (null != _hudKey)
                {
                    var hud = HUDManager.Instance.GetHUD<HUDFire>(_hudKey);
                    hud.OutFire();
                }
            }
        }

        public bool CanFire()
        {
            var hexagon = MapManager.Instance.GetHexagon(_data.UID);
            foreach (var v in MapManager.Instance.Dicections)
            {
                var hexagonKey = MapTool.Instance.GetHexagonKey(hexagon.coor + v);
                var roleID = RoleManager.Instance.GetRoleIDByHexagonID(hexagonKey);
                if (0 != roleID)
                {
                    var role = RoleManager.Instance.GetRole(roleID);
                    if (role.Type == Enum.RoleType.Hero)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
