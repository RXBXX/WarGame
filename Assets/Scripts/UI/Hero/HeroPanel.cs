using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using UnityEditor;
using DG.Tweening;

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
        private GList _heroList;
        private Dictionary<string, HeroItem> _herosDic = new Dictionary<string, HeroItem>();

        public HeroPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            GetUIChild<GLoader>("BG").url = "UI/Background/HeroBG";
            _touchArena = (GGraph)_gCom.GetChild("touchArena");
            _gCom.GetChild("closeBtn").onClick.Add(OnClickClose);

            _gCom.onTouchBegin.Add(OnTouchBegin);
            _gCom.onTouchMove.Add(OnTouchMove);
            _gCom.onTouchEnd.Add(OnTouchEnd);

            _roles = DatasMgr.Instance.GetAllRoles();

            _proComp = GetChild<HeroProComp>("proComp");

            var roleData = DatasMgr.Instance.GetRoleData(_roles[_roleIndex]);
            var roleConfig = roleData.GetConfig();
            string attrDesc = "";
            ConfigMgr.Instance.ForeachConfig<AttrConfig>("AttrConfig", (config) =>
            {
                attrDesc += ((AttrConfig)config).Name + ": +" + roleData.GetAttribute((Enum.AttrType)config.ID) + "\n";
            });
            GetUIChild<GLoader>("heroLoader").texture = new NTexture((RenderTexture)args[0]);

            EventDispatcher.Instance.AddListener(Enum.EventType.Hero_Wear_Equip, OnWearEquip);
            EventDispatcher.Instance.AddListener(Enum.EventType.Hero_Unwear_Equip, OnUnwearEquip);

            _heroList = GetUIChild<GList>("heroList");
            _heroList.itemRenderer = HeroItemRenderer;
            _heroList.onClickItem.Add(ClickHeroItem);
            _heroList.numItems = _roles.Length;

            SelectHero(_roles[_roleIndex]);

            //LoadHero(_roles[_roleIndex]);
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
            if (!_rolesGO.ContainsKey(_roles[_roleIndex]))
                return;
            _rolesGO[_roles[_roleIndex]].transform.Rotate(Vector3.up, _rotateSpeed * deltaTime * _dragingPower);
            _dragingPower = Mathf.Lerp(_dragingPower, 0, deltaTime);
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
            _rolesGO[_roles[_roleIndex]].transform.Rotate(Vector3.up, _rotateSpeed * _dragingPower * (Time.time - _lastDragTime));
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
            SceneMgr.Instance.CloseHeroScene();
        }

        private void LoadHero(int uid)
        {
            var roleData = DatasMgr.Instance.GetRoleData(uid);
            var heroConfing = ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", roleData.configId);

            GameObject hero = null;
            if (!_rolesGO.ContainsKey(uid))
            {
                AssetMgr.Instance.LoadAssetAsync<GameObject>(heroConfing.Prefab, (GameObject prefab) =>
                {
                    hero = GameObject.Instantiate(prefab);
                    _heroRoot = SceneMgr.Instance.GetHeroRoot();
                    hero.transform.SetParent(_heroRoot);
                    hero.transform.localPosition = new Vector3(2, 0, 0);
                    hero.transform.localScale = Vector3.zero;
                    hero.transform.forward = new Vector3(0, 0, -1);
                    Tool.SetLayer(hero.transform, Enum.Layer.Display);
                    Tool.Instance.ApplyProcessingFotOutLine(hero, new List<string> { "Body", "Hair", "Head", "Hat", "AC" });
                    _rolesGO.Add(uid, hero);

                    Jump(uid, hero, 0);

                    UpdateEquips(uid);
                    UpdateAnimator(uid);
                });
            }
            else
            {
                hero = _rolesGO[uid];
                hero.transform.localPosition = new Vector3(2, 0, 0);
                hero.transform.localScale = Vector3.zero;
                hero.transform.forward = new Vector3(0, 0, -1);
                //hero.SetActive(true);
                Jump(uid, hero, 0);

                UpdateEquips(uid);
                UpdateAnimator(uid);
            }
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
                AssetMgr.Instance.LoadAssetAsync<GameObject>(equipConfig.Prefab, (GameObject prefab) =>
                {
                    var weapon = GameObject.Instantiate<GameObject>(prefab);
                    Tool.Instance.ApplyProcessingFotOutLine(weapon);
                    weapon.transform.SetParent(spinePoint, false);
                    weapon.transform.localEulerAngles = equipConfig.Rotation;
                });
            }
        }

        private void UpdateAnimator(int uid)
        {
            var roleData = DatasMgr.Instance.GetRoleData(uid);
            int animatorID = 1;
            foreach (var v in roleData.equipmentDic)
            {
                var equipData = DatasMgr.Instance.GetEquipmentData(v.Value);
                var tempAnimatorID = equipData.GetTypeConfig().Animator;
                if (ConfigMgr.Instance.GetConfig<AnimatorConfig>("AnimatorConfig", animatorID).Priority < ConfigMgr.Instance.GetConfig<AnimatorConfig>("AnimatorConfig", tempAnimatorID).Priority)
                {
                    animatorID = tempAnimatorID;
                }
            }

            var animatorConfig = ConfigMgr.Instance.GetConfig<AnimatorConfig>("AnimatorConfig", animatorID);
            AssetMgr.Instance.LoadAssetAsync<RuntimeAnimatorController>(animatorConfig.Controller, (RuntimeAnimatorController controller) =>
            {
                _rolesGO[uid].GetComponent<Animator>().runtimeAnimatorController = controller;
            });
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
                AssetMgr.Instance.LoadAssetAsync<GameObject>(equipData.GetConfig().Prefab, (GameObject prefab) =>
                {
                    var equip = GameObject.Instantiate<GameObject>(prefab);
                    equip.transform.SetParent(spinePoint, false);
                    equip.transform.localEulerAngles = equipData.GetConfig().Rotation;
                });
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

        private void SelectHero(int index)
        {
            if (index == _roleIndex)
                return;

            if (_rolesGO.ContainsKey(_roles[_roleIndex]))
            {
                Jump(_roles[_roleIndex], _rolesGO[_roles[_roleIndex]], -2);
            }

            _roleIndex = index;
            LoadHero(_roles[_roleIndex]);

            _proComp.UpdateComp(_roles[_roleIndex]);
        }

        private void HeroItemRenderer(int index, GObject item)
        {
            if (!_herosDic.ContainsKey(item.id))
            {
                _herosDic.Add(item.id, new HeroItem((GComponent)item));
            }
            var roleData = DatasMgr.Instance.GetRoleData(_roles[index]);
            _herosDic[item.id].Update(roleData.GetConfig().Icon);
        }

        private void ClickHeroItem(EventContext context)
        {
            var index = _heroList.GetChildIndex((GObject)context.data);
            SelectHero(index);
        }

        private void Jump(int uid, GameObject hero, float posX)
        {
            var roleData = DatasMgr.Instance.GetRoleData(uid);
            int animatorID = 1;
            foreach (var v in roleData.equipmentDic)
            {
                var equipData = DatasMgr.Instance.GetEquipmentData(v.Value);
                var tempAnimatorID = equipData.GetTypeConfig().Animator;
                if (ConfigMgr.Instance.GetConfig<AnimatorConfig>("AnimatorConfig", animatorID).Priority < ConfigMgr.Instance.GetConfig<AnimatorConfig>("AnimatorConfig", tempAnimatorID).Priority)
                {
                    animatorID = tempAnimatorID;
                }
            }

            var animatorConfig = ConfigMgr.Instance.GetConfig<AnimatorConfig>("AnimatorConfig", animatorID);
            var animator = hero.GetComponent<Animator>();
            var eventDic = Tool.Instance.GetEventTimeForAnimClip(animator, animatorConfig.Jump);
            animator.SetBool("Jump", true);
            animator.SetBool("Idle", false);
            Sequence seq = DOTween.Sequence();
            var tweener = hero.transform.DOLocalMoveX(posX, eventDic["Jump_Loss"] - eventDic["Jump_Take"]);
            tweener.onUpdate = () =>
            {
                hero.transform.localScale = (1 - Mathf.Abs(hero.transform.localPosition.x / 2)) * Vector3.one;
            };

            seq.AppendInterval(eventDic["Jump_Take"] - eventDic["Jump_Start"]);
            seq.Append(tweener);
            seq.AppendInterval(eventDic["Jump_End"] - eventDic["Jump_Loss"]);
            seq.AppendCallback(() =>
            {
                animator.SetBool("Jump", false);
                animator.SetBool("Idle", true);
            });
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

            foreach (var v in _herosDic)
                v.Value.Dispose();
            _herosDic.Clear();

            //EventDispatcher.Instance.RemoveListener(Enum.EventType.Hero_Open_Equip, OnOpenEquip);
            //EventDispatcher.Instance.RemoveListener(Enum.EventType.Hero_Open_Skill, OnOpenSkill);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Hero_Wear_Equip, OnWearEquip);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Hero_Unwear_Equip, OnUnwearEquip);
        }
    }
}
