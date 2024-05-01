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

        public void CreateHero(RoleData data, string bornHexagon)
        {
            var hero = new Hero(data, bornHexagon);

            _roleList.Add(hero);
        }

        public void CreateEnemy(RoleData data, string bornHexagon)
        {
            var enemy = new Enemy(data, bornHexagon);

            _roleList.Add(enemy);
        }

        public void RemoveRole(int id)
        {
            for (int i = _roleList.Count - 1; i >= 0; i--)
            {
                if (id == _roleList[i].ID)
                {
                    _roleList[i].Dispose();
                    _roleList.RemoveAt(i);
                    return;
                }
            }
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

    }
}
