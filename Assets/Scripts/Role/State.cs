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
                _last = lastState;
                lastState.End(false);
            }
            DebugManager.Instance.Log(_name + "_Start_" + _role.ID);
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
            if (reverse)
            {
                _last.Start();
                _last = null;
            }
            DebugManager.Instance.Log(_name + "_End_" + _role.ID);
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
            var startPos = MapTool.Instance.GetPosFromCoor(startHexagon.coor) + _role.Offset;
            var endPos = MapTool.Instance.GetPosFromCoor(endHexagon.coor) + _role.Offset;

            var timeDic = Tool.Instance.GetEventTimeForAnimClip(_role.Animator, "JumpFull_Spin_RM_SwordAndShield");
            var clips = _role.Animator.runtimeAnimatorController.animationClips;
            _speed = Vector3.Distance(endPos, startPos) / (timeDic["Jump_Loss"] - timeDic["Jump_Take"]);
        }

        public override void End(bool reverse)
        {
            if (_state == Enum.RoleAnimState.End)
                return;

            base.End(reverse);
            _role.LerpStep = 0;

            _role.NextPath();
        }

        public override void Update()
        {
            if (_state == Enum.RoleAnimState.Start || _state == Enum.RoleAnimState.Loss || _state == Enum.RoleAnimState.End)
                return;

            var startHexagon = MapManager.Instance.GetHexagon(_role.Path[_role.PathIndex]);
            var endHexagon = MapManager.Instance.GetHexagon(_role.Path[_role.PathIndex + 1]);

            var startPos = MapTool.Instance.GetPosFromCoor(startHexagon.coor) + _role.Offset;
            var endPos = MapTool.Instance.GetPosFromCoor(endHexagon.coor) + _role.Offset;

            _lerpStep += (Time.deltaTime * _speed);

            //��Unity��ʹ�ò�ֵ��ʵ�ֶ����ƽ��ת��ʱ��ȷʵ�������ڱ���ת��ʱ���ֵ�ͻȻ�仯���⡣������Ϊ�ǶȲ�ֵ�ķ�ʽ���ܺܺõش����Ƕȵ�360�Ȼ��ƣ��Ӷ���������180�ȴ������������ԡ�
            var newPos = Vector3.Lerp(startPos, endPos, _lerpStep);
            _role.GameObject.transform.rotation = Quaternion.Lerp(_role.Rotation, Quaternion.LookRotation((endPos - startPos).normalized), _lerpStep);
            _role.GameObject.transform.position = newPos;
        }
    }

    public class MoveState : State
    {
        private float _speed = 3f;

        public MoveState(string name, Role role) : base(name, role)
        {

        }

        public override void Update()
        {
            var startHexagon = MapManager.Instance.GetHexagon(_role.Path[_role.PathIndex]);
            var endHexagon = MapManager.Instance.GetHexagon(_role.Path[_role.PathIndex + 1]);

            var startPos = MapTool.Instance.GetPosFromCoor(startHexagon.coor) + _role.Offset;
            var endPos = MapTool.Instance.GetPosFromCoor(endHexagon.coor) + _role.Offset;

            _lerpStep += (Time.deltaTime * _speed);

            //��Unity��ʹ�ò�ֵ��ʵ�ֶ����ƽ��ת��ʱ��ȷʵ�������ڱ���ת��ʱ���ֵ�ͻȻ�仯���⡣������Ϊ�ǶȲ�ֵ�ķ�ʽ���ܺܺõش����Ƕȵ�360�Ȼ��ƣ��Ӷ���������180�ȴ������������ԡ�
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

    public class AttackState : State
    {
        public AttackState(string name, Role role) : base(name, role)
        { }

        public override void End(bool reverse)
        {
            base.End(reverse);
            _role.SetState(Enum.RoleState.Over);
        }
    }

    public class AttackedState : State
    {
        public AttackedState(string name, Role role) : base(name, role)
        { 
        }

        public override void End(bool reverse)
        {
            base.End(reverse);
            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Attacked_End, new object[] { _role.ID});
        }
    }

    public class DeadState : State 
    {
        public DeadState(string name, Role role) : base(name, role)
        { }

        public override void End(bool reverse)
        {
            base.End(reverse);

            EventDispatcher.Instance.PostEvent(Enum.EventType.Fight_Dead_End, new object[] { _role.ID });
        }
    }
}