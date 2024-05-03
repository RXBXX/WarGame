using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;

namespace WarGame
{
    public class Role
    {
        protected int _id;

        protected RoleData _data;

        protected int _starConfigId = 1;

        private List<string> _path;

        private int _pathIndex;

        private float _lerpStep = 0;

        protected GameObject _gameObject;

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

        protected int _layer = 7;

        public float hp; //血量

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

        public Role(RoleData data, string hexagonID)
        {
            this._id = data.UID;
            this._data = data;
            this.hexagonID = hexagonID;
            this.hp = GetStarConfig().HP;

            var bornPoint = MapTool.Instance.GetPosFromCoor(MapManager.Instance.GetHexagon(hexagonID).coor) + _offset;
            OnCreate(bornPoint);//加载方式，同步方式，后面都要改

            DebugManager.Instance.Log(_state);
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

            InitStates();
            CreateHUD();
        }

        private void InitStates()
        {
            _stateDic.Add("Jump", new JumpState("Jump", this));
            _stateDic.Add("Idle", new State("Idle", this));
            _stateDic.Add("Move", new MoveState("Move", this));
            _stateDic.Add("Attack", new AttackState("Attack", this));
            _stateDic.Add("Attacked", new AttackedState("Attacked", this));
            _stateDic.Add("Dead", new DeadState("Dead", this));
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

        public virtual void Attack()
        {
            EnterState("Attack");
        }

        public virtual void Attacked(float hurt)
        {
            EnterState("Attacked");

            UpdateHP(hp - hurt);
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
            DebugManager.Instance.Log(_id + "_" + state);
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
            var hurt = this.hp - hp;
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
            return GetStarConfig().MoveDis;
        }

        public virtual float GetAttackDis()
        {
            return GetStarConfig().AttackDis;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual float GetViewDis()
        {
            return GetStarConfig().AttackDis;
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

        public void SetLayer(int layer)
        {
            SetLayerRecursion(_gameObject.transform, layer);
        }

        public void RecoverLayer()
        {
            SetLayerRecursion(_gameObject.transform, _layer);
        }

        private void SetLayerRecursion(Transform tran, int layer)
        {
            var childCount = tran.childCount;
            for (int i = 0; i < childCount; i++)
            {
                SetLayerRecursion(tran.GetChild(i), layer);
            }
            tran.gameObject.layer = layer;
        }

        public float GetAttackPower()
        {
            return GetStarConfig().Attack;
        }

        public float GetDefensePower()
        {
            return GetStarConfig().Defense;
        }

        public virtual Enum.RoleType GetTargetType()
        {
            return Enum.RoleType.None;
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

        public SkillConfig GetSkill(Enum.SkillType skillType)
        {
            if (Enum.SkillType.Common == skillType)
            {
                return ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", GetConfig().CommonSkill.id);
            }
            else
            {
                return ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", GetConfig().SpecialSkill.id);
            }
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

            GameObject.Destroy(_gameObject);
            _gameObject = null;
        }
    }
}
