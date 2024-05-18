using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;

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

        public string hexagonID;

        protected List<string> _numberHUDList = new List<string>();

        private Vector3 _offset = new Vector3(0.0f, 0.224f, 0.0f);

        protected Enum.RoleState _state;

        private Quaternion _rotation;

        protected Enum.RoleType _type = Enum.RoleType.None;

        private Dictionary<string, State> _stateDic = new Dictionary<string, State>();

        private string _curAnimState = null;

        protected Dictionary<Enum.EquipPlace, Equip> _equipDic = new Dictionary<Enum.EquipPlace, Equip>();

        private Vector3 _bornPoint;

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

            _bornPoint = MapTool.Instance.GetPosFromCoor(MapManager.Instance.GetHexagon(hexagonID).coor) + _offset;
            CreateGO();//加载方式，同步方式，后面都要改
        }

        private void CreateGO()
        {
            var assetPath = GetConfig().Prefab;
            GameObject prefab = AssetMgr.Instance.LoadAsset<GameObject>(assetPath);
            _gameObject = GameObject.Instantiate(prefab);

            OnCreate();
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            _gameObject.transform.position = _bornPoint;
            _gameObject.transform.localScale = Vector3.one * 0.7F;
            _animator = _gameObject.GetComponent<Animator>();
            _gameObject.GetComponent<RoleBehaviour>().ID = _id;
            _rotation = _gameObject.transform.rotation;
            _hudPoint = _gameObject.transform.Find("hudPoint").gameObject;


            InitEquips();
            InitAnimator();
            InitStates();
            CreateHUD();
        }

        protected override void SmoothNormal()
        {
            Tool.Instance.ApplyProcessingFotOutLine(_gameObject, new List<string> { "Body", "Hair", "Head", "Hat", "AC"});
        }

        protected virtual void InitEquips()
        {

        }

        protected virtual void InitAnimator()
        {
            var animatorConfig = GetAnimatorConfig();
            _gameObject.GetComponent<Animator>().runtimeAnimatorController = AssetMgr.Instance.LoadAsset<RuntimeAnimatorController>(animatorConfig.Controller);
        }

        private void InitStates()
        {
            _stateDic.Add("Jump", new JumpState("Jump", this));
            var clip = GetAnimatorConfig().Jump;
            var timeDic = Tool.Instance.GetEventTimeForAnimClip(_animator, clip);
            //DebugManager.Instance.Log(GetAnimatorConfig().Controller + clip);
            //foreach (var v in timeDic)
            //    DebugManager.Instance.Log(v.Key);
            ((JumpState)_stateDic["Jump"]).duration = timeDic["Jump_Loss"] - timeDic["Jump_Take"];

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

        public AnimatorConfig GetAnimatorConfig()
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
            return ConfigMgr.Instance.GetConfig<AnimatorConfig>("AnimatorConfig", animatorID);
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
            return _data.GetAttribute(Enum.AttrType.HP) <= 0;
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
            EventDispatcher.Instance.PostEvent(Enum.EventType.Role_MoveEnd_Event);
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

        public virtual void Hit(float attackPower)
        {
            //var prefabPath = "Assets/JMO Assets/Cartoon FX(legacy)/CFX Prefabs/Hits/CFX_Hit_A Red+RandomText.prefab";
            var prefabPath = "Assets/Prefabs/Effects/CFX_Hit_A Red+RandomText.prefab";
            var hitPrefab = GameObject.Instantiate(AssetMgr.Instance.LoadAsset<GameObject>(prefabPath));
            hitPrefab.transform.position = _gameObject.transform.position + new Vector3(0, 0.8f, 0);

            EnterState("Attacked");

            UpdateHP(_data.GetAttribute(Enum.AttrType.HP) - attackPower + GetDefensePower());
        }

        public virtual void Cure()
        {
            EnterState("Cure");
        }

        public virtual void Cured(float curePower)
        {
            EnterState("Cured");

            UpdateHP(_data.GetAttribute(Enum.AttrType.HP) + curePower);
        }

        public virtual float GetCurePower()
        {
            return _data.GetAttribute(Enum.AttrType.Cure);
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
            _state = state;

            OnStateChanged();
        }

        protected virtual void OnStateChanged()
        {
        }

        protected virtual void UpdateHP(float hp)
        {
            var hurt = hp - _data.GetAttribute(Enum.AttrType.HP);
            _data.UpdateAttr(Enum.AttrType.HP, hp);

            HUDRole hud = HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
            hud.UpdateHP(_data.GetAttribute(Enum.AttrType.HP));

            var numberID = ID + "_HUDNumber_" + _numberHUDList.Count;
            var numberHUD = (HUDNumber)HUDManager.Instance.AddHUD("HUD", "HUDNumber", numberID, _gameObject.transform.Find("hudPoint").gameObject);
            _numberHUDList.Add(numberID);
            numberHUD.Show(hurt, () =>
            {
                HUDManager.Instance.RemoveHUD(numberID);
                _numberHUDList.Remove(numberID);
            });

            if (_data.GetAttribute(Enum.AttrType.HP) <= 0)
            {
                Dead();
            }
        }

        public virtual float GetMoveDis()
        {
            return _data.GetAttribute(Enum.AttrType.MoveDis);
        }

        public virtual float GetAttackDis()
        {
            return _data.GetAttribute(Enum.AttrType.AttackDis);
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
            return _data.GetAttribute(Enum.AttrType.PhysicalAttack);
        }

        public float GetDefensePower()
        {
            return _data.GetAttribute(Enum.AttrType.PhysicalDefense);
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
            return _data.GetConfig();
        }

        public RoleStarConfig GetStarConfig()
        {
            return _data.GetStarConfig();
        }


        //添加buff
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
            hud.UpdateBuffs(_data.buffs);
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

        public virtual void UpdateRound()
        {
            ExcuteBuffs();
        }

        public override void ChangeToMapSpace()
        {
            base.ChangeToMapSpace();

            var hexagon = MapManager.Instance.GetHexagon(hexagonID);
            var pos = MapTool.Instance.GetPosFromCoor(hexagon.coor) + _offset;
            _gameObject.transform.position = pos;
        }

        public void SetHPVisible(bool visible)
        {
            var hud = HUDManager.Instance.GetHUD<HUDRole>(_hpHUDKey);
            hud.SetVisible(visible);
        }

        public float GetHP()
        {
            return _data.GetAttribute(Enum.AttrType.HP);
        }

        public override void HighLight()
        {
            SetMaterial(_gameObject, 0, Color.white);
        }

        public override void ResetHighLight()
        {
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

        public override void Dispose()
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

            base.Dispose();
        }
    }
}
