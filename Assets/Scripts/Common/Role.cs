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

        protected Animator _animator;

        protected string _hpHUDKey;

        protected float _speed = 5.0f;

        public string hexagonID;

        protected List<string> _numberHUDList = new List<string>();

        private Vector3 _offset = new Vector3(0.0f, 0.2f, 0.0f);

        protected Enum.RoleState _state;

        private Quaternion _rotation;

        protected Enum.RoleType _type = Enum.RoleType.None;

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

            CreateHUD();
        }

        protected virtual void CreateHUD()
        {
            _hpHUDKey = _id + "_HP";
            HUDManager.Instance.AddHUD("HUD", "HUDRole", _hpHUDKey, _gameObject.transform.Find("hudPoint").gameObject, new object[] { _id });
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

            _lerpStep += (Time.deltaTime * _speed);

            var startHexagon = MapManager.Instance.GetHexagon(_path[_pathIndex]);
            var endHexagon = MapManager.Instance.GetHexagon(_path[_pathIndex + 1]);
            var startPos = MapTool.Instance.GetPosFromCoor(startHexagon.coordinate) + _offset;
            var endPos = MapTool.Instance.GetPosFromCoor(endHexagon.coordinate) + _offset;

            //在Unity中使用插值来实现对象的平滑转向时，确实会遇到在背后转向时出现的突然变化问题。这是因为角度插值的方式不能很好地处理角度的360度环绕，从而导致了在180度处发生不连续性。
            var newPos = Vector3.Lerp(startPos, endPos, _lerpStep);
            _gameObject.transform.rotation = Quaternion.Lerp(_rotation, Quaternion.LookRotation((endPos - startPos).normalized), _lerpStep);
            _gameObject.transform.position = newPos;

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
                    MoveEnd();
                }
            }
        }

        public virtual void Move(List<string> hexagons)
        {
            var count = hexagons.Count;
            if (count <= 0 || hexagons[count - 1] == hexagonID)
                return;

            this._path = hexagons;
            _animator.Play("Move");
        }

        public virtual void MoveEnd()
        {
            Idle();
            EventDispatcher.Instance.Dispatch(Enum.EventType.Role_MoveEnd_Event);
        }

        public virtual void Attack()
        {
            _animator.Play("Attack");
        }

        public virtual void Attacked(float hurt)
        {
            _animator.Play("Attacked");
            UpdateHP(_attribute.hp - hurt);
        }

        public virtual void Dead()
        {
            _animator.Play("Die");
        }

        public virtual void Idle()
        {
            _animator.Play("Idle");
        }

        public virtual void Jump()
        {
            _animator.Play("Jump");
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
