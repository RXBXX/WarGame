using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace WarGame
{
    public class RoleManager : Singeton<RoleManager>
    {
        private List<Role> _roleList = new List<Role>();

        public override void Update(float deltaTime)
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

            return true;
        }

        public List<LevelRoleData> InitLevelRoles(NewEnemyMapPlugin[] roles)
        {
            var enemys = new List<LevelRoleData>();
            for (int i = 0; i < roles.Length; i++)
            {
                var enemyConfig = ConfigMgr.Instance.GetConfig<EnemyConfig>("EnemyConfig", roles[i].ID);
                var levelRoleData = DatasMgr.Instance.CreateLevelRoleData(Enum.RoleType.Enemy, enemyConfig.ID, roles[i].hexagonID);
                CreateRole(Enum.RoleType.Enemy, levelRoleData);
                enemys.Add(levelRoleData);

            }
            return enemys;
        }

        public Role CreateRole(Enum.RoleType type, LevelRoleData data)
        {
            var role = Factory.Instance.GetRole(type, data);
            role.SetParent(GameObject.Find("RoleRoot").transform);
            _roleList.Add(role);
            return role;
        }

        public Role CreateHero(LevelRoleData data)
        {
            var hero = new Hero(data);

            hero.SetParent(GameObject.Find("RoleRoot").transform);

            _roleList.Add(hero);

            return hero;
        }

        public Role CreateEnemy(LevelRoleData data)
        {
            var enemy = new Enemy(data);

            enemy.SetParent(GameObject.Find("RoleRoot").transform);

            _roleList.Add(enemy);

            return enemy;
        }

        public void RemoveRole(int id)
        {
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                if (id == _roleList[i].ID)
                {
                    var role = _roleList[i];
                    if (role.Dispose())
                    {
                        _roleList.RemoveAt(i);
                    }
                    return;
                }
            }
        }

        public List<int> ClearDeadRole()
        {
            var deadRoles = new List<int>();
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                var role = _roleList[i];
                if (role.DeadFlag)
                {
                    if (role.HaveNextStage())
                    {
                        role.NextStage();
                    }
                    else
                    {
                        role.ShowDrops();
                        _roleList.RemoveAt(i);
                        role.Dispose();
                    }
                    deadRoles.Add(role.ID);
                }
            }
            return deadRoles;
        }

        public float GetLoadingProgress()
        {
            float progress = 0;
            foreach (var v in _roleList)
                progress += v.GetLoadingProgress();
            return progress / _roleList.Count;
        }

        //public void MoveRole(int id, List<string> hexagons)
        //{
        //    for (int i = _roleList.Count - 1; i >= 0; i--)
        //    {
        //        if (id == _roleList[i].ID)
        //        {
        //            _roleList[i].Move(hexagons);
        //            return;
        //        }
        //    }
        //}


        /// <summary>
        /// 获取指定英雄所在的地块id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetHexagonIDByRoleID(int id)
        {
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                if (i >= _roleList.Count)
                    continue;
                if (_roleList[i].ID == id)
                {
                    return _roleList[i].Hexagon;
                }
            }
            return -1;
        }

        /// <summary>
        /// 获取指定地块的敌人
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetRoleIDByHexagonID(int id)
        {
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                if (i >= _roleList.Count)
                    continue;
                if (_roleList[i].Hexagon == id)
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

        public List<Role> GetAllRoles()
        {
            List<Role> roles = new List<Role>();
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                if (i >= _roleList.Count)
                    continue;
                roles.Add(_roleList[i]);
            }
            return roles;
        }

    }
}
