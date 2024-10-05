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
        private List<int> _roles;
        private int _roleIndex = -1;
        private Dictionary<int, GameObject> _rolesGO = new Dictionary<int, GameObject>();
        private float _dragingPower = 0.0f;
        private float _rotateSpeed = 50.0f;
        private float _lastDragTime = 0.0f;
        private bool _draging = false;
        private HeroProComp _proComp;
        private GList _heroList;
        private Dictionary<string, HeroItem> _herosDic = new Dictionary<string, HeroItem>();
        private GTextField _name;
        private List<Sequence> _seqList = new List<Sequence>();
        private CommonResComp _resComp;
        private Dictionary<int, float> _attrsDicCache = new Dictionary<int, float>();
        private GLoader _element;
        private GLoader _heroLoader;
        private NTexture _heroTexture;

        public HeroPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            GetGObjectChild<GLoader>("BG").url = "UI/Background/CommonBG";
            _touchArena = (GGraph)_gCom.GetChild("touchArena");
            _gCom.GetChild("closeBtn").onClick.Add(OnClickClose);
            _gCom.onTouchBegin.Add(OnTouchBegin);
            _gCom.onTouchMove.Add(OnTouchMove);
            _gCom.onTouchEnd.Add(OnTouchEnd);
            _proComp = GetChild<HeroProComp>("proComp");
            _name = GetGObjectChild<GTextField>("name");
            _resComp = GetChild<CommonResComp>("resComp");
            _resComp.InitComp(new List<TwoIntPair> {
                new TwoIntPair((int)Enum.ItemType.TalentRes, DatasMgr.Instance.GetItem((int)Enum.ItemType.TalentRes)),
                new TwoIntPair((int)Enum.ItemType.LevelRes, DatasMgr.Instance.GetItem((int)Enum.ItemType.LevelRes))
            });

            _heroLoader = GetGObjectChild<GLoader>("heroLoader");
            _heroTexture = new NTexture((RenderTexture)args[0]);
            _heroTexture.destroyMethod = DestroyMethod.None;
            _heroLoader.texture = _heroTexture;

            EventDispatcher.Instance.AddListener(Enum.Event.WearEquipS2C, OnWearEquip);
            EventDispatcher.Instance.AddListener(Enum.Event.UnwearEquipS2C, OnUnwearEquip);
            EventDispatcher.Instance.AddListener(Enum.Event.HeroLevelUpS2C, OnHeroLevelUpS2C);
            EventDispatcher.Instance.AddListener(Enum.Event.HeroTalentActiveS2C, OnHeroTalentActiveS2C);
            EventDispatcher.Instance.AddListener(Enum.Event.ResetHeroS2C, OnResetHeroS2C);
            EventDispatcher.Instance.AddListener(Enum.Event.HeroChange_Before, OnHeroChangeBefore);
            EventDispatcher.Instance.AddListener(Enum.Event.HeroChange_After, OnHeroChangeAfter);

            _heroList = GetGObjectChild<GList>("heroList");
            _heroList.itemRenderer = HeroItemRenderer;
            _heroList.onClickItem.Add(ClickHeroItem);

            _roles = DatasMgr.Instance.GetAllRoles();
            _heroList.numItems = _roles.Count;
            _heroList.selectedIndex = 0;

            _element = GetGObjectChild<GLoader>("element");

            GetGObjectChild<GButton>("resetBtn").onClick.Add(OnClickReset);
            SelectHero(0);
        }

        public override void Update(float deltaTime)
        {
            _resComp.Update(deltaTime);

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
            _lastDragTime = TimeMgr.Instance.GetTime();
            _draging = true;
        }

        private void OnTouchMove(EventContext context)
        {
            _dragingPower = _touchPos.x - context.inputEvent.position.x;
            _rolesGO[_roles[_roleIndex]].transform.Rotate(Vector3.up, _rotateSpeed * _dragingPower * (Time.time - _lastDragTime));
            _touchPos = context.inputEvent.position;
            _lastDragTime = TimeMgr.Instance.GetTime();
        }

        public void OnTouchEnd(EventContext context)
        {
            _lastDragTime = TimeMgr.Instance.GetTime();
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
                AssetsMgr.Instance.LoadAssetAsync<GameObject>(heroConfing.Prefab, (GameObject prefab) =>
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

                    UpdateEquips(uid);
                    UpdateAnimator(uid, () =>
                    {
                        Jump(uid, hero, 0);
                    });
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

                //UpdateEquips(uid);
                //UpdateAnimator(uid);
            }
        }

        public void UpdateEquips(int uid)
        {
            var roleData = DatasMgr.Instance.GetRoleData(uid);
            //ConfigMgr.Instance.ForeachConfig<EquipPlaceConfig>("EquipPlaceConfig", (config) =>
            // {
            //     var placeConfig = (EquipPlaceConfig)config;
            //     var spinePoint = _rolesGO[uid].transform.Find(placeConfig.SpinePoint);
            //     if (spinePoint.childCount > 0)
            //     {
            //         GameObject.Destroy(spinePoint.GetChild(0).gameObject);
            //     }
            // });

            foreach (var v in roleData.equipmentDic)
            {
                var equipData = DatasMgr.Instance.GetEquipmentData(v.Value);
                var equipConfig = equipData.GetConfig();
                var placeConfig = equipData.GetPlaceConfig();
                AssetsMgr.Instance.LoadAssetAsync<GameObject>(equipConfig.Prefab, (GameObject prefab) =>
                {
                    var weapon = GameObject.Instantiate<GameObject>(prefab);
                    Tool.Instance.ApplyProcessingFotOutLine(weapon);
                    weapon.transform.SetParent(_rolesGO[uid].transform.Find(placeConfig.SpinePoint), false);
                    weapon.transform.localPosition = equipData.GetTypeConfig().Pos;
                    weapon.transform.localEulerAngles = equipData.GetTypeConfig().Rotation;
                    Tool.SetLayer(weapon.transform, Enum.Layer.Display);
                });

                if (null != equipConfig.VicePrefab)
                {
                    AssetsMgr.Instance.LoadAssetAsync<GameObject>(equipConfig.VicePrefab, (GameObject prefab) =>
                    {
                        var weapon = GameObject.Instantiate<GameObject>(prefab);
                        Tool.Instance.ApplyProcessingFotOutLine(weapon);
                        weapon.transform.SetParent(_rolesGO[uid].transform.Find(placeConfig.ViceSpinePoint), false);
                        weapon.transform.localPosition = Vector3.zero;
                        weapon.transform.localEulerAngles = equipData.GetTypeConfig().ViceRotation;
                        Tool.SetLayer(weapon.transform, Enum.Layer.Display);
                    });
                }
            }
        }

        private void UpdateAnimator(int uid, WGCallback callback = null)
        {
            if (!_rolesGO.ContainsKey(uid))
                return;

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
            //DebugManager.Instance.Log(animatorConfig.Controller);
            AssetsMgr.Instance.LoadAssetAsync<RuntimeAnimatorController>(animatorConfig.Controller, (RuntimeAnimatorController controller) =>
            {
                _rolesGO[uid].GetComponent<Animator>().runtimeAnimatorController = controller;
                if (null != callback)
                    callback();
            });
        }

        private void OnWearEquip(params object[] args)
        {
            var roleUID = _roles[_roleIndex];
            var ndpu = (WearEquipNDPU)args[0];
            if (0 != ndpu.ret)
            {
                TipsMgr.Instance.Add("装备穿戴失败！");
                return;
            }

            ////销毁英雄身上已经穿戴的装备GO
            //foreach (var v in ndpu.unwearEquips)
            //{
            //    var unwearEquipData = DatasMgr.Instance.GetEquipmentData(v);
            //    var placeConfig = unwearEquipData.GetPlaceConfig();
            //    var unwearSpinePoint = _rolesGO[roleUID].transform.Find(placeConfig.SpinePoint);
            //    GameObject.Destroy(unwearSpinePoint.GetChild(0).gameObject);

            //    if (null != placeConfig.ViceSpinePoint)
            //    {
            //        unwearSpinePoint = _rolesGO[roleUID].transform.Find(placeConfig.ViceSpinePoint);
            //        GameObject.Destroy(unwearSpinePoint.GetChild(0).gameObject);
            //    }
            //}

            //创建装备GO
            foreach (var v in ndpu.wearEquips)
            {
                var equipData = DatasMgr.Instance.GetEquipmentData(v);
                var config = equipData.GetConfig();
                var placeConfig = equipData.GetPlaceConfig();
                var spinePoint = _rolesGO[roleUID].transform.Find(placeConfig.SpinePoint);
                AssetsMgr.Instance.LoadAssetAsync<GameObject>(config.Prefab, (GameObject prefab) =>
                {
                    var equip = GameObject.Instantiate<GameObject>(prefab);
                    equip.transform.SetParent(spinePoint, false);
                    equip.transform.localEulerAngles = equipData.GetTypeConfig().Rotation;
                    equip.transform.localPosition = equipData.GetTypeConfig().Pos;
                    Tool.SetLayer(equip.transform, Enum.Layer.Display);
                });

                if (null != placeConfig.ViceSpinePoint)
                {
                    var viceSpinePoint = _rolesGO[roleUID].transform.Find(placeConfig.ViceSpinePoint);
                    AssetsMgr.Instance.LoadAssetAsync<GameObject>(config.VicePrefab, (GameObject prefab) =>
                    {
                        var equip = GameObject.Instantiate<GameObject>(prefab);
                        equip.transform.SetParent(viceSpinePoint, false);
                        equip.transform.localEulerAngles = equipData.GetTypeConfig().ViceRotation;
                        equip.transform.localPosition = Vector3.zero;
                        Tool.SetLayer(equip.transform, Enum.Layer.Display);
                    });
                }
            }

            UpdateAnimator(roleUID);

            if (_roles[_roleIndex] == ndpu.roleUID)
                _proComp.UpdateComp(ndpu.roleUID);
        }

        private void OnUnwearEquip(params object[] args)
        {
            var ndpu = (UnwearEquipNDPU)args[0];
            if (0 != ndpu.ret)
            {
                TipsMgr.Instance.Add("装备卸下失败！");
                return;
            }

            var roleUID = ndpu.roleUID;
            if (!_rolesGO.ContainsKey(roleUID))
                return;

            foreach (var v in ndpu.unwearEquips)
            {
                var equipData = DatasMgr.Instance.GetEquipmentData(v);
                //DebugManager.Instance.Log(equipData.id);
                var placeConfig = equipData.GetPlaceConfig();
                var spinePoint = _rolesGO[roleUID].transform.Find(placeConfig.SpinePoint);
                GameObject.Destroy(spinePoint.GetChild(0).gameObject);

                if (null != placeConfig.ViceSpinePoint)
                {
                    var viceSpinePoint = _rolesGO[roleUID].transform.Find(placeConfig.ViceSpinePoint);
                    GameObject.Destroy(viceSpinePoint.GetChild(0).gameObject);
                }
            }

            UpdateAnimator(roleUID);

            if (!ndpu.fromWear)
            {
                if (_roles[_roleIndex] == ndpu.roleUID)
                    _proComp.UpdateComp(ndpu.roleUID);
            }
        }

        private void SelectHero(int index)
        {
            if (index == _roleIndex)
                return;

            if (_roleIndex >= 0 && _rolesGO.ContainsKey(_roles[_roleIndex]))
            {
                Jump(_roles[_roleIndex], _rolesGO[_roles[_roleIndex]], -2);
            }

            _roleIndex = index;
            LoadHero(_roles[_roleIndex]);

            var role = DatasMgr.Instance.GetRoleData(_roles[_roleIndex]);
            var roleConfig = role.GetConfig();
            _name.text = roleConfig.GetTranslation("Name");
            _element.url = ConfigMgr.Instance.GetConfig<ElementConfig>("ElementConfig", (int)roleConfig.Element).Icon;
            _proComp.UpdateComp(_roles[_roleIndex]);
        }

        private void HeroItemRenderer(int index, GObject item)
        {
            if (!_herosDic.ContainsKey(item.id))
            {
                _herosDic.Add(item.id, new HeroItem((GComponent)item));
            }

            _herosDic[item.id].UpdateHero(_roles[index]);
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
                //DebugManager.Instance.Log("Remove Seq");
                //DebugManager.Instance.Log("Seq Count:" + _seqList.Count);
            });
            seq.onComplete = () =>
            {
                _seqList.Remove(seq);
            };

            _seqList.Add(seq);
        }

        private void OnHeroLevelUpS2C(params object[] args)
        {
            _resComp.UpdateComp(new List<TwoIntPair> {
                new TwoIntPair((int)Enum.ItemType.TalentRes, DatasMgr.Instance.GetItem((int)Enum.ItemType.TalentRes)),
                new TwoIntPair((int)Enum.ItemType.LevelRes, DatasMgr.Instance.GetItem((int)Enum.ItemType.LevelRes))
            });
        }

        private void OnHeroTalentActiveS2C(params object[] args)
        {
            _resComp.UpdateComp(new List<TwoIntPair> {
                new TwoIntPair((int)Enum.ItemType.TalentRes, DatasMgr.Instance.GetItem((int)Enum.ItemType.TalentRes)),
                new TwoIntPair((int)Enum.ItemType.LevelRes, DatasMgr.Instance.GetItem((int)Enum.ItemType.LevelRes))
            });
        }

        private void OnResetHeroS2C(params object[] args)
        {
            if ((Enum.ErrorCode)args[0] != Enum.ErrorCode.Success)
                return;

            _resComp.UpdateComp(new List<TwoIntPair> {
                new TwoIntPair((int)Enum.ItemType.TalentRes, DatasMgr.Instance.GetItem((int)Enum.ItemType.TalentRes)),
                new TwoIntPair((int)Enum.ItemType.LevelRes, DatasMgr.Instance.GetItem((int)Enum.ItemType.LevelRes))
            });
            if (_roles[_roleIndex] == (int)args[1])
                _proComp.UpdateComp((int)args[1]);
        }

        private void OnHeroChangeBefore(params object[] args)
        {
            _attrsDicCache.Clear();

            var roleUID = _roles[_roleIndex];
            var role = DatasMgr.Instance.GetRoleData(roleUID);
            ConfigMgr.Instance.ForeachConfig<AttrConfig>("AttrConfig", (config) =>
            {
                var value = role.GetAttribute((Enum.AttrType)config.ID);
                if (value > 0)
                    _attrsDicCache.Add(config.ID, value);
            });
        }

        private void OnHeroChangeAfter(params object[] args)
        {
            var roleUID = _roles[_roleIndex];
            var role = DatasMgr.Instance.GetRoleData(roleUID);
            ConfigMgr.Instance.ForeachConfig<AttrConfig>("AttrConfig", (config) =>
            {
                var delta = 0.0f;
                var value = role.GetAttribute((Enum.AttrType)config.ID);
                if (!_attrsDicCache.ContainsKey(config.ID))
                    delta = value;
                else if (value != _attrsDicCache[config.ID])
                    delta = value - _attrsDicCache[config.ID];

                if (0 != delta)
                    TipsMgr.Instance.Add(config.GetTranslation("Name") + ": " + BattleMgr.Instance.GetAttributeColorStr(config.ID, delta));
            });

            _attrsDicCache.Clear();
        }

        private void OnClickReset(EventContext context)
        {
            var roleUID = _roles[_roleIndex];
            var role = DatasMgr.Instance.GetRoleData(roleUID);

            WGCallback cb = () =>
            {
                DatasMgr.Instance.ResetHeroC2S(roleUID);
            };

            var desc = string.Format(ConfigMgr.Instance.GetTranslation("HeroPanel_Reset"), role.GetConfig().GetTranslation("Name"));
            UIManager.Instance.OpenPanel("Common", "CommonTipsPanel", new object[] { desc, cb });
        }

        public override void Dispose(bool disposeGCom = false)
        {
            foreach (var v in _seqList)
            {
                v.Kill();
            }
            _seqList.Clear();


            foreach (var v in _rolesGO)
            {
                AssetsMgr.Instance.Destroy(v.Value);
            }
            _rolesGO.Clear();
            _roles = null;

            foreach (var v in _herosDic)
                v.Value.Dispose();
            _herosDic.Clear();

            _heroLoader.texture = null;
            _heroTexture.Unload(true);
            _heroTexture.Dispose();

            //EventDispatcher.Instance.RemoveListener(Enum.EventType.Hero_Open_Equip, OnOpenEquip);
            //EventDispatcher.Instance.RemoveListener(Enum.EventType.Hero_Open_Skill, OnOpenSkill);
            EventDispatcher.Instance.RemoveListener(Enum.Event.WearEquipS2C, OnWearEquip);
            EventDispatcher.Instance.RemoveListener(Enum.Event.UnwearEquipS2C, OnUnwearEquip);
            EventDispatcher.Instance.RemoveListener(Enum.Event.HeroLevelUpS2C, OnHeroLevelUpS2C);
            EventDispatcher.Instance.RemoveListener(Enum.Event.HeroTalentActiveS2C, OnHeroTalentActiveS2C);
            EventDispatcher.Instance.RemoveListener(Enum.Event.ResetHeroS2C, OnResetHeroS2C);
            EventDispatcher.Instance.RemoveListener(Enum.Event.HeroChange_Before, OnHeroChangeBefore);
            EventDispatcher.Instance.RemoveListener(Enum.Event.HeroChange_After, OnHeroChangeAfter);

            base.Dispose(disposeGCom);
        }
    }
}
