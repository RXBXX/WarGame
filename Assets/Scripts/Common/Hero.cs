using UnityEngine;

namespace WarGame
{
    public class Hero:Role
    {
        public Hero(int id, RoleAttribute attribute, string prefab, string bornHexagon) : base(id, attribute, prefab, bornHexagon)
        {
            _gameObject.tag = Enum.Tag.Hero.ToString();
        }
    }
}
