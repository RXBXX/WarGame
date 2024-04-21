using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class RoleManager : Singeton<RoleManager>
    {
        private List<Hero> _heroList = new List<Hero>();
        private List<Enemy> _enemyList = new List<Enemy>();

        public override bool Init()
        {
            base.Init();
            return true;
        }

        public void Update()
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
                }
            }
            hero.Move(hexagons);
        }

        /// <summary>
        /// 判断当前地块是否有英雄
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HaveHeroByHexagonID(string id)
        {
            for (int i = _heroList.Count - 1; i >= 0; i--)
            {
                if (i >= _heroList.Count)
                    continue;
                if (_heroList[i].hexagonID == id)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 判断当前地块是否有敌人
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HaveEnemyByHexagonID(string id)
        {
            for (int i = _enemyList.Count - 1; i >= 0; i--)
            {
                if (i >= _enemyList.Count)
                    continue;
                if (_enemyList[i].hexagonID == id)
                {
                    return true;
                }
            }
            return false;
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
    }
}
