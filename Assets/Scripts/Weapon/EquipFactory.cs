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
            switch ((Enum.EquipmentType)config.Type)
            {
                case Enum.EquipmentType.Wand:
                    return new Wand(data);
                case Enum.EquipmentType.Bow:
                    return new Bow(data);
                case Enum.EquipmentType.Arrow:
                    return new Arrow(data);
                default:
                    return new Equip(data);
            }
        }
    }
}