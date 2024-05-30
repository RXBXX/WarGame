using FairyGUI;
using UnityEngine;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class FightArenaPanel : UIBase
    {
        private Dictionary<int, GProgressBar> _roleDic = new Dictionary<int, GProgressBar>();

        public FightArenaPanel(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        {
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_HP_Change, OnHPChange);

            var initiators = (List<int>)args[0];
            for (int i = 0; i < initiators.Count; i++)
            {
                AddHPProgress(initiators[i], new Vector2(1200 - i * 30, 660), i);
            }

            var targets = (List<int>)args[1];
            for (int i = 0; i < targets.Count; i++)
            {
                AddHPProgress(targets[i], new Vector2(134 + i * 30, 660), i);
            }
        }

        private void AddHPProgress(int roleID, Vector2 pos, int index)
        {
            var progress = UIManager.Instance.CreateObject<GProgressBar>("Common", "HUDProgress");
            progress.rotation = -90;
            progress.SetPivot(0, 0.5F, true);
            progress.xy = pos;
            if (0 == index)
            {
                progress.width = 500;
            }
            else
            {
                progress.width = 400;
            }
            progress.max = 100;
            var role = RoleManager.Instance.GetRole(roleID);
            progress.value = role.GetHP();
            progress.scaleX = 0;
            progress.GetController("style").SetSelectedIndex((int)role.Type - 1);
            progress.TweenScaleX(1, 0.5F);
            GCom.AddChild(progress);
            _roleDic.Add(roleID, progress);
        }

        private void OnHPChange(params object[] args)
        {
            var senderID = (int)args[0];
            if (!_roleDic.ContainsKey(senderID))
                return;

            var target = RoleManager.Instance.GetRole(senderID);
            _roleDic[senderID].TweenValue(target.GetHP(), 0.5F);
        }

        public override void Dispose(bool disposeGCom = false)
        {
            foreach (var v in _roleDic)
            {
                v.Value.Dispose();
            }
            _roleDic.Clear();

            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_HP_Change, OnHPChange);
            base.Dispose(disposeGCom);
        }
    }
}
