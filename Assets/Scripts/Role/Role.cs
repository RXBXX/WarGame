using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;

namespace WarGame
{
    public class Role:MapObject
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

        public string hexagonID;

        protected List<string> _numberHUDList = new List<string>();

        private Vector3 _offset = new Vector3(0.0f, 0.2f, 0.0f);

        protected Enum.RoleState _state;

        private Quaternion _rotation;

        protected Enum.RoleType _type = Enum.RoleType.None;

        private Dictionary<string, State> _stateDic = new Dictionary<string, State>();

        private string _curAnimState = null;

        public float hp; //血量

        public List<Pair> _buffs = new List<Pair>();

        protected Dictionary<Enum.EquipPlace, Equip> _equipDic = new Dictionary<Enum.EquipPlace, Equip>();

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

        public Role(LevelRoleData data)
        {
            this._layer = 7;
            this._id = data.UID;
            this._data = data;
            this.hexagonID = data.hexagonID;
            this.hp = GetStarConfig().HP;

            var bornPoint = MapTool.Instance.GetPosFromCoor(MapManager.Instance.GetHexagon(hexagonID).coor) + _offset;
            OnCreate(bornPoint);//加载方式，同步方式，后面都要改
        }

        protected virtual void OnCreate(Vector3 bornPoint)
        {
            var assetPath = GetConfig().Prefab;
            GameObject prefab = AssetMgr.Instance.LoadAsset<GameObject>(assetPath);
            _gameObject = GameObject.Instantiate(prefab);
            _gameObject.transform.position = bornPoint;
            _gameObject.transform.localScale = Vector3.one * 0.6F;
            _animator = _gameObject.GetComponent<Animator>();
            _gameObject.GetComponent<RoleBehaviour>().ID = _id;
            _rotation = _gameObject.transform.rotation;
            _hudPoint = _gameObject.transform.Find("hudPoint").gameObject;

            InitEquips();
            InitAnimator();
            InitStates();
            CreateHUD();
        }

        protected virtual void InitEquips()
        {
        }

        protected virtual void InitAnimator()
        {
            int animatorID = 1;
            foreach (var v in _equipDic)
            {
                if (1 == animatorID)
                {
                    var equipTypeConfig = v.Value.GetTypeConfig();
                    animatorID = equipTypeConfig.Animator;
                }
            }

            var animatorConfig = ConfigMgr.Instance.GetConfig<AnimatorConfig>("AnimatorConfig", animatorID);
            _gameObject.GetComponent<Animator>().runtimeAnimatorController = AssetMgr.Instance.LoadAsset<RuntimeAnimatorController>(animatorConfig.Controller);
        }

        private void InitStates()
        {
            _stateDic.Add("Jump", new JumpState("Jump", this));
            _stateDic.Add("Idle", new State("Idle", this));
            _stateDic.Add("Move", new MoveState("Move", this));
            _stateDic.Add("Attack", new AttackState("Attack", this));
            _stateDic.Add("Attacked", new AttackedState("Attacked", this));
            _stateDic.Add("Dead", new DeadState("Dead", this));
            _stateDic.Add("Cured", new CuredState("Cured", this));
            _stateDic.Add("Cure", new CureState("Cure", this));
            _curAnimState = "Idle";
            _stateDic[_curAnimState].Start();
        }

        protected virtual void CreateHUD()
        {
            _hpHUDKey = _id + "_HP";
            HUDManager.Instance.AddHUD("HUD", "HUDRole", _hpHUDKey, _hudPoint, new object[] { _id });
        }

        private void UpdateHexagonID(string id)
        {
            hexagonID = id;
        }


        public virtual void Update()
        {
            UpdatePosition();
        }

        public virtual bool IsDead()
        {
            return hp <= 0;
        }

        public virtual void UpdatePosition()
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
        }

        public virtual void Move(List<string> hexagons)
        {
            var count = hexagons.Count;
            if (count <= 0 || hexagons[count - 1] == hexagonID)
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

        public virtual void Attacked(float attackPower)
        {
            EnterState("Attacked");

            UpdateHP(hp - attackPower + GetDefensePower());
        }

        public virtual void Cure()
        {
            EnterState("Cure");
        }

        public virtual void Cured(float curePower)
        {
            EnterState("Cured");

            UpdateHP(hp + curePower);
        }

        public virtual float GetCurePower()
        {
            return GetAttribute(Enum.AttrType.Cure);
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
            return _state;
        }

        public void SetState(Enum.RoleState state)
        {
            if (state == _state)
                return;
            //DebugManager.Instance.Log(_id + "_" + state);
            _state = state;

            OnStateChanged();
        }

        protected virtual void OnStateChanged()
        {
            if (_state == Enum.RoleState.Over)
            {
                EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Attack_End, new object[] { _id });
            }
        }

        protected virtual void UpdateHP(float hp)
        {
            var hurt = hp - this.hp;
            this.hp = hp;
            HUDRole hud = HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
            hud.UpdateHP(this.hp);

            var numberID = ID + "_HUDNumber_" + _numberHUDList.Count;
            var numberHUD = (HUDNumber)HUDManager.Instance.AddHUD("HUD", "HUDNumber", numberID, _gameObject.transform.Find("hudPoint").gameObject);
            _numberHUDList.Add(numberID);
            numberHUD.Show(hurt, () =>
            {
                HUDManager.Instance.RemoveHUD(numberID);
                _numberHUDList.Remove(numberID);
            });

            if (this.hp <= 0)
            {
                Dead();
            }
        }

        public virtual float GetMoveDis()
        {
            return GetAttribute(Enum.AttrType.Move);
        }

        public virtual float GetAttackDis()
        {
            return GetAttribute(Enum.AttrType.AttackDis);
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
            var state = _stateDic[stateName];
            var method = typeof(State).GetMethod(secondStateName);
            if (secondStateName == "End")
                method.Invoke(state, new object[] { true });
            else
                method.Invoke(state, null);
        }

        public void NextPath()
        {
            //DebugManager.Instance.Log(_pathIndex + "_" + _path.Count);
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

        public float GetAttackPower()
        {
            return GetAttribute(Enum.AttrType.Attack);
        }

        public float GetDefensePower()
        {
            return GetAttribute(Enum.AttrType.Defense);
        }

        public Vector3 GetPosition()
        {
            return _gameObject.transform.position;
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
            return ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", _data.configId);
        }

        public RoleStarConfig GetStarConfig()
        {
            return ConfigMgr.Instance.GetConfig<RoleStarConfig>("RoleStarConfig", _data.configId * 1000 + _data.level);
        }

        public SkillConfig GetSkillConfig(Enum.SkillType skillType)
        {
            if (Enum.SkillType.Common == skillType)
            {
                return ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", GetConfig().CommonSkill);
            }
            else
            {
                return ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", GetConfig().SpecialSkill);
            }
        }

        /// <summary>
        ///获取指定技能的攻击对象 
        ///后面会考虑会不会设计类似迷惑类的debuff，会让攻击者有概率选择错误攻击目标
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public virtual Enum.RoleType GetTargetType(Enum.SkillType skillType)
        {
            return GetSkillConfig(skillType).TargetType;
        }

        private float GetAttribute(Enum.AttrType attrType)
        {
            var value = 0.0F;
            //英雄属性
            var attrs = GetStarConfig().Attrs;
            foreach (var v in attrs)
            {
                if ((Enum.AttrType)v.id == attrType)
                {
                    value += v.value;
                    break;
                }
            }

            //天赋属性
            //暂未开发

            //装备属性
            if (null != _data.equipmentDic)
            {
                foreach (var v in _data.equipmentDic)
                {
                    var equipData = DatasMgr.Instance.GetEquipmentData(v.Value);
                    var equipConfig = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", equipData.configId);
                    if (null != equipConfig.Attrs)
                    {
                        foreach (var v1 in equipConfig.Attrs)
                        {
                            if ((Enum.AttrType)v1.id == attrType)
                            {
                                value += v1.value;
                                break;
                            }
                        }
                    }
                }
            }

            return value;
        }

        //添加buff
        public void AddBuffs(List<int> buffs)
        {
            var hpDelta = 0.0F;
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                var buffConfig = ConfigMgr.Instance.GetConfig<BufferConfig>("BufferConfig", buffs[i]);
                UpdateAttr(buffConfig.Attr);
                if (buffConfig.Duration - 1 > 0)
                {
                    _buffs.Add(new Pair(buffs[i], buffConfig.Duration - 1));
                }
            }

            var hud = HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
            hud.UpdateBuffs(_buffs);
        }

        protected virtual void ExcuteBuffs()
        {
            for (int i = _buffs.Count - 1; i >= 0; i--)
            {
                var buffConfig = ConfigMgr.Instance.GetConfig<BufferConfig>("BufferConfig", _buffs[i].id);
                UpdateAttr(buffConfig.Attr);

                if (_buffs[i].value - 1 <= 0)
                {
                    _buffs.RemoveAt(i);
                }
                else
                {
                    _buffs[i] = new Pair(_buffs[i].id, _buffs[i].value - 1);
                }
            }

            var hud = HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
            hud.UpdateBuffs(_buffs);
        }

        public List<int> GetAttackBuffs()
        {
            var buffs = new List<int>();

            if (null != _data.equipmentDic)
            {
                foreach (var v in _data.equipmentDic)
                {
                    var equipData = DatasMgr.Instance.GetEquipmentData(v.Value);
                    var equipConfig = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", equipData.configId);
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

        private void UpdateAttr(Pair attr)
        {
            switch ((Enum.AttrType)attr.id)
            {
                case Enum.AttrType.Attack:
                    break;
                case Enum.AttrType.Cure:
                    break;
                case Enum.AttrType.Defense:
                    break;
                case Enum.AttrType.Move:
                    break;
                case Enum.AttrType.AttackDis:
                    break;
                case Enum.AttrType.Blood:
                    UpdateHP(this.hp - attr.value);
                    break;
            }
        }

        public virtual void UpdateRound()
        {
            ExcuteBuffs();
        }

        public override void ChangeToMapSpace()
        {
            var hexagon = MapManager.Instance.GetHexagon(hexagonID);
            var pos = MapTool.Instance.GetPosFromCoor(hexagon.coor) + _offset;
            _gameObject.transform.position = pos;
        }

        public void SetHPVisible(bool visible)
        {
            var hud = HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
            hud.SetVisible(visible);
        }

        public virtual void Dispose()
        {
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

            GameObject.Destroy(_gameObject);
            _gameObject = null;
        }
    }
}
