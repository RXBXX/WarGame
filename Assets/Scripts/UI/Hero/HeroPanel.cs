using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using UnityEditor;

namespace WarGame.UI
{
    public class HeroPanel : UIBase
    {
        private Transform _heroRoot;
        private Vector2 _touchPos;
        private GGraph _touchArena;
        private int[] _roles;
        private int _roleIndex = 0;
        private Dictionary<int, GameObject> _rolesGO = new Dictionary<int, GameObject>();
        private float _dragingPower = 0.0f;
        private float _rotateSpeed = 50.0f;
        private float _lastDragTime = 0.0f;
        private bool _draging = false;
        private int[] _equipments;
        private GButton _commonSkill;
        private GButton _specialSkill;
        private GTextField _name;


        public HeroPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _touchArena = (GGraph)_gCom.GetChild("touchArena");
            _name = (GTextField)_gCom.GetChild("name");

            _gCom.GetChild("closeBtn").onClick.Add(OnClickClose);

            _gCom.onTouchBegin.Add(OnTouchBegin);
            _gCom.onTouchMove.Add(OnTouchMove);
            _gCom.onTouchEnd.Add(OnTouchEnd);

            _gCom.GetChild("rightBtn").onClick.Add(() => { OnClickArrow(1); });
            _gCom.GetChild("leftBtn").onClick.Add(() => { OnClickArrow(-1); });
            var equipmentList = (GList)_gCom.GetChild("equipmentList");
            equipmentList.itemRenderer = OnEquipmentRender;

            _roles = DatasMgr.Instance.GetAllRole();
            _equipments = DatasMgr.Instance.GetAllEquipment();
            equipmentList.numItems = _equipments.Length;

            LoadHero(_roles[_roleIndex]);

            _commonSkill = (GButton)_gCom.GetChild("commonSkill");
            _specialSkill = (GButton)_gCom.GetChild("specialSkill");
            UpdateSkills(_roles[_roleIndex]);
        }

        public override void Update(float deltaTime)
        {
            if (_draging)
                return;

            if (Mathf.Abs(_dragingPower) < 0.1F)
            {
                _dragingPower = 0;
                return;
            }

            _heroRoot.Rotate(Vector3.up, _rotateSpeed * deltaTime * _dragingPower);
            _dragingPower = Mathf.Lerp(_dragingPower, 0, deltaTime);// (_dragingPower / Mathf.Abs(_dragingPower)) * 5;
        }

        private void OnClickArrow(int dir)
        {
            if (_rolesGO.ContainsKey(_roles[_roleIndex]))
                _rolesGO[_roles[_roleIndex]].SetActive(false);

            _roleIndex = (_roleIndex + 1) % _roles.Length;

            LoadHero(_roles[_roleIndex]);
            UpdateSkills(_roles[_roleIndex]);
        }

        private void OnTouchBegin(EventContext context)
        {
            if (Stage.inst.touchTarget != _touchArena.displayObject)
                return;
            context.CaptureTouch();
            _dragingPower = 0;
            _touchPos = context.inputEvent.position;
            _lastDragTime = Time.time;
            _draging = true;
        }

        private void OnTouchMove(EventContext context)
        {
            _dragingPower = _touchPos.x - context.inputEvent.position.x;
            _heroRoot.Rotate(Vector3.up, _rotateSpeed * _dragingPower * (Time.time - _lastDragTime));
            _touchPos = context.inputEvent.position;
            _lastDragTime = Time.time;
        }

        public void OnTouchEnd(EventContext context)
        {
            _draging = false;
        }

        private void OnClickClose()
        {
            SceneMgr.Instance.CloseHeroScene();
        }

        private void LoadHero(int uid)
        {
            var roleData = DatasMgr.Instance.GetRoleData(uid);
            var heroConfing = ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", roleData.configId);
            _name.text = heroConfing.Name;

            GameObject hero = null;
            if (!_rolesGO.ContainsKey(uid))
            {
                var prefab = AssetMgr.Instance.LoadAsset<GameObject>(heroConfing.Prefab);
                hero = GameObject.Instantiate(prefab);
                _heroRoot = SceneMgr.Instance.GetHeroRoot();
                hero.transform.SetParent(_heroRoot);
                hero.transform.localPosition = Vector3.zero;
                _rolesGO.Add(uid, hero);
            }

            _rolesGO[uid].SetActive(true);
        }

        private void OnEquipmentRender(int index, GObject item)
        {
            var equipmentData = DatasMgr.Instance.GetEquipmentData(_equipments[index]);
            ((GButton)item).title = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", equipmentData.configId).Name;
        }

        private void UpdateSkills(int roleUID)
        {
            var roleData = DatasMgr.Instance.GetRoleData(roleUID);
            var roleConfig = ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", roleData.configId);
            DebugManager.Instance.Log(roleConfig.ID);
            var commonSkillConfig = ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", roleConfig.CommonSkill.id);
            _commonSkill.title = commonSkillConfig.Name;
            var specialSkillConfig = ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", roleConfig.SpecialSkill.id);
            _specialSkill.title = specialSkillConfig.Name;
        }

        public override void Dispose(bool disposeGCom = false)
        {
            base.Dispose(disposeGCom);

            foreach (var v in _rolesGO)
            {
                GameObject.Destroy(v.Value);
            }
            _rolesGO.Clear();
            _roles = null;
        }
    }
}
