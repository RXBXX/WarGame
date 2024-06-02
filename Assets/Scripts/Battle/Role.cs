using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;
using DG.Tweening;

namespace WarGame
{
    public class Role : MapObject
    {
        protected int _id;

        protected LevelRoleData _data;

        protected int _starConfigId = 1;

        private List<string> _path;

        private int _pathIndex;

        private float _lerpStep = 0;

        protected GameObject _hudPoint;

        protected Animator _animator;

        protected string _hpHUDKey;

        protected float _moveSpeed = 0.50f, _jumpSpeed = 0.0f;

        protected List<string> _numberHUDList = new List<string>();

        private Vector3 _offset = new Vector3(0.0f, 0.224f, 0.0f);

        private Quaternion _rotation;

        protected Enum.RoleType _type = Enum.RoleType.None;

        private Dictionary<string, State> _stateDic = new Dictionary<string, State>();

        private string _curAnimState = null;

        protected Dictionary<Enum.EquipPlace, Equip> _equipDic = new Dictionary<Enum.EquipPlace, Equip>();

        private Vector3 _position;

        private bool _isFollowing = false;

        public int ID
        {
            set { }
            get { return _id; }
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

        public List<string> Path
        {
            get { return _path; }
        }

        public int PathIndex
        {
            get { return _pathIndex; }
            set { _pathIndex = value; }
        }

        public float LerpStep
        {
            get { return _lerpStep; }
            set { _lerpStep = value; }
        }

        public Vector3 Offset
        {
            get { return _offset; }
        }

        public Quaternion Rotation
        {
            get { return _rotation; }
        }

        public LevelRoleData Data
        {
            get { return _data; }
        }

        public string Hexagon
        {
            get { return _data.hexagonID; }
            set { _data.hexagonID = value; }
        }

        public Role(LevelRoleData data)
        {
            this._id = data.UID;
            this._data = data;
            _position = MapTool.Instance.GetPosFromCoor(MapManager.Instance.GetHexagon(Hexagon).coor) + _offset;

            CreateGO();
        }

        protected override void CreateGO()
        {
            base.CreateGO();
            _assetID = AssetMgr.Instance.LoadAssetAsync<GameObject>(GetConfig().Prefab, OnCreate);
        }

        protected override void OnCreate(GameObject go)
        {
            base.OnCreate(go);
            _gameObject.transform.position = _position;
            _gameObject.transform.localScale = Vector3.one * 0.6F;
            _animator = _gameObject.GetComponent<Animator>();

            _gameObject.GetComponent<RoleBehaviour>().ID = _id; ;

            _rotation = _gameObject.transform.rotation;
            _hudPoint = _gameObject.transform.Find("hudPoint").gameObject;

            InitEquips();
            InitAnimator();

            if (!Application.isPlaying)
                return;

            CreateHUD();

            OnStateChanged();
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
                var equip = EquipFactory.Instance.GetEquip(equipData);
                equip.SetSpinePoint(spinePoint);
                _equipDic[equip.GetPlace()] = equip;
            }
        }

        protected virtual void InitAnimator()
        {
            var animatorConfig = GetAnimatorConfig();
            AssetMgr.Instance.LoadAssetAsync<RuntimeAnimatorController>(animatorConfig.Controller, (RuntimeAnimatorController controller) =>
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
            _curAnimState = "Idle";
            _stateDic[_curAnimState].Start();
        }

        protected virtual void CreateHUD()
        {
            _hpHUDKey = _id + "_HP";
            var hud = (HUDRole)HUDManager.Instance.AddHUD("HUD", "HUDRole", _hpHUDKey, _hudPoint, new object[] { _id });
            hud.Init(GetHP(), GetAttribute(Enum.AttrType.HP), GetRage(), GetAttribute(Enum.AttrType.Rage));
            hud.SetFollowing(_isFollowing);
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

        public void UpdateHexagonID(string id)
        {
            Hexagon = id;
            _position = MapTool.Instance.GetPosFromCoor(MapManager.Instance.GetHexagon(Hexagon).coor) + _offset;
            _gameObject.transform.position = _position;
        }


        public virtual void Update()
        {
            if (null == _gameObject)
                return;

            if (null == _path || _path.Count <= 0)
                return;

            var startHexagon = MapManager.Instance.GetHexagon(_path[_pathIndex]);
            var endHexagon = MapManager.Instance.GetHexagon(_path[_pathIndex + 1]);

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
            _gameObject.transform.rotation = rotation;
        }

        public void SetAnimState(string stateName)
        {
            _curAnimState = stateName;
        }

        private void EnterState(string stateName)
        {
            if (stateName == _curAnimState)
                return;
            var curState = _stateDic[_curAnimState];
            //_curState = stateName;
            _stateDic[stateName].Start(curState);
        }

        public virtual void MoveEnd()
        {
            EnterState("Idle");
            EventDispatcher.Instance.PostEvent(Enum.EventType.Role_MoveEnd_Event);
        }

        public virtual void Move(List<string> hexagons)
        {
            var count = hexagons.Count;
            if (count <= 0 || hexagons[count - 1] == Hexagon)
                return;

            this._path = hexagons;

            EnterState("Move");
        }

        public virtual void Attack(Vector3 targetPos)
        {
            EnterState("Attack");

            foreach (var e in _equipDic)
            {
                e.Value.Attack(targetPos);
            }
        }

        public virtual void Hit(float deltaHP)
        {
            var prefabPath = "Assets/Prefabs/Effects/CFX_Hit_A Red+RandomText.prefab";
            AssetMgr.Instance.LoadAssetAsync<GameObject>(prefabPath, (GameObject prefab) =>
            {
                var hitPrefab = GameObject.Instantiate(prefab);
                hitPrefab.transform.position = _gameObject.transform.position + new Vector3(0, 0.8f, 0);
            });

            EnterState("Attacked");

            UpdateAttr(Enum.AttrType.HP, -deltaHP);
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
            DebugManager.Instance.Log("Dodge");
            EnterState("Dodge");
            AddFloatHUD("Miss");
        }

        public virtual void Dead()
        {
            EnterState("Dead");
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

        public void SetState(Enum.RoleState state)
        {
            if (state == _data.state)
                return;
            _data.state = state;

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
            var numberHUD = (HUDNumber)HUDManager.Instance.AddHUD("HUD", "HUDNumber", numberID, _gameObject.transform.Find("hudPoint").gameObject);
            _numberHUDList.Add(numberID);
            numberHUD.Show(str, () =>
            {
                HUDManager.Instance.RemoveHUD(numberID);
                _numberHUDList.Remove(numberID);
            });
        }

        protected virtual void UpdateAttr(Enum.AttrType type, float delta)
        {
            _data.UpdateAttr(type, delta);

            if (type == Enum.AttrType.HP)
            {
                HUDRole hud = HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
                hud.UpdateHP(GetHP());

                AddFloatHUD(delta.ToString());

                if (IsDead())
                {
                    Dead();
                }
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_HP_Change, new object[] { _id });
            }
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual float GetViewDis()
        {
            return 0;
        }

        public void HandleEvent(string stateName, string secondStateName)
        {
            if (_stateDic.ContainsKey(stateName))
            {
                var state = _stateDic[stateName];
                var method = typeof(State).GetMethod(secondStateName);
                if (null != method)
                {
                    if (secondStateName == "End")
                    {
                        method.Invoke(state, new object[] { true });
                    }
                    else
                    {
                        method.Invoke(state, null);
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
            _pathIndex++;
            _rotation = _gameObject.transform.rotation;
            UpdateHexagonID(_path[_pathIndex]);
            if (_pathIndex >= _path.Count - 1)
            {
                _path = null;
                _pathIndex = 0;
                MoveEnd();
            }
        }

        public Vector3 GetPosition()
        {
            return _position;
            //return _gameObject.transform.position;
        }

        public void SetForward(Vector3 forward)
        {
            _rotation = Quaternion.LookRotation(forward);
            _gameObject.transform.rotation = _rotation;
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


        //Ìí¼Óbuff
        public void AddBuffs(List<int> buffs)
        {
            _data.AddBuffs(buffs);

            var hud = HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
            hud.UpdateBuffs(_data.buffs);
        }

        protected virtual void ExcuteBuffs()
        {
            _data.ExcuteBuffs();

            var hud = HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
            //if (null != hud)
            hud.UpdateBuffs(_data.buffs);
        }

        public List<int> GetAttackBuffs()
        {
            var buffs = new List<int>();

            if (null != _data.equipDataDic)
            {
                foreach (var v in _data.equipDataDic)
                {
                    var equipConfig = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", v.Value.configId);
                    foreach (var v1 in equipConfig.Buffs)
                    {
                        var rd = Random.Range(0, 100);
                        if (rd < v1.value)
                            buffs.Add(v1.id);
                    }
                }
            }
            return buffs;
        }

        public virtual void UpdateRound()
        {
            ExcuteBuffs();
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
            var pos = MapTool.Instance.GetPosFromCoor(hexagon.coor) + _offset;
            UpdatePosition(pos);
            //_gameObject.transform.position = pos;
        }

        public void SetHPVisible(bool visible)
        {
            var hud = HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
            hud.SetVisible(visible);
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
                smr.material.SetFloat("_HighLight", highLigt);
                smr.material.SetColor("_OutlineColor", color);
            }

            MeshRenderer mr;
            if (go.TryGetComponent(out mr))
            {
                mr.material.SetFloat("_HighLight", highLigt);
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
            _isFollowing = following;

            var hud = HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
            if (null != hud)
                hud.SetFollowing(_isFollowing);
        }

        public Vector3 GetEffectPos()
        {
            return GetPosition() + new Vector3(0, 0.6F, 0);
        }

        public GameObject GetEffectPoint()
        {
            return _gameObject.transform.Find("root/pelvis/spine_01/spine_02/spine_03/neck_01/head").gameObject;
        }

        public void AddEffects(List<Pair> effects)
        {
            foreach (var v in effects)
            {
                UpdateAttr((Enum.AttrType)v.id, v.value);
            }
        }

        public override void Dispose()
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Role_Dispose, new object[] { _id });
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

            base.Dispose();
        }
    }
}
