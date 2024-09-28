using FairyGUI;
using UnityEngine;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class FightArenaPanel : UIBase
    {
        private class AttrProgressGroup
        {
            public GProgressBar hp;
            public GProgressBar rage;

            public AttrProgressGroup(GProgressBar hp, GProgressBar rage)
            {
                this.hp = hp;
                this.rage = rage;
            }

            public void Dispose()
            {

            }
        }

        private object[] _args;
        private Dictionary<int, AttrProgressGroup> _roleDic = new Dictionary<int, AttrProgressGroup>();

        public FightArenaPanel(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        {
            this._args = args;

            EventDispatcher.Instance.AddListener(Enum.Event.Role_Attr_Change, OnAttrChange);
        }

        public override void OnEnable()
        {
            var initiators = (List<int>)_args[0];
            for (int i = 0; i < initiators.Count; i++)
            {
                AddHPProgress(initiators[i], new Vector2(1200 - i * 30, GCom.height - 60));
            }

            if (_args.Length > 1)
            {
                var targets = (List<int>)_args[1];
                for (int i = 0; i < targets.Count; i++)
                {
                    AddHPProgress(targets[i], new Vector2(134 + i * 30, GCom.height - 60));
                }
            }

            GetGObjectChild<GButton>("tag").title = (string)_args[2];
        }

        private void AddHPProgress(int roleID, Vector2 pos)
        {
            var role = RoleManager.Instance.GetRole(roleID);

            var hp = UIManager.Instance.CreateObject<GProgressBar>("Common", "HUDProgress");
            hp.rotation = -90;
            hp.SetPivot(0, 0.5F, true);
            hp.xy = pos;
            hp.size = new Vector2(520, 16);
            hp.max = role.GetAttribute(Enum.AttrType.HP);
            hp.value = role.GetHP();
            hp.scaleX = 0;
            hp.GetController("style").SetSelectedIndex((int)role.Type - 1);
            hp.TweenScaleX(1, 0.5F);
            GCom.AddChild(hp);

            var rage = UIManager.Instance.CreateObject<GProgressBar>("Common", "HUDProgress");
            rage.rotation = -90;
            rage.SetPivot(0, 0.5F, true);
            pos.x += 11;
            rage.xy = pos;
            rage.size = new Vector2(520, 6);
            rage.max = role.GetAttribute(Enum.AttrType.Rage);
            rage.value = role.GetRage();
            rage.scaleX = 0;
            rage.GetController("style").SetSelectedIndex(2);
            rage.TweenScaleX(1, 0.5F);
            GCom.AddChild(rage);

            _roleDic.Add(roleID, new AttrProgressGroup(hp, rage));
        }

        private void OnAttrChange(params object[] args)
        {
            var senderID = (int)args[0];
            if (!_roleDic.ContainsKey(senderID))
                return;

            var target = RoleManager.Instance.GetRole(senderID);
            if ((Enum.AttrType)args[1] == Enum.AttrType.HP)
            {
                var progress = _roleDic[senderID].hp;
                var duration = (float)(Mathf.Abs(target.GetHP() - (float)progress.value) / progress.max) * 0.2f;
                progress.TweenValue(target.GetHP(), duration);
            }
            else
            {
                var progress = _roleDic[senderID].rage;
                var duration = (float)(Mathf.Abs(target.GetRage() - (float)progress.value) / progress.max) * 0.2f;
                progress.TweenValue(target.GetRage(), duration);
            }
        }

        public override void Dispose(bool disposeGCom = false)
        {
            foreach (var v in _roleDic)
            {
                v.Value.Dispose();
            }
            _roleDic.Clear();

            EventDispatcher.Instance.RemoveListener(Enum.Event.Role_Attr_Change, OnAttrChange);
            base.Dispose(disposeGCom);
        }
    }
}
