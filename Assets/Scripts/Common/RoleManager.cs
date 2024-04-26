using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;

namespace WarGame
{
    public class RoleManager : Singeton<RoleManager>
    {
        private List<Role> _roleList = new List<Role>();
        private int _initiator = 0, _target = 0;

        public override bool Init()
        {
            base.Init();
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Event, HandleFightEvents);
            return true;
        }

        public override void Update()
        {
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                _roleList[i].Update();
            }
        }

        public override bool Dispose()
        {
            base.Dispose();

            Clear();

            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Event, HandleFightEvents);
            return true;
        }

        public int CreateHero(int id, RoleAttribute attr, string prefab, string bornHexagon)
        {
            var hero = new Hero(id, attr, prefab, bornHexagon);

            _roleList.Add(hero);

            return id;
        }

        public int CreateEnemy(int id, RoleAttribute attr, string prefab, string bornHexagon)
        {
            var enemy = new Enemy(id, attr, prefab, bornHexagon);

            _roleList.Add(enemy);

            return id;
        }

        public void RemoveRole(int id)
        {
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                if (id == _roleList[i].ID)
                {
                    _roleList.RemoveAt(i);
                    return;
                }
            }
        }


        public void MoveRole(int id, List<string> hexagons)
        {
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                if (id == _roleList[i].ID)
                {
                    _roleList[i].Move(hexagons);
                    return;
                }
            }
        }


        /// <summary>
        /// 获取指定英雄所在的地块id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetHexagonIDByRoleID(int id)
        {
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                if (i >= _roleList.Count)
                    continue;
                if (_roleList[i].ID == id)
                {
                    return _roleList[i].hexagonID;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取指定地块的敌人
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetRoleIDByHexagonID(string id)
        {
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                if (i >= _roleList.Count)
                    continue;
                if (_roleList[i].hexagonID == id)
                {
                    return _roleList[i].ID;
                }
            }
            return 0;
        }

        public void Clear()
        {
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                if (i >= _roleList.Count)
                    continue;
                _roleList[i].Dispose();
            }
            _roleList.Clear();
        }

        public Role GetRole(int id)
        {
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                if (i >= _roleList.Count)
                    continue;
                if (_roleList[i].ID == id)
                    return _roleList[i];
            }
            return null;
        }

        public List<Role> GetAllRolesByType(Enum.RoleType type)
        {
            List<Role> roles = new List<Role>();
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                if (i >= _roleList.Count)
                    continue;
                if (_roleList[i].Type == type)
                    roles.Add(_roleList[i]);
            }
            return roles;
        }

        public void Attack(int initiatorID, int targetID)
        {
            _initiator = initiatorID;
            _target = targetID;

            var initiator = GetRole(initiatorID);
            initiator.Attack();
        }

        public bool IsAllAttackOver()
        {
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                if (i >= _roleList.Count)
                    continue;
                if (_roleList[i].State != Enum.RoleState.AttackOver)
                    return false;
            }

            return true;
        }

        public void NextState(int id)
        {
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                if (i >= _roleList.Count)
                    continue;
                if (_roleList[i].ID == id)
                {
                    _roleList[i].NextState();
                    break;
                }
            }

            bool allHeroAttackOver = true;
            bool allEnemyAttackOver = true;
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                if (i >= _roleList.Count)
                    continue;
                if (_roleList[i].State != Enum.RoleState.AttackOver)
                {
                    if (_roleList[i].Type == Enum.RoleType.Hero && !_roleList[i].IsDead())
                        allHeroAttackOver = false;
                    else if (_roleList[i].Type == Enum.RoleType.Enemy && !_roleList[i].IsDead())
                        allEnemyAttackOver = false;
                }
            }

            if (allEnemyAttackOver && allHeroAttackOver)
            {
                EventDispatcher.Instance.Dispatch(Enum.EventType.Fight_RoundOver_Event);
            }
            else if (allHeroAttackOver)
            {
                var nonAttackingEnemy = true;
                for (int i = _roleList.Count - 1; i >= 0; i--)
                {
                    if (i >= _roleList.Count)
                        continue;
                    if (_roleList[i].State == Enum.RoleState.Attacking && _roleList[i].Type == Enum.RoleType.Enemy)
                    {
                        nonAttackingEnemy = false;
                        break;
                    }
                }
                if (nonAttackingEnemy)
                {
                    for (int i = _roleList.Count - 1; i >= 0; i--)
                    {
                        if (i >= _roleList.Count)
                            continue;
                        if (_roleList[i].State == Enum.RoleState.Waiting && _roleList[i].Type == Enum.RoleType.Enemy && !_roleList[i].IsDead())
                        {
                            _roleList[i].NextState();
                            break;
                        }
                    }
                }
            }
        }

        public void NextAllHeroState()
        {
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                if (i >= _roleList.Count)
                    continue;
                if (_roleList[i].Type == Enum.RoleType.Hero)
                    _roleList[i].NextState();
            }
        }

        public void NextAllState()
        {
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                if (i >= _roleList.Count)
                    continue;
                _roleList[i].NextState();
            }
        }

        private void HandleFightEvents(params object[] args)
        {
            if (null == args)
                return;
            if (args.Length <= 0)
                return;

            var strs = ((string)args[0]).Split('_');
            var role = GetRole((int)args[1]);
            role.HandleEvent(strs[0], strs[1]);
            //DebugManager.Instance.Log("HandleFightEvents:" + eventName);
            //if (eventName == "Attack")
            //{
            //    if (_initiator == (int)args[1] && _target > 0)
            //    {
            //        var initiator = GetRole(_initiator);
            //        var target = GetRole(_target);
            //        var hurt = initiator.Attribute.attack - target.Attribute.defense;
            //        target.Attacked(hurt);
            //        _target = 0;
            //    }
            //}
            //else if (eventName == "Attack_End")
            //{
            //    if (_initiator == (int)args[1])
            //    {
            //        NextState(_initiator);

            //    }
            //}
            //else if (eventName == "Dead")
            //{
            //    var role = GetRole((int)args[1]);
            //    if (null != role)
            //    {
            //        NextState((int)args[1]);
            //        _roleList.Remove(role);
            //        role.Dispose();
            //    }
            //}
            //else if (eventName == "Jump_Take")
            //{
            //    var role1 = RoleManager.Instance.GetRole((int)args[1]);
            //    role1.TakeJump();
            //}
            //else if (eventName == "Jump_Loss")
            //{
            //    var role1 = RoleManager.Instance.GetRole((int)args[1]);
            //    role1.LossJump();
            //}
            //else if (eventName == "Jump_End")
            //{
            //    var role2 = RoleManager.Instance.GetRole((int)args[1]);
            //    role2.EndJump();
            //}
        }
    }
}
