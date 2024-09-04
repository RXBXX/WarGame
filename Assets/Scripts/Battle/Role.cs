using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;
using DG.Tweening;
using FairyGUI;

namespace WarGame
{
    public class Role : MapObject
    {
        protected LevelRoleData _data;

        private List<int> _path;

        public int PathIndex;

        protected GameObject _hudPoint;

        protected Animator _animator;

        protected string _hpHUDKey;

        protected float _moveSpeed = 0.20f, _jumpSpeed = 0.0f;

        protected List<string> _numberHUDList = new List<string>();

        private Quaternion _rotation;

        protected Enum.RoleType _type = Enum.RoleType.None;

        protected Dictionary<string, State> _stateDic = new Dictionary<string, State>();

        private string _curAnimState = null;

        protected Dictionary<Enum.EquipPlace, Equip> _equipDic = new Dictionary<Enum.EquipPlace, Equip>();

        private Vector3 _position;

        public bool DeadFlag = false;

        private Dictionary<int, ElementLine> _elementEffectDic = new Dictionary<int, ElementLine>();

        private Dictionary<Enum.Buff, List<AssetPair<GameObject>>> _buffEffectDic = new Dictionary<Enum.Buff, List<AssetPair<GameObject>>>();

        private bool _isAttacking = false;

        public int ID
        {
            get { return _data.UID; }
        }

        public Enum.RoleType Type
        {
            get { return _type; }
        }

        public GameObject GameObject
        {
            get { return _gameObject; }
        }

        public GameObject HUDPoint
        {
            get { return _hudPoint; }
        }

        public Animator Animator
        {
            get { return _animator; }
        }

        public List<int> Path
        {
            get { return _path; }
        }

        public Quaternion Rotation
        {
            get { return _rotation; }
        }

        public LevelRoleData Data
        {
            get { return _data; }
        }

        public int Hexagon
        {
            get { return _data.hexagonID; }
            set { _data.hexagonID = value; }
        }

        public Role(LevelRoleData data)
        {
            this._data = data;
            //DebugManager.Instance.Log(ID  );
            _position = MapTool.Instance.GetPosFromCoor(MapManager.Instance.GetHexagon(Hexagon).coor) + CommonParams.Offset;

            CreateGO();
        }

        protected override void CreateGO()
        {
            base.CreateGO();
            _assetID = AssetsMgr.Instance.LoadAssetAsync<GameObject>(GetConfig().Prefab, OnCreate);
        }

        protected override void OnCreate(GameObject go)
        {
            base.OnCreate(go);
            _gameObject.transform.position = _position;
            _gameObject.transform.localScale = Vector3.one * 0.4F;
            _animator = _gameObject.GetComponent<Animator>();

            _gameObject.GetComponent<RoleBehaviour>().ID = ID; ;

            UpdateRotation(Quaternion.Euler(Vector3.zero));
            _hudPoint = _gameObject.transform.Find("hudPoint").gameObject;

            InitEquips();
            InitAnimator();

            if (!Application.isPlaying)
                return;

            CreateHUD();

            OnStateChanged();

            UpdateElementEffects();

            EventDispatcher.Instance.PostEvent(Enum.Event.Role_Create_Success, new object[] { ID });
        }

        protected override void SmoothNormal()
        {
            Tool.Instance.ApplyProcessingFotOutLine(_gameObject, new List<string> { "Body", "Hair", "Head", "Hat", "AC" });
        }

        protected virtual void InitEquips()
        {
            if (null == _data.equipDataDic)
                return;

            foreach (var v in _data.equipDataDic)
            {
                var equipPlaceConfig = ConfigMgr.Instance.GetConfig<EquipPlaceConfig>("EquipPlaceConfig", (int)v.Key);
                var spinePoint = _gameObject.transform.Find(equipPlaceConfig.SpinePoint);

                var equipData = _data.equipDataDic[v.Key];
                var equip = Factory.Instance.GetEquip(equipData, _gameObject.transform);
                _equipDic[equip.GetPlace()] = equip;
            }
        }

        protected virtual void InitAnimator()
        {
            var animatorConfig = GetAnimatorConfig();
            AssetsMgr.Instance.LoadAssetAsync<RuntimeAnimatorController>(animatorConfig.Controller, (RuntimeAnimatorController controller) =>
            {
                _gameObject.GetComponent<Animator>().runtimeAnimatorController = controller;
                InitStates();
            });
        }

        private void InitStates()
        {
            _stateDic.Add("Jump", new JumpState("Jump", this));
            var clip = GetAnimatorConfig().Jump;
            var timeDic = Tool.Instance.GetEventTimeForAnimClip(_animator, clip);
            ((JumpState)_stateDic["Jump"]).duration = timeDic["Jump_Loss"] - timeDic["Jump_Take"];

            _stateDic.Add("Idle", new State("Idle", this));
            _stateDic.Add("Move", new MoveState("Move", this));
            _stateDic.Add("Attack", new AttackState("Attack", this));
            _stateDic.Add("Attacked", new AttackedState("Attacked", this));
            _stateDic.Add("Dead", new DeadState("Dead", this));
            _stateDic.Add("Cured", new CuredState("Cured", this));
            _stateDic.Add("Cure", new CureState("Cure", this));
            _stateDic.Add("Dodge", new DodgeState("Dodge", this));
            _stateDic.Add("Victory", new VictoryState("Victory", this));

            if (null != _curAnimState && "Idle" != _curAnimState)
            {
                _stateDic[_curAnimState].Start(_stateDic["Idle"]);
            }
            else
            {
                _curAnimState = "Idle";
                _stateDic[_curAnimState].Start();
            }
        }

        protected virtual void CreateHUD()
        {
            UIManager.Instance.AddPackage("HUD");
            var uiPanel = _hudPoint.AddComponent<UIPanel>();
            uiPanel.packagePath = "UI/HUD";
            uiPanel.packageName = "HUD";
            uiPanel.componentName = "HUDRole";
            uiPanel.container.renderMode = RenderMode.WorldSpace;
            uiPanel.ui.scale = new Vector2(0.012F, 0.012F);

            _hpHUDKey = ID + "_HP";
            var args = new object[] { ID, GetHPType(), GetHP(), GetAttribute(Enum.AttrType.HP), GetRage(), GetAttribute(Enum.AttrType.Rage), GetElement(), _data.buffs };
            var hud = HUDManager.Instance.AddHUD<HUDRole>("HUDRole", _hpHUDKey, _hudPoint.GetComponent<UIPanel>().ui, _hudPoint, args);
            hud.SetHPVisible(true);
        }

        protected virtual int GetHPType()
        {
            return 0;
        }

        public AnimatorConfig GetAnimatorConfig()
        {
            int animatorID = 1;
            foreach (var v in _data.equipDataDic)
            {
                var tempAnimatorID = v.Value.GetTypeConfig().Animator;
                if (ConfigMgr.Instance.GetConfig<AnimatorConfig>("AnimatorConfig", animatorID).Priority < ConfigMgr.Instance.GetConfig<AnimatorConfig>("AnimatorConfig", tempAnimatorID).Priority)
                {
                    animatorID = tempAnimatorID;
                }
            }
            return ConfigMgr.Instance.GetConfig<AnimatorConfig>("AnimatorConfig", animatorID);
        }

        public void UpdateHexagonID(int id, bool showElements = false)
        {
            if (showElements)
                ClearElementEffects();

            Hexagon = id;
            _position = MapTool.Instance.GetPosFromCoor(MapManager.Instance.GetHexagon(Hexagon).coor) + CommonParams.RoleOffset;
            _gameObject.transform.position = _position;

            if (showElements)
                UpdateElementEffects();
        }


        public virtual void Update()
        {
            if (null == _gameObject)
                return;

            if (null == _path || _path.Count <= 0)
                return;

            var startHexagon = MapManager.Instance.GetHexagon(_path[PathIndex]);
            var endHexagon = MapManager.Instance.GetHexagon(_path[PathIndex + 1]);

            if (startHexagon.coor.y != endHexagon.coor.y)
            {
                EnterState("Jump");
            }
            else
            {
                EnterState("Move");
            }

            _stateDic[_curAnimState].Update();
        }

        public virtual bool IsDead()
        {
            return GetHP() <= 0;
        }

        public virtual void UpdatePosition(Vector3 pos)
        {
            _position = pos;
            _gameObject.transform.position = _position;
        }

        public virtual void UpdateRotation(Quaternion rotation)
        {
            _rotation = rotation;
            _gameObject.transform.rotation = rotation;
        }

        public void SetAnimState(string stateName)
        {
            _curAnimState = stateName;
        }

        private void EnterState(string stateName)
        {
            if (!IsCreated())
            {
                _curAnimState = stateName;
                return;
            }

            if (stateName == _curAnimState)
                return;

            _stateDic[stateName].Start(_stateDic[_curAnimState]);
        }

        public virtual void MoveEnd()
        {
            EnterState("Idle");

            UpdateElementEffects();

            EventDispatcher.Instance.PostEvent(Enum.Event.Role_MoveEnd_Event);
        }

        public virtual void Move(List<int> hexagons)
        {
            var count = hexagons.Count;
            if (count <= 0 || hexagons[count - 1] == Hexagon)
                return;

            this._path = hexagons;

            ClearElementEffects();

            EnterState("Move");
        }

        public virtual void Attack(List<Vector3> hitPoss)
        {
            _isAttacking = true;

            EnterState("Attack");

            foreach (var e in _equipDic)
            {
                e.Value.Attack(hitPoss);
            }
        }

        public virtual void Hit(float deltaHP, string hitEffect, int attacker)
        {
            _isAttacking = true;

            if (null != hitEffect)
            {
                //var prefabPath = "Assets/Prefabs/Effects/CFX_Hit_A Red+RandomText.prefab";
                AssetsMgr.Instance.LoadAssetAsync<GameObject>(hitEffect, (GameObject prefab) =>
                {
                    var hitPrefab = GameObject.Instantiate(prefab);
                    hitPrefab.transform.position = _gameObject.transform.position + new Vector3(0, 0.8f, 0);
                });
            }

            PlaySound("Assets/Audios/Hit.mp3");
            EnterState("Attacked");

            UpdateAttr(Enum.AttrType.HP, -deltaHP);

            //AudioMgr.Instance.PlaySound("Assets/Audios/Hit.mp3");
        }

        public virtual void Cure()
        {
            EnterState("Cure");
        }

        public virtual void Cured(float deltaHP)
        {
            EnterState("Cured");

            UpdateAttr(Enum.AttrType.HP, deltaHP);
        }

        public virtual float GetCurePower()
        {
            return GetAttribute(Enum.AttrType.Cure);
        }

        public virtual void Dodge()
        {
            PlaySound("Assets/Audios/Roll.mp3");
            EnterState("Dodge");
            AddFloatHUD("Miss");
        }

        public virtual void Inspire()
        {
            EnterState("Cure");
        }

        public virtual void Inspired(float delta)
        {
            EnterState("Cured");

            UpdateAttr(Enum.AttrType.Rage, delta);
        }

        public virtual void Dead()
        {
            DeadFlag = true;
            EnterState("Dead");

            if (0 != _data.cloneRole)
            {
                var cloneRole = RoleManager.Instance.GetRole(_data.cloneRole);
                if (null != cloneRole)
                    cloneRole.Dead();

                _data.cloneRole = 0;
            }
            PlaySound(GetConfig().DeadSound);
            //AudioMgr.Instance.PlaySound(GetConfig().DeadSound);
        }

        public virtual void Idle()
        {
            EnterState("Idle");
        }

        public virtual void Jump()
        {
            EnterState("Jump");
        }

        public Enum.RoleState GetState()
        {
            return _data.state;
        }

        public virtual void ResetState()
        {
            ResetState();
        }

        public void SetState(Enum.RoleState state, bool ingoreStateChange = false)
        {
            if (state == _data.state)
                return;
            _data.state = state;

            if (!ingoreStateChange)
                OnStateChanged();
        }

        protected virtual void OnStateChanged()
        {
            HUDRole hud = HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
            if (_data.state == Enum.RoleState.Locked)
                hud.SetState(0);
            else if (_data.state == Enum.RoleState.Waiting)
                hud.SetState(1);
            else if (_data.state == Enum.RoleState.Over)
                hud.SetState(2);
            else
                hud.SetState(3);

        }

        private void AddFloatHUD(string str)
        {
            var numberID = ID + "_HUDNumber_" + _numberHUDList.Count;
            var numberHUD = HUDManager.Instance.AddHUD<HUDNumber>("HUD", "HUDNumber", numberID, _hudPoint);
            _numberHUDList.Add(numberID);
            numberHUD.Show((_numberHUDList.Count - 1) * 0.1F, str, () => { onNumberHUDRemove(numberID); });
        }

        private void onNumberHUDRemove(string id)
        {
            HUDManager.Instance.RemoveHUD(id);
            _numberHUDList.Remove(id);
        }

        protected virtual void UpdateAttr(Enum.AttrType type, float delta)
        {
            if (0 == delta)
                return;

            _data.UpdateAttr(type, delta);
            OnUpdateAttr(type, delta);
        }

        private void OnUpdateAttr(Enum.AttrType type, float delta)
        {
            if (0 == delta)
                return;

            if (type == Enum.AttrType.HP)
            {
                HUDRole hud = HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
                hud.UpdateHP(GetHP());
                if (IsDead())
                {
                    Dead();
                }
            }
            else if (type == Enum.AttrType.Rage)
            {
                HUDRole hud = HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
                hud.UpdateRage(GetRage());
            }

            var format = "";
            if (delta > 0)
                format = "[color={0}]{1}[/color]";
            else
                format = "[color={0}]{1}[/color]";
            AddFloatHUD(string.Format(format, CommonParams.GetAttrColor(type), delta));
        }

        public float GetHP()
        {
            return _data.HP;
        }

        public float GetRage()
        {
            return _data.Rage;
        }

        public virtual float GetMoveDis()
        {
            return GetAttribute(Enum.AttrType.MoveDis);
        }

        public virtual float GetAttackDis()
        {
            return GetAttribute(Enum.AttrType.AttackDis);
        }

        public float GetAttribute(Enum.AttrType type)
        {
            return _data.GetAttribute(type);
        }

        public void HandleEvent(string stateName, string secondStateName)
        {
            if (_stateDic.ContainsKey(stateName) && _curAnimState == stateName)
            {
                var state = _stateDic[stateName];
                var method = typeof(State).GetMethod(secondStateName);
                if (null != method)
                {
                    if (secondStateName == "End")
                    {
                        method.Invoke(state, new object[] { true });
                    }
                    else if (secondStateName == "Start")
                    {
                        method.Invoke(state, new object[] { null });
                    }
                    else
                    {
                        method.Invoke(state, new object[] { });
                    }
                }
            }

            foreach (var v in _equipDic)
            {
                v.Value.HandleEvent(stateName, secondStateName);
            }
        }

        public void NextPath()
        {
            PathIndex++;
            UpdateHexagonID(_path[PathIndex]);
            if (PathIndex >= _path.Count - 1)
            {
                _path = null;
                PathIndex = 0;
                MoveEnd();
            }
        }

        public override Vector3 GetPosition()
        {
            return _position;
        }

        public Vector3 GetFollowPos()
        {
            return _position + new Vector3(0, 0.8f, 0);
        }

        public void SetForward(Vector3 forward)
        {
            UpdateRotation(Quaternion.LookRotation(forward));
        }

        public void SetColliderEnable(bool enable)
        {
            _gameObject.GetComponent<BoxCollider>().enabled = enable;
        }

        public RoleConfig GetConfig()
        {
            return _data.GetConfig();
        }

        public RoleStarConfig GetStarConfig()
        {
            return _data.GetStarConfig();
        }


        //添加buff
        public void AddBuffs(List<int> buffs, Enum.RoleType type)
        {
            _data.AddBuffs(buffs, type, OnUpdateBuff);

            foreach (var v in buffs)
            {
                AddFloatHUD(ConfigMgr.Instance.GetConfig<BufferConfig>("BufferConfig", v).GetTranslation("Name"));
            }

            //UpdateBuffs(type);
        }

        protected virtual void UpdateBuffs(Enum.RoleType type)
        {
            _data.UpdateBuffs(type, OnUpdateBuff);
        }

        /// <summary>
        /// buff变化
        /// </summary>
        /// <param name="args">
        /// [0] buffer ID
        /// [1] buffer 变化类型
        /// [2] Attribute ID
        /// [3] Attribute ChangeValue
        /// </param>
        protected void OnUpdateBuff(params object[] args)
        {
            var buffUpdate = (Enum.BuffUpdate)args[1];
            var hud = HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
            hud.UpdateBuffs(_data.buffs);
            if (buffUpdate != Enum.BuffUpdate.None)
            {

                var buff = (Enum.Buff)args[0];
                switch (buff)
                {
                    case Enum.Buff.Cloaking:
                        SetVisible(buffUpdate != Enum.BuffUpdate.Add);
                        break;
                }
                OnUpdateBuffEffect(buff, buffUpdate);
            }

            OnUpdateAttr((Enum.AttrType)args[2], (float)args[3]);
        }

        public List<int> GetAttackBuffs()
        {
            var buffs = new List<int>();

            if (null != _data.equipDataDic)
            {
                foreach (var v in _data.equipDataDic)
                {
                    var equipConfig = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", v.Value.id);
                    if (null != equipConfig.Buffs)
                    {
                        foreach (var v1 in equipConfig.Buffs)
                        {
                            var rd = Random.Range(0, 100);
                            if (rd < v1.value)
                                buffs.Add(v1.id);
                        }
                    }
                }
            }
            return buffs;
        }

        public virtual void UpdateRound(Enum.RoleType type)
        {
            UpdateBuffs(type);

            if (type != Type)
                return;

            UpdateAttr(Enum.AttrType.Rage, GetAttribute(Enum.AttrType.RageRecover));

            if (!_isAttacking)
            {
                UpdateAttr(Enum.AttrType.HP, GetAttribute(Enum.AttrType.HPRecover));
            }

            ResetState();

            _isAttacking = false;
        }

        public override Tweener ChangeToArenaSpace(Vector3 pos, float duration)
        {
            var tweener = base.ChangeToArenaSpace(pos, duration);
            tweener.onUpdate = () =>
            {
                _position = GameObject.transform.position;
            };
            return tweener;
        }

        public override void ChangeToMapSpace()
        {
            base.ChangeToMapSpace();

            var hexagon = MapManager.Instance.GetHexagon(Hexagon);
            var pos = MapTool.Instance.GetPosFromCoor(hexagon.coor) + CommonParams.RoleOffset;
            UpdatePosition(pos);
            //_gameObject.transform.position = pos;
        }

        public void SetHUDRoleVisible(bool visible)
        {
            GetHUDRole().SetVisible(visible);
        }

        public HUDRole GetHUDRole()
        {
            return HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
        }

        public override void HighLight()
        {
            if (null == _gameObject)
                return;
            SetMaterial(_gameObject, 0, Color.white);
        }

        public override void ResetHighLight()
        {
            if (null == _gameObject)
                return;
            SetMaterial(_gameObject, 1, Color.black);
        }

        private void SetMaterial(GameObject go, float highLigt, Color color)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                SetMaterial(go.transform.GetChild(i).gameObject, highLigt, color);
            }

            SkinnedMeshRenderer smr;
            if (go.TryGetComponent(out smr))
            {
                smr.material.SetColor("_OutlineColor", color);
            }

            MeshRenderer mr;
            if (go.TryGetComponent(out mr))
            {
                mr.material.SetColor("_OutlineColor", color);
            }
        }

        public override float GetLoadingProgress()
        {
            var percent = base.GetLoadingProgress();
            if (null != _data.equipDataDic)
            {
                foreach (var v in _data.equipDataDic)
                {
                    if (_equipDic.ContainsKey(v.Key))
                    {
                        percent += _equipDic[v.Key].GetLoadingProgress();
                    }
                }
            }
            return percent / (1 + _data.equipDataDic.Count);
        }

        public void SetFollowing(bool following)
        {
            var hud = HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
            if (null != hud)
                hud.SetFollowing(following);
        }

        public Vector3 GetEffectPos()
        {
            return _gameObject.transform.Find("effectPoint").position;
        }

        public Vector3 GetHitPos()
        {
            return _gameObject.transform.Find("hitPoint").position;
        }

        public GameObject GetEffectPoint()
        {
            return _gameObject.transform.Find("hitPoint").gameObject;
        }

        public void AddEffects(List<IntFloatPair> effects)
        {
            foreach (var v in effects)
            {
                UpdateAttr((Enum.AttrType)v.id, v.value);
            }
        }

        public void ClearRage()
        {
            UpdateAttr(Enum.AttrType.Rage, -GetRage());
        }

        public Enum.Element GetElement()
        {
            return GetConfig().Element;
        }

        public ElementConfig GetElementConfig()
        {
            return _data.GetElementConfig();
        }

        protected virtual int GetNextStage()
        {
            return 0;
        }

        public virtual bool HaveNextStage()
        {
            return false;
        }

        public virtual void NextStage()
        {

        }

        public string GetAttackEffect()
        {
            foreach (var v in _equipDic)
            {
                if (null != v.Value.GetAttackEffect())
                    return v.Value.GetAttackEffect();
            }
            return null;
        }

        public string GetName()
        {
            return _data.GetConfig().GetTranslation("Name");
        }

        /// <summary>
        ///检查是否有元素加成
        /// </summary>
        private void UpdateElementEffects()
        {
            var hexagon = MapManager.Instance.GetHexagon(Hexagon);
            foreach (var v in MapManager.Instance.Dicections)
            {
                var roleID = RoleManager.Instance.GetRoleIDByHexagonID(MapTool.Instance.GetHexagonKey(hexagon.coor + v));
                if (0 == roleID || roleID == ID)
                    continue;

                var role = RoleManager.Instance.GetRole(roleID);
                if (Type != role.Type)
                    continue;

                if (role.GetElementConfig().Reinforce == GetElement())
                {
                    AddElementEffect(roleID, role.GetElement());
                }
                if (GetElementConfig().Reinforce == role.GetElement())
                {
                    role.AddElementEffect(ID, GetElement());
                }
            }
        }

        public void AddElementEffect(int roleUID, Enum.Element element)
        {
            if (!IsCreated())
                return;

            if (_elementEffectDic.ContainsKey(roleUID))
                return;

            var linkRole = RoleManager.Instance.GetRole(roleUID);
            if (!linkRole.IsCreated())
                return;

            var el = new ElementLine(GetEffectPos(), linkRole.GetEffectPos(), CommonParams.GetElementColor(element));
            _elementEffectDic.Add(roleUID, el);
        }

        public void RemoveElementEffect(int roleUID)
        {
            if (!_elementEffectDic.ContainsKey(roleUID))
                return;

            _elementEffectDic[roleUID].Dispose();
            _elementEffectDic.Remove(roleUID);
        }

        /// <summary>
        ///清除元素加成特效
        /// </summary>
        private void ClearElementEffects()
        {
            var hexagon = MapManager.Instance.GetHexagon(Hexagon);
            foreach (var v in MapManager.Instance.Dicections)
            {
                var roleID = RoleManager.Instance.GetRoleIDByHexagonID(MapTool.Instance.GetHexagonKey(hexagon.coor + v));
                if (0 == roleID || roleID == ID)
                    continue;

                var role = RoleManager.Instance.GetRole(roleID);
                if (Type != role.Type)
                    continue;

                if (role.GetElementConfig().Reinforce == GetElement())
                {
                    RemoveElementEffect(roleID);
                }

                if (GetElementConfig().Reinforce == role.GetElement())
                {
                    role.RemoveElementEffect(ID);
                }
            }
        }

        //是否可见
        public bool Visible()
        {
            foreach (var v in _data.buffs)
            {
                if ((Enum.Buff)v.id == Enum.Buff.Cloaking)
                    return false;
            }
            return true;
        }

        protected virtual void SetVisible(bool visible)
        {
        }

        /// <summary>
        /// 克隆
        /// </summary>
        /// <returns></returns>
        public virtual LevelRoleData Clone(int hexagon, int cloneUID)
        {
            return null;
        }

        public virtual bool HaveCloneRole()
        {
            return 0 != _data.cloneRole && null != RoleManager.Instance.GetRole(_data.cloneRole);
        }

        public void ExtraTurn()
        {
            EnterState("Cured");

            ResetState();
        }

        public void ReduceRage(float ratio)
        {
            UpdateAttr(Enum.AttrType.Rage, -GetRage() * ratio);
        }

        public void AddShield(List<int> buffs, Enum.RoleType type)
        {
            EnterState("Cured");

            AddBuffs(buffs, type);
        }

        public virtual bool CanAction()
        {
            foreach (var v in _data.buffs)
            {
                if ((Enum.Buff)v.id == Enum.Buff.Dizzy)
                    return false;
            }

            return true;
        }

        private void OnUpdateBuffEffect(Enum.Buff id, Enum.BuffUpdate update)
        {
            if (update == Enum.BuffUpdate.None)
                return;

            var buffConfig = ConfigMgr.Instance.GetConfig<BufferConfig>("BufferConfig", (int)id);
            if (buffConfig.Effect == null)
                return;

            if (update == Enum.BuffUpdate.Add)
            {
                if (!_buffEffectDic.ContainsKey(id))
                {
                    _buffEffectDic[id] = new List<AssetPair<GameObject>>();
                }

                var newAP = new AssetPair<GameObject>();
                newAP.ID = AssetsMgr.Instance.LoadAssetAsync<GameObject>(buffConfig.Effect, (go) =>
                {
                    var GO = GameObject.Instantiate<GameObject>(go);
                    GO.transform.SetParent(_gameObject.transform, false);
                    //GO.transform.localPosition = Vector3.zero;
                    GO.transform.position = GetEffectPos();
                    newAP.Obj = GO;
                });
                _buffEffectDic[id].Add(newAP);
            }
            else
            {
                if (!_buffEffectDic.ContainsKey(id))
                    return;
                if (_buffEffectDic[id].Count <= 0)
                    return;

                var dirtyEffect = _buffEffectDic[id][_buffEffectDic[id].Count - 1];
                dirtyEffect.Dispose();
                _buffEffectDic[id].Remove(dirtyEffect);
            }
        }

        public void AddHP(float deltaHP)
        {
            UpdateAttr(Enum.AttrType.HP, deltaHP);
        }

        /// <summary>
        /// 掉落
        /// </summary>
        public virtual void ShowDrops()
        {
        }

        public void Preview(float hurt)
        {
            GetHUDRole().Preview(hurt);
        }

        public void CancelPreview()
        {
            GetHUDRole().CancelPreview();
        }

        public void GoIntoBattle()
        {
            //AudioMgr.Instance.PlaySound(GetConfig().IntoBattleSound);
            EnterState("Cured");
        }

        public override bool Dispose()
        {
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Role_Dispose, new object[] { ID });

            var allRoles = RoleManager.Instance.GetAllRoles();
            foreach (var v in allRoles)
            {
                if (v.ID == ID)
                    continue;
                v.RemoveElementEffect(ID);
            }
            foreach (var v in _elementEffectDic)
                v.Value.Dispose();
            _elementEffectDic.Clear();

            foreach (var v in _stateDic)
                v.Value.Dispose();
            _stateDic.Clear();

            if (null != _hpHUDKey)
            {
                HUDManager.Instance.RemoveHUD(_hpHUDKey);
                _hpHUDKey = null;
            }
            for (int i = _numberHUDList.Count - 1; i >= 0; i--)
            {
                HUDManager.Instance.RemoveHUD(_numberHUDList[i]);
            }
            _numberHUDList.Clear();

            foreach (var e in _equipDic)
            {
                e.Value.Dispose();
            }
            _equipDic.Clear();

            _data.Dispose();

            foreach (var v in _buffEffectDic)
            {
                foreach (var v1 in v.Value)
                    v1.Dispose();
            }
            _buffEffectDic.Clear();

            return base.Dispose();
        }
    }
}
