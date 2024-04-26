using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class State
    {
        protected string _name;
        protected Enum.RoleAnimState _state = Enum.RoleAnimState.End;
        protected Role _role = null;
        protected float _lerpStep = 0;

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
                lastState.End();
            }
            DebugManager.Instance.Log(_name + "_Start");
            _state = Enum.RoleAnimState.Start;
            _role.Animator.SetBool(_name, true);
        }

        public virtual void Take()
        {
            if (_state == Enum.RoleAnimState.Take)
                return;
            DebugManager.Instance.Log(_name + "_Take");
            _state = Enum.RoleAnimState.Take;
        }

        public virtual void Loss()
        {
            if (_state == Enum.RoleAnimState.Loss)
                return;
            DebugManager.Instance.Log(_name + "_Loss");
            _state = Enum.RoleAnimState.Loss;
        }

        public virtual void End()
        {
            if (_state == Enum.RoleAnimState.End)
                return;
            DebugManager.Instance.Log(_name + "_End");
            _state = Enum.RoleAnimState.End;
            _role.Animator.SetBool(_name, false);
        }

        public virtual void Update()
        {
        }
    }

    public class JumpState : State
    {
        private float _speed = 0;

        public JumpState(string name, Role role) : base(name, role)
        { }

        public override void Take()
        {
            if (_state == Enum.RoleAnimState.Take)
                return;
            base.Take();

            _lerpStep = 0;
            var startHexagon = MapManager.Instance.GetHexagon(_role.Path[_role.PathIndex]);
            var endHexagon = MapManager.Instance.GetHexagon(_role.Path[_role.PathIndex + 1]);
            var startPos = MapTool.Instance.GetPosFromCoor(startHexagon.coordinate) + _role.Offset;
            var endPos = MapTool.Instance.GetPosFromCoor(endHexagon.coordinate) + _role.Offset;

            var timeDic = Tool.Instance.GetEventTimeForAnimClip(_role.Animator, "JumpFull_Spin_RM_SwordAndShield");
            var clips = _role.Animator.runtimeAnimatorController.animationClips;
            _speed = Vector3.Distance(endPos, startPos) / (timeDic["Jump_Loss"] - timeDic["Jump_Take"]);
        }

        public override void End()
        {
            if (_state == Enum.RoleAnimState.End)
                return;

            base.End();
            _role.LerpStep = 0;

            _role.NextPath();
        }

        public override void Update()
        {
            if (_state == Enum.RoleAnimState.Start || _state == Enum.RoleAnimState.Loss || _state == Enum.RoleAnimState.End)
                return;

            var startHexagon = MapManager.Instance.GetHexagon(_role.Path[_role.PathIndex]);
            var endHexagon = MapManager.Instance.GetHexagon(_role.Path[_role.PathIndex + 1]);

            var startPos = MapTool.Instance.GetPosFromCoor(startHexagon.coordinate) + _role.Offset;
            var endPos = MapTool.Instance.GetPosFromCoor(endHexagon.coordinate) + _role.Offset;

            _lerpStep += (Time.deltaTime * _speed);

            //在Unity中使用插值来实现对象的平滑转向时，确实会遇到在背后转向时出现的突然变化问题。这是因为角度插值的方式不能很好地处理角度的360度环绕，从而导致了在180度处发生不连续性。
            var newPos = Vector3.Lerp(startPos, endPos, _lerpStep);
            _role.GameObject.transform.rotation = Quaternion.Lerp(_role.Rotation, Quaternion.LookRotation((endPos - startPos).normalized), _lerpStep);
            _role.GameObject.transform.position = newPos;
        }
    }

    public class MoveState : State
    {
        private float _speed = 5f;

        public MoveState(string name, Role role) : base(name, role)
        {

        }

        public override void Update()
        {
            var startHexagon = MapManager.Instance.GetHexagon(_role.Path[_role.PathIndex]);
            var endHexagon = MapManager.Instance.GetHexagon(_role.Path[_role.PathIndex + 1]);

            var startPos = MapTool.Instance.GetPosFromCoor(startHexagon.coordinate) + _role.Offset;
            var endPos = MapTool.Instance.GetPosFromCoor(endHexagon.coordinate) + _role.Offset;

            _lerpStep += (Time.deltaTime * _speed);

            //在Unity中使用插值来实现对象的平滑转向时，确实会遇到在背后转向时出现的突然变化问题。这是因为角度插值的方式不能很好地处理角度的360度环绕，从而导致了在180度处发生不连续性。
            var newPos = Vector3.Lerp(startPos, endPos, _lerpStep);
            _role.GameObject.transform.rotation = Quaternion.Lerp(_role.Rotation, Quaternion.LookRotation((endPos - startPos).normalized), _lerpStep);
            _role.GameObject.transform.position = newPos;

            if (_lerpStep >= 1)
            {
                _lerpStep = 0;
                _role.NextPath();
            }
        }
    }
}
