using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class SingleHealSkill : SingleSkill
    {
        public SingleHealSkill(int id, int initiatorID) : base(id, initiatorID)
        {
        }

        protected override void AddListeners()
        {
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Cured_End, OnCuredEnd);
        }

        protected override void RemoveListeners()
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Cured_End, OnCuredEnd);
        }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            //var initiator = RoleManager.Instance.GetRole(sender);
            if ("Cure" == stateName && "Take" == secondStateName)
            {
                BattleMgr.Instance.DoCure(_initiatorID, _targets[0]);
            }
        }

        protected override void TriggerSkill()
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);
            initiator.Cure();
        }

        private void OnCuredEnd(object[] args)
        {
            var targetID = (int)args[0];
            if (targetID != _targets[0])
                return;

            if (null != _attackCoroutine)
                return;

            _attackCoroutine = CoroutineMgr.Instance.StartCoroutine(Over(1.5F));
        }

        protected override void Preview(int touchingID)
        {
            _previewTargets.Add(touchingID);
            var role = RoleManager.Instance.GetRole(touchingID);
            role.SetLayer(Enum.Layer.Gray);
            role.SetHUDRoleVisible(true);
            role.Preview(BattleMgr.Instance.GetCurePower(_initiatorID, touchingID));
        }

        protected override void CancelPreview()
        {
            //DebugManager.Instance.Log("CancelPreview");
            foreach (var v in _previewTargets)
            {
                var role = RoleManager.Instance.GetRole(v);
                if (!_attackableTargets.Contains(v))
                {
                    role.RecoverLayer();
                    role.SetHUDRoleVisible(false);
                }
                role.CancelPreview();
            }
            _previewTargets.Clear();
        }
    }
}
