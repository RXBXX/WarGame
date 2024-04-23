using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;

namespace WarGame
{
    public class RoleManager : Singeton<RoleManager>
    {
        private List<Hero> _heroList = new List<Hero>();
        private List<Enemy> _enemyList = new List<Enemy>();
        private int _initiator = 0, _target = 0;

        public override bool Init()
        {
            base.Init();
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_Event, HandleFightEvents);
            return true;
        }

        public override void Update()
        {
            for (int i = _heroList.Count - 1; i >= 0; i--)
            {
                if (null != _heroList[i])
                    _heroList[i].Update();
            }
            for (int i = _enemyList.Count - 1; i >= 0; i--)
            {
                if (null != _enemyList[i])
                    _enemyList[i].Update();
            }
        }

        public override bool Dispose()
        {
            base.Dispose();

            for (int i = _heroList.Count - 1; i >= 0; i--)
            {
                if (null != _heroList[i])
                    _heroList[i].Dispose();
            }
            _heroList.Clear();

            for (int i = _enemyList.Count - 1; i >= 0; i--)
            {
                if (null != _enemyList[i])
                    _enemyList[i].Dispose();
            }
            _enemyList.Clear();

            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_Event, HandleFightEvents);
            return true;
        }

        public int CreateHero(int id, RoleAttribute attr, string prefab, string bornHexagon)
        {
            var hero = new Hero(id, attr, prefab, bornHexagon);

            _heroList.Add(hero);

            return id;
        }

        public void RemoveHero(int id)
        {
            for (int i = _heroList.Count - 1; i >= 0; i--)
            {
                if (null != _heroList[i] && id == _heroList[i].ID)
                {
                    _heroList.RemoveAt(i);
                    return;
                }
            }
        }

        public int CreateEnemy(int id, RoleAttribute attr, string prefab, string bornHexagon)
        {
            var enemy = new Enemy(id, attr, prefab, bornHexagon);

            _enemyList.Add(enemy);

            return id;
        }

        public void RemoveEnemy(int id)
        {
            for (int i = _enemyList.Count - 1; i >= 0; i--)
            {
                if (null != _enemyList[i] && id == _enemyList[i].ID)
                {
                    _enemyList.RemoveAt(i);
                    return;
                }
            }
        }


        public void MoveHero(int id, List<string> hexagons)
        {
            Hero hero = null;
            for (int i = _heroList.Count - 1; i >= 0; i--)
            {
                if (null != _heroList[i] && id == _heroList[i].ID)
                {
                    hero = _heroList[i];
                    break;
                }
            }
            hero.Move(hexagons);
        }


        /// <summary>
        /// 获取指定英雄所在的地块id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetHexagonIDByHeroID(int id)
        {
            for (int i = _heroList.Count - 1; i >= 0; i--)
            {
                if (i >= _heroList.Count)
                    continue;
                if (_heroList[i].ID == id)
                {
                    return _heroList[i].hexagonID;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取指定敌人所在的地块id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetHexagonIDByEnemyID(int id)
        {
            for (int i = _enemyList.Count - 1; i >= 0; i--)
            {
                if (i >= _enemyList.Count)
                    continue;
                if (_enemyList[i].ID == id)
                {
                    return _enemyList[i].hexagonID;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取指定地块的敌人
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetEnemyIDByHexagonID(string id)
        {
            for (int i = _enemyList.Count - 1; i >= 0; i--)
            {
                if (i >= _enemyList.Count)
                    continue;
                if (_enemyList[i].hexagonID == id)
                {
                    return _enemyList[i].ID;
                }
            }
            return 0;
        }

        /// <summary>
        /// 获取指定地块的英雄
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetHeroIDByHexagonID(string id)
        {
            for (int i = _heroList.Count - 1; i >= 0; i--)
            {
                if (i >= _heroList.Count)
                    continue;
                if (_heroList[i].hexagonID == id)
                {
                    return _heroList[i].ID;
                }
            }
            return 0;
        }

        public void Clear()
        {
            for (int i = _heroList.Count - 1; i >= 0; i--)
            {
                if (i >= _heroList.Count)
                    continue;
                _heroList[i].Dispose();
            }
            _heroList.Clear();

            for (int i = _enemyList.Count - 1; i >= 0; i--)
            {
                if (i >= _enemyList.Count)
                    continue;
                _enemyList[i].Dispose();
            }
            _enemyList.Clear();
        }

        public Hero GetHero(int id)
        {
            for (int i = _heroList.Count - 1; i >= 0; i--)
            {
                if (_heroList[i].ID == id)
                    return _heroList[i];
            }
            return null;
        }

        public Enemy GetEnemy(int id)
        {
            for (int i = _enemyList.Count - 1; i >= 0; i--)
            {
                if (_enemyList[i].ID == id)
                    return _enemyList[i];
            }
            return null;
        }

        public void Attack(int initiatorID, int targetID)
        {
            _initiator = initiatorID;
            _target = targetID;

            var initiator = GetHero(initiatorID);
            initiator.Attack();
        }

        private void HandleFightEvents(params object[] args)
        {
            if (null == args)
                return;
            if (args.Length <= 0)
                return;

            var arg = (string)args[0];
            switch (arg)
            {
                case "Attack":
                    if(_initiator == (int)args[1] && _target > 0)
                    {
                        var initiator = GetHero(_initiator);
                        var target = GetEnemy(_target);

                        var hurt = initiator.Attribute.attack - target.Attribute.defense;
                        target.Attacked(hurt);

                        _initiator = 0;
                        _target = 0;
                    }
                    break;
                case "Dead":
                    var hero = GetHero((int)args[1]);
                    if (null != hero)
                    {
                        _heroList.Remove(hero);
                        hero.Dispose();
                    }    
                    var enemy = GetEnemy((int)args[1]);
                    if (null != enemy)
                    {
                        _enemyList.Remove(enemy);
                        enemy.Dispose();
                    }
                    break;
            }
        }
    }
}
