using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using WarGame.UI;

namespace WarGame
{
    //角色属性
    public struct RoleAttribute
    {
        public float hp; //血量
        public float attack; //攻击
        public float defense; //防御
        public float attackDis; //攻击距离
        public float moveDis; //单次移动距离
    }


    public class Role
    {
        protected int _id;

        private RoleAttribute _attribute;

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

        private string _curState = null;
        //private Enum.RoleAnimState _jumpState = Enum.RoleAnimState.End;

        public int ID
        {
            set { }
            get { return _id; }
        }

        public RoleAttribute Attribute
        {
            get { return _attribute; }
        }

        public Enum.RoleState State
        {
            get { return _state; }
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

        public Role(int id, RoleAttribute attribute, string assetPath, string hexagonID)
        {
            this._id = id;
            this._attribute = attribute;
            this.hexagonID = hexagonID;

            var bornPoint = MapTool.Instance.GetPosFromCoor(MapManager.Instance.GetHexagon(hexagonID).coordinate) + _offset;
            OnCreate(assetPath, bornPoint);//加载方式，同步方式，后面都要改
        }

        protected virtual void OnCreate(string assetPath, Vector3 bornPoint)
        {
            GameObject prefab = AssetMgr.Instance.LoadAsset<GameObject>(assetPath);
            _gameObject = GameObject.Instantiate(prefab);
            _gameObject.transform.position = bornPoint;
            _gameObject.transform.localScale = Vector3.one * 0.6F;
            _animator = _gameObject.GetComponent<Animator>();
            _gameObject.GetComponent<RoleData>().ID = _id;
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
            _stateDic.Add("Attack", new State("Attack", this));
            _stateDic.Add("Attacked", new State("Attacked", this));
            _stateDic.Add("Dead", new State("Dead", this));
            _curState = "Idle";
            _stateDic[_curState].Start();
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
            if (_state != Enum.RoleState.Attacking)
                return;

            UpdatePosition();

        }

        public virtual bool IsDead()
        {
            return _attribute.hp <= 0;
        }

        public virtual void UpdatePosition()
        {
            if (null == _gameObject)
                return;

            if (null == _path || _path.Count <= 0)
                return;

            var startHexagon = MapManager.Instance.GetHexagon(_path[_pathIndex]);
            var endHexagon = MapManager.Instance.GetHexagon(_path[_pathIndex + 1]);

            if (startHexagon.coordinate.y != endHexagon.coordinate.y)
            {
                EnterState("Jump");
            }
            else
            {
                EnterState("Move");
            }

            _stateDic[_curState].Update();
        }

        private void EnterState(string stateName)
        {
            if (stateName == _curState)
                return;
            var curState = _stateDic[_curState];
            _curState = stateName;
            _stateDic[_curState].Start(curState);
        }

        public virtual void Stop()
        {
            EnterState("Idle");
            EventDispatcher.Instance.Dispatch(Enum.EventType.Role_MoveEnd_Event);
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

            UpdateHP(_attribute.hp - hurt);
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

        public virtual void NextState()
        {
            if (_state + 1 > Enum.RoleState.AttackOver)
                _state = Enum.RoleState.Waiting;
            else
                _state += 1;

            OnStateChanged();
        }

        public virtual void OnStateChanged()
        {

        }

        public virtual void UpdateHP(float hp)
        {
            var hurt = _attribute.hp - hp;
            _attribute.hp = hp;
            HUDRole hud = (HUDRole)HUDManager.Instance.GetHUD(_hpHUDKey);
            hud.UpdateHP(hp);

            var numberID = ID + "_HUDNumber_" + _numberHUDList.Count;
            var numberHUD = (HUDNumber)HUDManager.Instance.AddHUD("HUD", "HUDNumber", numberID, _gameObject.transform.Find("hudPoint").gameObject);
            _numberHUDList.Add(numberID);
            numberHUD.Show(hurt, () =>
            {
                HUDManager.Instance.RemoveHUD(numberID);
                _numberHUDList.Remove(numberID);
            });

            if (_attribute.hp <= 0)
            {
                Dead();
            }
        }

        public virtual float GetMoveDis()
        {
            return _attribute.moveDis;
        }

        public virtual float GetAttackDis()
        {
            return _attribute.attackDis;
        }

        public void HandleEvent(string stateName, string secondStateName)
        {
            var state = _stateDic[stateName];
            var method = typeof(State).GetMethod(secondStateName);
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
                Stop();
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
