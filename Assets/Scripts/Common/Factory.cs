using UnityEngine;
using System.Collections.Generic;

namespace WarGame
{
    public class Factory : Singeton<Factory>
    {
        public Role GetRole(Enum.RoleType type, LevelRoleData data)
        {
            switch (type)
            {
                case Enum.RoleType.Hero:
                    return new Hero(data);
                case Enum.RoleType.Enemy:
                    return new Enemy(data);
            }
            return null;
        }

        public Equip GetEquip(EquipmentData data, Transform spineRoot)
        {
            var config = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", data.id);
            switch (config.Type)
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

        public Skill GetSkill(int skillID, int initiatorID)
        {
            switch ((Enum.Skill)skillID)
            {
                case Enum.Skill.FierceAttack:
                    return new FierceAttackSkill(skillID, initiatorID);
                case Enum.Skill.SingleHeal:
                    return new SingleHealSkill(skillID, initiatorID);
                case Enum.Skill.AtrikeAndRelocate:
                    return new AttackAndRelocateSkill(skillID, initiatorID);
                case Enum.Skill.ChainAttack:
                    return new ChainAttackSkill(skillID, initiatorID);
                case Enum.Skill.Inspire:
                    return new InspireSkill(skillID, initiatorID);
                case Enum.Skill.Stealth:
                    return new StealthSkill(skillID, initiatorID);
                case Enum.Skill.CriticalHit:
                    return new CriticalHitSkill(skillID, initiatorID);
                case Enum.Skill.Clone:
                    return new CloneSkill(skillID, initiatorID);
                case Enum.Skill.Dizzy:
                    return new DizzySkill(skillID, initiatorID);
                case Enum.Skill.ExtraTurn:
                    return new ExtraTurnSkill(skillID, initiatorID);
                case Enum.Skill.MassPhyShield:
                    return new MassPhyShieldSkill(skillID, initiatorID);
                case Enum.Skill.Charm:
                    return new CharmSkill(skillID, initiatorID);
                case Enum.Skill.Roulette:
                    return new RouletteSkill(skillID, initiatorID);
                case Enum.Skill.LifeDrain:
                    return new LifeDrainSkill(skillID, initiatorID);
                case Enum.Skill.MassHeal:
                    return new MassHealSkill(skillID, initiatorID);
                case Enum.Skill.SinglePhyShield:
                    return new SinglePhyShieldSkill(skillID, initiatorID);
                case Enum.Skill.RageReduction:
                    return new RageReductionSkill(skillID, initiatorID);
                case Enum.Skill.SingleMagShield:
                    return new SingleMagShieldSkill(skillID, initiatorID);
                case Enum.Skill.MassMagShield:
                    return new MassMagShieldSkill(skillID, initiatorID);
            }
            return null;
        }

        public Hexagon GetHexagon(HexagonMapPlugin plugin)
        {
            switch ((Enum.HexagonType)plugin.configId)
            {
                case Enum.HexagonType.Hex19:
                    return new WaterHexagon(plugin.ID, plugin.configId, plugin.isReachable, plugin.coor);
                case Enum.HexagonType.Hex28:
                    return new MagmaHexagon(plugin.ID, plugin.configId, plugin.isReachable, plugin.coor);
                default:
                    return new Hexagon(plugin.ID, plugin.configId, plugin.isReachable, plugin.coor);
            }
        }

        public PostProcessing GetPostProcessiong(Enum.PostProcessingType type, params object[] args)
        {
            switch (type)
            {
                case Enum.PostProcessingType.Gray:
                    return new GrayPP();
                case Enum.PostProcessingType.Fog:
                    return new FogPP();
                case Enum.PostProcessingType.Palette:
                    return new PalettePP(args);
                default:
                    return null;
            }
        }

        public LevelRoleData GetLevelRoleData(Enum.RoleType type, int UID, int bornHexagonID)
        {
            if (type == Enum.RoleType.Hero)
            {
                var roleData = DatasMgr.Instance.GetRoleData(UID);
                var equipDataDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
                foreach (var v in roleData.equipmentDic)
                {
                    equipDataDic.Add(v.Key, DatasMgr.Instance.GetEquipmentData(v.Value));
                }
                return new LevelRoleData(roleData.UID, roleData.configId, roleData.level, bornHexagonID, Enum.RoleState.Waiting, equipDataDic, roleData.talents);
            }
            else if (type == Enum.RoleType.Enemy)
            {
                var enemyConfig = ConfigMgr.Instance.GetConfig<EnemyConfig>("EnemyConfig", UID);
                var equipDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
                if (null != enemyConfig.Equips)
                {
                    for (int j = 0; j < enemyConfig.Equips.Length; j++)
                    {
                        var equipConfig = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", enemyConfig.Equips[j]);
                        var equipTypeConfig = ConfigMgr.Instance.GetConfig<EquipmentTypeConfig>("EquipmentTypeConfig", (int)equipConfig.Type);
                        equipDic[equipTypeConfig.Place] = new EquipmentData(0, equipConfig.ID);
                    }
                }
                return new LevelRoleData(enemyConfig.ID, enemyConfig.RoleID, enemyConfig.Level, bornHexagonID, Enum.RoleState.Locked, equipDic, null);
            }
            return null;
        }
    }
}
