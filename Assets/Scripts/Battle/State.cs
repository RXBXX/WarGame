using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace WarGame
{
    public class State
    {
        protected string _name;
        protected Enum.RoleAnimState _state = Enum.RoleAnimState.End;
        protected Role _role = null;
        protected float _lerpStep = 0;
        protected State _last;

        public State(string name, Role role)
        {
            this._name = name;
            this._role = role;
        }

        public virtual void Start(State lastState = null)
        {
            if (_state == Enum.RoleAnimState.Start)
                return;
            if (lastState == this)
                return;
            if (null != lastState)
            {
                //DebugManager.Instance.Log(lastState._name);
                _last = lastState;
                lastState.End(false);
            }

            //DebugManager.Instance.Log(_name + "_Start_" + _role.ID);
            _role.SetAnimState(_name);
            _state = Enum.RoleAnimState.Start;
            _role.Animator.SetBool(_name, true);
        }

        public virtual void Take()
        {
            if (_state == Enum.RoleAnimState.Take)
                return;
            _state = Enum.RoleAnimState.Take;
        }

        public virtual void Loss()
        {
            if (_state == Enum.RoleAnimState.Loss)
                return;
            _state = Enum.RoleAnimState.Loss;
        }

        public virtual void End(bool reverse)
        {
            if (_state == Enum.RoleAnimState.End)
                return;

            //DebugManager.Instance.Log("StateEnd" + reverse);

            _state = Enum.RoleAnimState.End;
            _role.Animator.SetBool(_name, false);
            //DebugManager.Instance.Log(_name + ":" + _role.Animator.GetBool(_name));
            //if (_name.Equals("Jump"))
            //{
            //    DebugManager.Instance.Log("Jump Pause:");
            //    UnityEditor.EditorApplication.isPaused = true;
            //}
            if (reverse)
            {
                //DebugManager.Instance.Log("StateEnd" + _last._name) ;
                _last.Start();
                _last = null;
            }
            //DebugManager.Instance.Log(_name + "_End_" + _role.ID);
        }

        public virtual void Update()
        {
        }

        public virtual void Dispose()
        {
            _role = null;
            _last = null;
        }
    }

    public class JumpState : State
    {
        public float duration = 0;
        private float _speed = 0;

        public JumpState(string name, Role role) : base(name, role)
        { }

        public override void Take()
        {
            //DebugManager.Instance.Log("JumpTake:" + _state);
            if (_state == Enum.RoleAnimState.Take)
                return;
            base.Take();

            _lerpStep = 0;
            var startHexagon = MapManager.Instance.GetHexagon(_role.Path[_role.PathIndex]);
            var endHexagon = MapManager.Instance.GetHexagon(_role.Path[_role.PathIndex + 1]);
            var startPos = MapTool.Instance.GetPosFromCoor(startHexagon.coor) + CommonParams.RoleOffset;
            var endPos = MapTool.Instance.GetPosFromCoor(endHexagon.coor) + CommonParams.RoleOffset;
            _speed = Vector3.Distance(endPos, startPos) / duration;
        }

        public override void End(bool reverse)
        {
            if (_state == Enum.RoleAnimState.End)
                return;

            base.End(reverse);
            _role.NextPath();
        }

        public override void Update()
        {
            //DebugManager.Instance.Log("JumpState:" + _state);
            if (_state == Enum.RoleAnimState.Start || _state == Enum.RoleAnimState.Loss || _state == Enum.RoleAnimState.End)
                return;

            var startHexagon = MapManager.Instance.GetHexagon(_role.Path[_role.PathIndex]);
            var endHexagon = MapManager.Instance.GetHexagon(_role.Path[_role.PathIndex + 1]);

            var startPos = MapTool.Instance.GetPosFromCoor(startHexagon.coor) + CommonParams.RoleOffset;
            var endPos = MapTool.Instance.GetPosFromCoor(endHexagon.coor) + CommonParams.RoleOffset;

            _lerpStep += (Time.deltaTime * _speed);

            //在Unity中使用欧拉角插值来实现对象的平滑转向时，确实会遇到在背后转向时出现的突然变化问题。这是因为角度插值的方式不能很好地处理角度的360度环绕，从而导致了在180度处发生不连续性。
            //var newPos = Vector3.Lerp(startPos, endPos, _lerpStep);
            //_role.GameObject.transform.rotation = Quaternion.Lerp(_role.Rotation, Quaternion.LookRotation((endPos - startPos).normalized), _lerpStep);
            //_role.GameObject.transform.position = newPos;
            _role.UpdatePosition(Vector3.Lerp(startPos, endPos, _lerpStep));
            var forward = endPos - startPos;
            forward.y = 0;
            _role.UpdateRotation(Quaternion.Lerp(_role.Rotation, Quaternion.LookRotation(forward.normalized), _lerpStep));
        }
    }

    public class MoveState : State
    {
        private float _speed = 3.0f;

        public MoveState(string name, Role role) : base(name, role)
        {

        }

        public override void Update()
        {
            var startHexagon = MapManager.Instance.GetHexagon(_role.Path[_role.PathIndex]);
            var endHexagon = MapManager.Instance.GetHexagon(_role.Path[_role.PathIndex + 1]);

            var startPos = MapTool.Instance.GetPosFromCoor(startHexagon.coor) + CommonParams.RoleOffset;
            var endPos = MapTool.Instance.GetPosFromCoor(endHexagon.coor) + CommonParams.RoleOffset;

            _lerpStep += (Time.deltaTime * _speed);

            //在Unity中使用插值来实现对象的平滑转向时，确实会遇到在背后转向时出现的突然变化问题。这是因为角度插值的方式不能很好地处理角度的360度环绕，从而导致了在180度处发生不连续性。
            _role.UpdatePosition(Vector3.Lerp(startPos, endPos, _lerpStep));
            var forward = endPos - startPos;
            forward.y = 0;
            _role.UpdateRotation(Quaternion.Lerp(_role.Rotation, Quaternion.LookRotation(forward.normalized), _lerpStep));

            //var newPos = Vector3.Lerp(startPos, endPos, _lerpStep);
            //_role.GameObject.transform.rotation = Quaternion.Lerp(_role.Rotation, Quaternion.LookRotation((endPos - startPos).normalized), _lerpStep);
            //_role.GameObject.transform.position = newPos;

            //DebugManager.Instance.Log(_lerpStep);
            if (_lerpStep >= 1)
            {
                _lerpStep = 0;
                _role.NextPath();
            }
        }
    }

    public class AttackState : State
    {
        public AttackState(string name, Role role) : base(name, role)
        { }

        public override void End(bool reverse)
        {
            base.End(reverse);
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Attack_End, new object[] { _role.ID });
        }
    }

    public class AttackedState : State
    {
        public AttackedState(string name, Role role) : base(name, role)
        {
        }

        public override void End(bool reverse)
        {
            //DebugManager.Instance.Log("AttackEnd");

            base.End(reverse);
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Attacked_End, new object[] { _role.ID });
        }
    }

    public class DeadState : State
    {
        public DeadState(string name, Role role) : base(name, role)
        { }

        public override void End(bool reverse)
        {
            //base.End(reverse);
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Dead_End, new object[] { _role.ID });
        }
    }

    public class CureState : State
    {
        public CureState(string name, Role role) : base(name, role)
        { }

        public override void End(bool reverse)
        {
            base.End(reverse);
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Cure_End, new object[] { _role.ID });
        }
    }

    public class CuredState : State
    {
        public CuredState(string name, Role role) : base(name, role)
        {
        }

        public override void End(bool reverse)
        {
            base.End(reverse);
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Cured_End, new object[] { _role.ID });
        }
    }

    public class DodgeState : State
    {
        public DodgeState(string name, Role role) : base(name, role)
        {
        }

        public override void Take()
        {
            base.Take();
            //_role.GameObject.transform.DOMove(_role.GetPosition() - _role.GameObject.transform.forward * 0.2F, 0.2F);
        }

        public override void End(bool reverse)
        {
            base.End(reverse);
            EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Dodge_End, new object[] { _role.ID });
        }
    }
}
