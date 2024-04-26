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

    public class StateInfo
    {
        private string _name;
        private Animator _animator;
        private Enum.RoleAnimState _state = Enum.RoleAnimState.End;
        private StateInfo _lastState = null;

        public StateInfo(string name, Animator animator)
        {
            this._name = name;
            this._animator = animator;
        }

        public void Start(StateInfo lastState = null)
        {
            if (null != lastState)
            {
                lastState.End();
                _lastState = lastState;
            }
            _state = Enum.RoleAnimState.Start;
            _animator.SetBool(_name, true);
        }

        public void Take()
        {
            _state = Enum.RoleAnimState.Take;
        }

        public void Loss()
        {
            _state = Enum.RoleAnimState.Loss;
        }

        public void End()
        {
            _state = Enum.RoleAnimState.End;
            _animator.SetBool(_name, false);
            if (null != _lastState)
            {
                _lastState.Start();
                _lastState = null;
            }
        }
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

        private Dictionary<string, StateInfo> _stateDic = new Dictionary<string, StateInfo>();

        private Enum.RoleAnimState _jumpState = Enum.RoleAnimState.End;

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

            _stateDic.Add("Jump", new StateInfo("Jump", _animator));
            _stateDic.Add("Idle", new StateInfo("Idle", _animator));
            _stateDic.Add("Move", new StateInfo("Move", _animator));
            _stateDic.Add("Attack", new StateInfo("Attack", _animator));
            _stateDic.Add("Attacked", new StateInfo("Attacked", _animator));

            CreateHUD();
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

            if (_jumpState == Enum.RoleAnimState.Start || _jumpState == Enum.RoleAnimState.Loss)
                return;

            var startHexagon = MapManager.Instance.GetHexagon(_path[_pathIndex]);
            var endHexagon = MapManager.Instance.GetHexagon(_path[_pathIndex + 1]);

            //如果需要跨越不同高度地块，切换到跳跃动作
            if (_jumpState == Enum.RoleAnimState.End && endHexagon.coordinate.y != startHexagon.coordinate.y)
            {
                StartJump();
            }
            else
            {
                var startPos = MapTool.Instance.GetPosFromCoor(startHexagon.coordinate) + _offset;
                var endPos = MapTool.Instance.GetPosFromCoor(endHexagon.coordinate) + _offset;

                //if (_jumpState != Enum.RoleAnimState.End)
                //    return;

                if (_jumpState == Enum.RoleAnimState.Take)
                    _lerpStep += (Time.deltaTime * _jumpSpeed);
                else
                    _lerpStep += (Time.deltaTime * _moveSpeed);

                //在Unity中使用插值来实现对象的平滑转向时，确实会遇到在背后转向时出现的突然变化问题。这是因为角度插值的方式不能很好地处理角度的360度环绕，从而导致了在180度处发生不连续性。
                var newPos = Vector3.Lerp(startPos, endPos, _lerpStep);
                _gameObject.transform.rotation = Quaternion.Lerp(_rotation, Quaternion.LookRotation((endPos - startPos).normalized), _lerpStep);
                _gameObject.transform.position = newPos;

                if (_jumpState == Enum.RoleAnimState.Take)
                {
                    return;
                }
                if (Stop())
                {
                    MoveEnd();
                }
            }
        }

        private bool Stop()
        {
            //if (null == _path || _pathIndex >= _path.Count)
            //    return true;

            if (_lerpStep >= 1)
            {
                _lerpStep = 0;
                _pathIndex++;
                _rotation = _gameObject.transform.rotation;
                UpdateHexagonID(_path[_pathIndex]);
                if (_pathIndex >= _path.Count - 1)
                {
                    _path = null;
                    _pathIndex = 0;
                    return true;
                }
            }
            return false;
        }
        public virtual void Move(List<string> hexagons)
        {
            var count = hexagons.Count;
            if (count <= 0 || hexagons[count - 1] == hexagonID)
                return;

            this._path = hexagons;
            _animator.SetBool("Idle", false);
            _animator.SetBool("Move", true);
        }

        public virtual void MoveEnd()
        {
            _animator.SetBool("Move", false);
            Idle();
            EventDispatcher.Instance.Dispatch(Enum.EventType.Role_MoveEnd_Event);
        }

        public virtual void Attack()
        {
            //_animator.Play("Attack");
            _animator.SetBool("Attack", true);
        }

        public virtual void Attacked(float hurt)
        {
            //_animator.Play("Attacked");
            _animator.SetBool("Attacked", true);
            UpdateHP(_attribute.hp - hurt);
        }

        public virtual void Dead()
        {
            //_animator.Play("Die");
            _animator.SetBool("Die", true);
        }

        public virtual void Idle()
        {
            DebugManager.Instance.Log("Idle");
            //_animator.Play("Idle");
            _animator.SetBool("Move", false);
            _animator.SetBool("Jump", false);
            _animator.SetBool("Idle", true);
        }

        public virtual void StartJump()
        {
            DebugManager.Instance.Log("StartJump");
            _animator.SetBool("Move", false);
            _animator.SetBool("Idle", false);
            _animator.SetBool("Jump", true);
            _jumpState = Enum.RoleAnimState.Start;
        }

        public virtual void TakeJump()
        {
            DebugManager.Instance.Log("TakeJump");
            if (null == _path || _pathIndex > _path.Count - 1)
                return;
            var startHexagon = MapManager.Instance.GetHexagon(_path[_pathIndex]);
            var endHexagon = MapManager.Instance.GetHexagon(_path[_pathIndex + 1]);
            var startPos = MapTool.Instance.GetPosFromCoor(startHexagon.coordinate) + _offset;
            var endPos = MapTool.Instance.GetPosFromCoor(endHexagon.coordinate) + _offset;
            var clips = _animator.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i].name == "JumpFull_Spin_RM_SwordAndShield")
                {
                    float startTime = 0, endTime = 0;
                    for (int j = 0; j < clips[i].events.Length; j++)
                    {
                        if (clips[i].events[j].stringParameter == "Jump_Take")
                            startTime = clips[i].events[j].time;
                        if (clips[i].events[j].stringParameter == "Jump_Loss")
                            endTime = clips[i].events[j].time;
                    }
                    _jumpSpeed = Vector3.Distance(endPos, startPos) / (endTime - startTime);
                    break;
                }
            }
            _jumpState = Enum.RoleAnimState.Take;
        }

        public virtual void LossJump()
        {
            DebugManager.Instance.Log("LossJump");
            _jumpState = Enum.RoleAnimState.Loss;
        }

        public virtual void EndJump()
        {
            DebugManager.Instance.Log("EndJump");
            _animator.SetBool("Jump", false);
            _jumpState = Enum.RoleAnimState.End;
            _lerpStep = 1;
            if (Stop())
            {
                MoveEnd();
            }
            else
            {
                _animator.SetBool("Move", true);
            }
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
