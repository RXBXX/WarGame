using UnityEngine;

namespace WarGame
{
    public class Enemy : Role
    {
        public Enemy(int id, RoleAttribute attribute, string prefab, string bornHexagon) : base(id, attribute, prefab, bornHexagon)
        {
            _gameObject.tag = Enum.Tag.Enemy.ToString();
        }
    }
}
