using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;

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

        public void InitEnemys(EnemyMapPlugin[] enemys)
        {
            for (int i = 0; i < enemys.Length; i++)
            {
                var enemyConfig = ConfigMgr.Instance.GetConfig<LevelEnemyConfig>("LevelEnemyConfig", enemys[i].configId);
                var equipDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
                for (int j = 0; j < enemyConfig.Equips.Length; j++)
                {
                    var equipConfig = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", enemyConfig.Equips[j]);
                    var equipTypeConfig = ConfigMgr.Instance.GetConfig<EquipmentTypeConfig>("EquipmentTypeConfig", (int)equipConfig.Type);
                    equipDic[equipTypeConfig.Place] = new EquipmentData(0, equipConfig.ID);
                }
                var levelRoleData = new LevelRoleData(enemyConfig.ID, enemyConfig.RoleID, enemyConfig.Level, Enum.RoleState.Locked, equipDic, null);
                levelRoleData.HP = enemyConfig.HP;
                levelRoleData.hexagonID = enemys[i].hexagonID;
                CreateEnemy(levelRoleData);
            }
        }

        public Role CreateHero(LevelRoleData data)
        {
            var hero = new Hero(data);

            hero.SetParent(GameObject.Find("RoleRoot").transform);

            _roleList.Add(hero);

            return hero;
        }

        public void CreateEnemy(LevelRoleData data)
        {
            var enemy = new Enemy(data);

            enemy.SetParent(GameObject.Find("RoleRoot").transform);

            _roleList.Add(enemy);
        }

        public void RemoveRole(int id)
        {
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                if (id == _roleList[i].ID)
                {
                    var role = _roleList[i];
                    _roleList.RemoveAt(i);
                    role.Dispose();
                    return;
                }
            }
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
        public string GetHexagonIDByRoleID(int id)
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
