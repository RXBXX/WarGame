using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class FightEnemyQueue : UIBase
    {
        private List<int> _enemys = new List<int>();
        private List<GComponent> _queue = new List<GComponent>();

        public FightEnemyQueue(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_AIAction_Start, OnEnemyStart);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_AIAction_Over, OnEnemyOver);
            EventDispatcher.Instance.AddListener(Enum.Event.Fight_Role_Dispose, OnEnemyDispose);

            Init();
        }

        public override void Dispose(bool disposeGCom = false)
        {
            foreach (var v in _queue)
            {
                GTween.Kill(v);
                v.Dispose();
            }
            _queue.Clear();

            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_AIAction_Start, OnEnemyStart);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_AIAction_Over, OnEnemyOver);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Fight_Role_Dispose, OnEnemyDispose);
            base.Dispose(disposeGCom);
        }

        private void Init()
        {
            _enemys.Clear();

            foreach (var v in _queue)
                v.visible = false;

            var startPosX = GCom.width - 90;
            var enemys = RoleManager.Instance.GetAllRolesByType(Enum.RoleType.Enemy);
            for (int i = 0; i < enemys.Count; i++)
            {
                _enemys.Add(enemys[i].ID);

                GButton ui = null;
                if (_queue.Count < i + 1)
                {
                    ui = UIManager.Instance.CreateObject<GButton>("Common", "CommonVerHero");
                    GCom.AddChild(ui);
                    ui.xy = new Vector2(startPosX - 76 * i, (GCom.height - ui.height) / 2);
                    _queue.Add(ui);
                }
                else
                {
                    _queue[i].visible = true;
                }
                var role = RoleManager.Instance.GetRole(enemys[i].ID);
                //ui.title = role.GetConfig().Name;
                ui.icon = role.GetConfig().Icon;
            }
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        private void OnEnemyStart(params object[] args)
        {
            var id = (int)args[0];
            var index = 0;
            for (int i = 0; i < _enemys.Count; i++)
            {
                if (_enemys[i] == id)
                {
                    index = i;
                    break;
                }
            }

            var startPosX = GCom.width;
            var ui = _queue[index];
            var tweener = ui.TweenMoveY(-90, 0.2F);
            tweener.OnComplete(() =>
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    _queue[i].TweenMoveX(_queue[i].x - 76, 0.2F);
                    _queue[i + 1] = _queue[i];
                }
                _queue[0] = ui;
                ui.TweenMoveX(startPosX - 90, 0.2F);
            });


            for (int i = index - 1; i >= 0; i--)
            {
                _enemys[i + 1] = _enemys[i];
            }
            _enemys[0] = id;
        }

        private void OnEnemyOver(params object[] args)
        {
            _queue[0].TweenMoveY((GCom.height - _queue[0].height) / 2, 0.2F);
        }

        private void OnEnemyDispose(params object[] args)
        {
            var index = -1;
            var enemyUID = (int)args[0];
            for (int i = 0; i < _enemys.Count; i++)
            {
                if (enemyUID == _enemys[i])
                {
                    index = i;
                    break;
                }
            }
            if (index < 0)
                return;

            _enemys.RemoveAt(index);
            _queue[index].Dispose();
            _queue.RemoveAt(index);

            for (int i = index; i < _enemys.Count; i++)
            {
                _queue[i].TweenMoveX(_queue[i].x + 76, 0.2F);
            }
        }
    }
}
