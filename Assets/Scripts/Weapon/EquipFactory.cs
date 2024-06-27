using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class EquipFactory : Singeton<EquipFactory>
    {
        public Equip GetEquip(EquipmentData data, Transform spineRoot)
        {
            var config = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", data.configId);
            switch ((Enum.EquipType)config.Type)
            {
                case Enum.EquipType.Wand:
                    return new Wand(data, spineRoot);
                case Enum.EquipType.BowArrow:
                    return new Bow(data, spineRoot);
                case Enum.EquipType.Sword:
                    return new Sword(data, spineRoot);
                default:
                    return new Equip(data, spineRoot);
            }
        }
    }
}
