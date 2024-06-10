using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class EquipFactory : Singeton<EquipFactory>
    {
        public Equip GetEquip(EquipmentData data)
        {
            var config = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", data.configId);
            switch ((Enum.EquipType)config.Type)
            {
                case Enum.EquipType.Wand:
                    return new Wand(data);
                case Enum.EquipType.Bow:
                    return new Bow(data);
                case Enum.EquipType.Arrow:
                    return new Arrow(data);
                case Enum.EquipType.Sword:
                    return new Sword(data);
                default:
                    return new Equip(data);
            }
        }
    }
}
