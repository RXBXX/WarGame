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
        private HeroProComp _proComp;
        private HeroEquipComp _equipComp;
        private HeroSkillComp _skillComp;
        private Controller _stateC;

        public HeroPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _stateC = _gCom.GetController("state");
            _touchArena = (GGraph)_gCom.GetChild("touchArena");
            _gCom.GetChild("closeBtn").onClick.Add(OnClickClose);

            _gCom.onTouchBegin.Add(OnTouchBegin);
            _gCom.onTouchMove.Add(OnTouchMove);
            _gCom.onTouchEnd.Add(OnTouchEnd);

            _gCom.GetChild("rightBtn").onClick.Add(() => { OnClickArrow(1); });
            _gCom.GetChild("leftBtn").onClick.Add(() => { OnClickArrow(-1); });

            _roles = DatasMgr.Instance.GetAllRoles();

            LoadHero(_roles[_roleIndex]);

            _proComp = GetChild<HeroProComp>("proComp");
            _equipComp = GetChild<HeroEquipComp>("equipComp");
            _skillComp = GetChild<HeroSkillComp>("skillComp");

            var roleData = DatasMgr.Instance.GetRoleData(_roles[_roleIndex]);
            var roleConfig = ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", roleData.configId);
            _proComp.UpdateComp(roleConfig.Name, "");
            _equipComp.UpdateComp(roleConfig.Job);

            EventDispatcher.Instance.AddListener(Enum.EventType.Hero_Open_Equip, OnOpenEquip);
            EventDispatcher.Instance.AddListener(Enum.EventType.Hero_Open_Skill, OnOpenSkill);
            EventDispatcher.Instance.AddListener(Enum.EventType.Hero_Wear_Equip, OnWearEquip);
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

            var roleData = DatasMgr.Instance.GetRoleData(_roles[_roleIndex]);
            var roleConfig = ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", roleData.configId);
            _proComp.UpdateComp(roleConfig.Name, "");
            _equipComp.UpdateComp(roleConfig.Job);
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
            if (2 == _stateC.selectedIndex || 1 == _stateC.selectedIndex)
                _stateC.SetSelectedIndex(0);
            else
                SceneMgr.Instance.CloseHeroScene();
        }

        private void LoadHero(int uid)
        {
            var roleData = DatasMgr.Instance.GetRoleData(uid);
            var heroConfing = ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", roleData.configId);

            GameObject hero = null;
            if (!_rolesGO.ContainsKey(uid))
            {
                var prefab = AssetMgr.Instance.LoadAsset<GameObject>(heroConfing.Prefab);
                hero = GameObject.Instantiate(prefab);
                _heroRoot = SceneMgr.Instance.GetHeroRoot();
                hero.transform.SetParent(_heroRoot);
                hero.transform.localPosition = Vector3.zero;
                //hero.GetComponent<Animator>().runtimeAnimatorController = AssetMgr.Instance.LoadAsset<RuntimeAnimatorController>("Assets/Animators/NoWeaponController.controller");
                _rolesGO.Add(uid, hero);
            }
            _rolesGO[uid].SetActive(true);

            int animatorID = 1;
            foreach (var v in roleData.equipmentDic)
            {
                var equipPlaceConfig = ConfigMgr.Instance.GetConfig<EquipPlaceConfig>("EquipPlaceConfig", (int)v.Key);
                var spinePoint = _rolesGO[uid].transform.Find(equipPlaceConfig.SpinePoint);

                var equipData = DatasMgr.Instance.GetEquipmentData(v.Value);
                var equipConfig = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", equipData.configId);

                if (1 == animatorID)
                {
                    var equipTypeConfig = ConfigMgr.Instance.GetConfig<EquipmentTypeConfig>("EquipmentTypeConfig", equipConfig.Type);
                    animatorID = equipTypeConfig.Animator;
                }
                var weapon = GameObject.Instantiate<GameObject>(AssetMgr.Instance.LoadAsset<GameObject>(equipConfig.Prefab));
                weapon.transform.SetParent(spinePoint, false);
                weapon.transform.localEulerAngles = equipConfig.Rotation;
            }
            
            var animatorConfig = ConfigMgr.Instance.GetConfig<AnimatorConfig>("AnimatorConfig", animatorID);
            hero.GetComponent<Animator>().runtimeAnimatorController = AssetMgr.Instance.LoadAsset<RuntimeAnimatorController>(animatorConfig.Controller);
        }

        private void OnOpenEquip(params object[] args)
        {
            _stateC.SetSelectedIndex(1);
        }

        private void OnOpenSkill(params object[] args)
        {
            _stateC.SetSelectedIndex(2);
        }

        private void OnWearEquip(params object[] args)
        {
            var roleUID = _roles[_roleIndex];
            var equipUID = (int)args[0];
            var equipData = DatasMgr.Instance.GetEquipmentData(equipUID);
            var equipConfig = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", equipData.configId);
            var equipTypeConfig = ConfigMgr.Instance.GetConfig<EquipmentTypeConfig>("EquipmentTypeConfig", equipConfig.Type);
            var roleData = DatasMgr.Instance.GetRoleData(roleUID);
            EquipPlaceConfig equipPlaceConfig;
            Transform spinePoint;

            //检查同部位有没有佩戴武器
            if (roleData.equipmentDic.ContainsKey(equipTypeConfig.Place))
            {
                roleData.equipmentDic.Remove(equipTypeConfig.Place);
                equipPlaceConfig = ConfigMgr.Instance.GetConfig<EquipPlaceConfig>("EquipPlaceConfig", (int)equipTypeConfig.Place);
                spinePoint = _rolesGO[roleUID].transform.Find(equipPlaceConfig.SpinePoint);
                GameObject.Destroy(spinePoint.GetChild(0).gameObject);
            }

            var removePlace = new List<Enum.EquipPlace>();
            foreach (var v in roleData.equipmentDic)
            {
                var tempEquipData = DatasMgr.Instance.GetEquipmentData(v.Value);
                var equipType = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", tempEquipData.configId).Type;
                var combination = false;
                if (null != equipTypeConfig.Combination)
                {
                    for (int i = 0; i < equipTypeConfig.Combination.Length; i++)
                    {
                        if (equipType == equipTypeConfig.Combination[i])
                        {
                            combination = true;
                            break;
                        }
                    }
                }
                if (!combination)
                {
                    removePlace.Add(v.Key);
                }
            }

            for (int i = removePlace.Count - 1; i >= 0; i--)
            {
                roleData.equipmentDic.Remove(removePlace[i]);
                equipPlaceConfig = ConfigMgr.Instance.GetConfig<EquipPlaceConfig>("EquipPlaceConfig", (int)removePlace[i]);
                spinePoint = _rolesGO[roleUID].transform.Find(equipPlaceConfig.SpinePoint);
                GameObject.Destroy(spinePoint.GetChild(0).gameObject);
            }

            roleData.equipmentDic.Add(equipTypeConfig.Place, equipUID);
            equipPlaceConfig = ConfigMgr.Instance.GetConfig<EquipPlaceConfig>("EquipPlaceConfig", (int)equipTypeConfig.Place);
            spinePoint = _rolesGO[roleUID].transform.Find(equipPlaceConfig.SpinePoint);
            var weapon = GameObject.Instantiate<GameObject>(AssetMgr.Instance.LoadAsset<GameObject>(equipConfig.Prefab));
            weapon.transform.SetParent(spinePoint, false);
            weapon.transform.localEulerAngles = equipConfig.Rotation;

            var animatorConfig = ConfigMgr.Instance.GetConfig<AnimatorConfig>("AnimatorConfig", equipTypeConfig.Animator);
            var animator = _rolesGO[roleUID].GetComponent<Animator>();
            animator.runtimeAnimatorController = AssetMgr.Instance.LoadAsset<RuntimeAnimatorController>(animatorConfig.Controller);
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

            EventDispatcher.Instance.RemoveListener(Enum.EventType.Hero_Open_Equip, OnOpenEquip);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Hero_Open_Skill, OnOpenSkill);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Hero_Wear_Equip, OnWearEquip);
        }
    }
}
