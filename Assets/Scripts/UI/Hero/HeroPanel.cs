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
        private HeroTalentComp _talentComp;
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

            _proComp = GetChild<HeroProComp>("proComp");
            _equipComp = GetChild<HeroEquipComp>("equipComp");
            _talentComp = GetChild<HeroTalentComp>("talentComp");

            var roleData = DatasMgr.Instance.GetRoleData(_roles[_roleIndex]);
            var roleConfig = roleData.GetConfig();
            string attrDesc = "";
            ConfigMgr.Instance.ForeachConfig<AttrConfig>("AttrConfig", (config) =>
            {
                attrDesc += ((AttrConfig)config).Name + ": +" + roleData.GetAttribute((Enum.AttrType)config.ID) + "\n";
            });
            _proComp.UpdateComp(_roles[_roleIndex]);
            _equipComp.UpdateComp(_roles[_roleIndex]);
            _talentComp.UpdateComp(roleConfig.TalentGroup, roleData.talentDic);

            EventDispatcher.Instance.AddListener(Enum.EventType.Hero_Open_Equip, OnOpenEquip);
            EventDispatcher.Instance.AddListener(Enum.EventType.Hero_Open_Skill, OnOpenSkill);
            EventDispatcher.Instance.AddListener(Enum.EventType.Hero_Wear_Equip, OnWearEquip);
            EventDispatcher.Instance.AddListener(Enum.EventType.Hero_Talent_Active, OnActiveEquip);
            EventDispatcher.Instance.AddListener(Enum.EventType.Hero_Unwear_Equip, OnUnwearEquip);

            LoadHero(_roles[_roleIndex]);
        }

        public override void Update(float deltaTime)
        {
            if (_draging)
            {
                if (Time.time - _lastDragTime > deltaTime)
                    _dragingPower = 0;
                return;
            }

            if (Mathf.Abs(_dragingPower) < 0.1F)
            {
                _dragingPower = 0;
                return;
            }

            _heroRoot.Rotate(Vector3.up, _rotateSpeed * deltaTime * _dragingPower);
            _dragingPower = Mathf.Lerp(_dragingPower, 0, deltaTime);
        }

        private void OnClickArrow(int dir)
        {
            if (_rolesGO.ContainsKey(_roles[_roleIndex]))
                _rolesGO[_roles[_roleIndex]].SetActive(false);

            _roleIndex = (_roleIndex + 1) % _roles.Length;

            LoadHero(_roles[_roleIndex]);

            var roleData = DatasMgr.Instance.GetRoleData(_roles[_roleIndex]);
            var roleConfig = ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", roleData.configId);
            _proComp.UpdateComp(_roles[_roleIndex]);
            _equipComp.UpdateComp(_roles[_roleIndex]);
            _talentComp.UpdateComp(roleConfig.TalentGroup, roleData.talentDic);
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
            _lastDragTime = Time.time;
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
                hero.transform.localScale = Vector3.one * 1.2F;
                Tool.Instance.ApplyProcessingFotOutLine(hero, new List<string> { "Body", "Hair", "Head", "Hat", "AC" });
                _rolesGO.Add(uid, hero);
            }
            else
            {
                hero = _rolesGO[uid];
                hero.SetActive(true);
            }

            UpdateEquips(uid);
            UpdateAnimator(uid);
        }

        public void UpdateEquips(int uid)
        {
            var roleData = DatasMgr.Instance.GetRoleData(uid);
            ConfigMgr.Instance.ForeachConfig<EquipPlaceConfig>("EquipPlaceConfig", (config) =>
             {
                 var placeConfig = (EquipPlaceConfig)config;
                 var spinePoint = _rolesGO[uid].transform.Find(placeConfig.SpinePoint);
                 if (spinePoint.childCount > 0)
                 {
                     GameObject.Destroy(spinePoint.GetChild(0).gameObject);
                 }
             });

            foreach (var v in roleData.equipmentDic)
            {
                var equipData = DatasMgr.Instance.GetEquipmentData(v.Value);
                var spinePoint = _rolesGO[uid].transform.Find(equipData.GetPlaceConfig().SpinePoint);

                var equipConfig = equipData.GetConfig();
                var weapon = GameObject.Instantiate<GameObject>(AssetMgr.Instance.LoadAsset<GameObject>(equipConfig.Prefab));

                Tool.Instance.ApplyProcessingFotOutLine(weapon);

                weapon.transform.SetParent(spinePoint, false);
                weapon.transform.localEulerAngles = equipConfig.Rotation;
            }
        }

        private void UpdateAnimator(int uid)
        {
            var roleData = DatasMgr.Instance.GetRoleData(uid);
            int animatorID = 1;
            foreach (var v in roleData.equipmentDic)
            {
                var equipData = DatasMgr.Instance.GetEquipmentData(v.Value);
                if (1 == animatorID)
                {
                    animatorID = equipData.GetTypeConfig().Animator;
                }
            }

            var animatorConfig = ConfigMgr.Instance.GetConfig<AnimatorConfig>("AnimatorConfig", animatorID);
            _rolesGO[uid].GetComponent<Animator>().runtimeAnimatorController = AssetMgr.Instance.LoadAsset<RuntimeAnimatorController>(animatorConfig.Controller);
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
            var ndpu = DatasMgr.Instance.WearEquip(roleUID, (int)args[0], args.Length <= 1 ? 0 : (int)args[1]);
            if (0 != ndpu.ret)
            {
                TipsMgr.Instance.Add("装备穿戴失败！");
                return;
            }

            foreach (var v in ndpu.unwearEquips)
            {
                var unwearEquipData = DatasMgr.Instance.GetEquipmentData(v);
                var unwearSpinePoint = _rolesGO[roleUID].transform.Find(unwearEquipData.GetPlaceConfig().SpinePoint);
                GameObject.Destroy(unwearSpinePoint.GetChild(0).gameObject);
            }

            foreach (var v in ndpu.wearEquips)
            {
                var equipData = DatasMgr.Instance.GetEquipmentData(v);
                var spinePoint = _rolesGO[roleUID].transform.Find(equipData.GetPlaceConfig().SpinePoint);
                var equip = GameObject.Instantiate<GameObject>(AssetMgr.Instance.LoadAsset<GameObject>(equipData.GetConfig().Prefab));
                equip.transform.SetParent(spinePoint, false);
                equip.transform.localEulerAngles = equipData.GetConfig().Rotation;
            }

            UpdateAnimator(roleUID);
        }

        private void OnUnwearEquip(params object[] args)
        {
            var roleUID = _roles[_roleIndex];
            var ndpu = DatasMgr.Instance.UnwearEquip(roleUID, (int)args[0]);
            if (0 != ndpu.ret)
            {
                TipsMgr.Instance.Add("装备卸下失败！");
                return;
            }

            foreach (var v in ndpu.unwearEquips)
            {
                var equipData = DatasMgr.Instance.GetEquipmentData(v);
                var spinePoint = _rolesGO[roleUID].transform.Find(equipData.GetPlaceConfig().SpinePoint);
                GameObject.Destroy(spinePoint.GetChild(0).gameObject);
            }

            UpdateAnimator(roleUID);
        }

        private void OnActiveEquip(params object[] args)
        {
            var talentId = (int)args[0];
            DatasMgr.Instance.ActiveTalent(_roles[_roleIndex], talentId);
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
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Hero_Unwear_Equip, OnUnwearEquip);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Hero_Talent_Active, OnActiveEquip);
        }
    }
}
