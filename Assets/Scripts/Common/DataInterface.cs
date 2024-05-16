using System.Collections.Generic;
using System;

namespace WarGame
{
    [Serializable]
    public class EquipmentData
    {
        public int UID;
        public int configId;

        public EquipmentData(int UID, int configId)
        {
            this.UID = UID;
            this.configId = configId;
        }

        public int GetConfigID()
        {
            return configId;
        }

        public string GetName()
        {
            return GetConfig().Name;
        }

        public string GetIcon()
        {
            return GetConfig().Icon;
        }

        public EquipmentConfig GetConfig()
        {
            return ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", configId);
        }

        public EquipmentTypeConfig GetTypeConfig()
        {
            return ConfigMgr.Instance.GetConfig<EquipmentTypeConfig>("EquipmentTypeConfig", (int)GetType());
        }

        public new Enum.EquipType GetType()
        {
            return (Enum.EquipType)GetConfig().Type;
        }

        public Enum.EquipType[] GetCombination()
        {
            return GetTypeConfig().Combination;
        }

        public Enum.EquipType GetForcedCombination()
        {
            return GetTypeConfig().ForcedCombination;
        }

        public List<Pair> GetAttrs()
        {
            return GetConfig().Attrs;
        }

        public Enum.EquipPlace GetPlace()
        {
            return GetTypeConfig().Place;
        }

        public EquipPlaceConfig GetPlaceConfig()
        {
            return ConfigMgr.Instance.GetConfig<EquipPlaceConfig>("EquipPlaceConfig", (int)GetPlace());
        }

        public string GetSpinePoint()
        {
            return GetPlaceConfig().SpinePoint;
        }

        public EquipmentData Clone()
        {
            return new EquipmentData(this.UID, this.configId);
        }
    }

    [Serializable]
    public class RoleData
    {
        public int UID;
        public int configId;
        public int level;
        public Dictionary<Enum.EquipPlace, int> equipmentDic = new Dictionary<Enum.EquipPlace, int>();
        public Dictionary<int, int> skillDic = new Dictionary<int, int>();
        public Dictionary<int, int> talentDic = new Dictionary<int, int>();

        public RoleData(int UID, int configId, int level, Dictionary<Enum.EquipPlace, int> equipmentDic, Dictionary<int, int> skillDic)
        {
            this.UID = UID;
            this.configId = configId;
            this.level = level;
            this.equipmentDic = equipmentDic;
            this.skillDic = skillDic;
        }

        public RoleConfig GetConfig()
        {
            return ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", configId);
        }

        public RoleStarConfig GetStarConfig()
        {
            return ConfigMgr.Instance.GetConfig<RoleStarConfig>("RoleStarConfig", configId * 1000 + level);
        }

        public SkillConfig GetSkillConfig(Enum.SkillType skillType)
        {
            if (Enum.SkillType.CommonAttack == skillType)
            {
                return ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", GetConfig().CommonSkill);
            }
            else
            {
                return ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", GetConfig().SpecialSkill);
            }
        }

        public RoleJobConfig GetJobConfig()
        {
            return ConfigMgr.Instance.GetConfig<RoleJobConfig>("RoleJobConfig", GetConfig().Job);
        }

        public virtual float GetAttribute(Enum.AttrType attrType)
        {
            var value = 0.0F;

            //英雄属性
            var attrs = GetStarConfig().Attrs;
            foreach (var v in attrs)
            {
                if ((Enum.AttrType)v.id == attrType)
                {
                    value += v.value;
                    break;
                }
            }

            //天赋属性
            foreach (var v in talentDic)
            {
                var config = ConfigMgr.Instance.GetConfig<TalentConfig>("TalentConfig", v.Value);
                foreach (var v1 in config.Attrs)
                {
                    if ((Enum.AttrType)v1.id == attrType)
                    {
                        value += v1.value;
                    }
                }
            }

            //装备属性
            if (null != equipmentDic)
            {
                foreach (var v in equipmentDic)
                {
                    var equipData = DatasMgr.Instance.GetEquipmentData(v.Value);
                    var equipConfig = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", equipData.configId);
                    if (null != equipConfig.Attrs)
                    {
                        foreach (var v1 in equipConfig.Attrs)
                        {
                            if ((Enum.AttrType)v1.id == attrType)
                            {
                                value += v1.value;
                                break;
                            }
                        }
                    }
                }
            }

            return value;
        }

        public RoleData Clone()
        {
            var cloneEquipDic = new Dictionary<Enum.EquipPlace, int>();
            foreach (var v in this.equipmentDic)
            {
                cloneEquipDic.Add(v.Key, v.Value);
            }
            var cloneSkillDic = new Dictionary<int, int>();
            foreach (var v in this.skillDic)
            {
                cloneSkillDic.Add(v.Key, v.Value);
            }
            return new RoleData(this.UID, this.configId, this.level, cloneEquipDic, cloneSkillDic);
        }
    }

    /// <summary>
    /// 关卡中角色数据
    /// </summary>
    [Serializable]
    public class LevelRoleData : RoleData
    {
        public float hp;
        public string hexagonID;
        public List<Pair> buffs = new List<Pair>();
        public Dictionary<Enum.AttrType, float> attrDic = new Dictionary<Enum.AttrType, float>();

        public LevelRoleData(int UID, int configId, int level, Dictionary<Enum.EquipPlace, int> equipmentDic, Dictionary<int, int> skillDic) : base(UID, configId, level, equipmentDic, skillDic)
        {
            this.UID = UID;
            this.configId = configId;
            this.level = level;
            this.equipmentDic = equipmentDic;
            this.skillDic = skillDic;

            InitAttrs();
        }

        /// <summary>
        /// 初始化所有属性
        /// </summary>
        private void InitAttrs()
        {
            ConfigMgr.Instance.ForeachConfig<AttrConfig>("AttrConfig", (config) =>
            {
                var attrType = (Enum.AttrType)config.ID;
                attrDic[attrType] = base.GetAttribute(attrType);
            });
        }

        public override float GetAttribute(Enum.AttrType attrType)
        {
            var value = attrDic[attrType];

            //需要找到所有临时性buff计算
            foreach (var v in buffs)
            {

            }

            return value;
        }

        //添加buff
        public void AddBuffs(List<int> buffs)
        {
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                var buffConfig = ConfigMgr.Instance.GetConfig<BufferConfig>("BufferConfig", buffs[i]);
                this.buffs.Add(new Pair(buffs[i], buffConfig.Duration));
            }
        }

        public void ExcuteBuffs()
        {
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                var buffConfig = ConfigMgr.Instance.GetConfig<BufferConfig>("BufferConfig", buffs[i].id);
                var attrType = (Enum.AttrType)buffConfig.Attr.id;
                UpdateAttr(attrType, attrDic[attrType] + buffConfig.Attr.id);

                if (buffs[i].value - 1 <= 0)
                {
                    buffs.RemoveAt(i);
                }
                else
                {
                    buffs[i] = new Pair(buffs[i].id, buffs[i].value - 1);
                }
            }
        }

        public void UpdateAttr(Enum.AttrType type, float value)
        {
            switch (type)
            {
                case Enum.AttrType.HP:
                    attrDic[Enum.AttrType.HP] = value;
                    break;
                case Enum.AttrType.PhysicalAttack:
                    break;
                case Enum.AttrType.Cure:
                    break;
                case Enum.AttrType.PhysicalDefense:
                    break;
                case Enum.AttrType.MoveDis:
                    break;
                case Enum.AttrType.AttackDis:
                    break;
            }
        }
    }

    /// <summary>
    /// 关卡数据
    /// </summary>
    [Serializable]
    public class LevelData
    {
        public int configId;
        public bool pass;
        public List<LevelRoleData> heros = new List<LevelRoleData>();
        public List<LevelRoleData> enemys = new List<LevelRoleData>();

        public LevelData(int configId)
        {
            this.configId = configId;
        }
    }

    [Serializable]
    public class GameData
    {
        public string title;
        public float time;
        public Dictionary<int, RoleData> roleDataDic = new Dictionary<int, RoleData>();
        public Dictionary<int, EquipmentData> equipDataDic = new Dictionary<int, EquipmentData>();
        public Dictionary<int, LevelData> levelDataDic = new Dictionary<int, LevelData>();

        public GameData(string title, float time)
        {
            this.title = title;
            this.time = time;
            var skillDic = new Dictionary<int, int>();
            skillDic.Add(10001, 1);
            skillDic.Add(10002, 1);
            roleDataDic.Add(1, new RoleData(1, 10001, 1, new Dictionary<Enum.EquipPlace, int>(), skillDic));
            roleDataDic.Add(2, new RoleData(2, 10002, 1, new Dictionary<Enum.EquipPlace, int>(), skillDic));
            roleDataDic.Add(3, new RoleData(3, 10003, 1, new Dictionary<Enum.EquipPlace, int>(), skillDic));
            roleDataDic.Add(4, new RoleData(4, 10004, 1, new Dictionary<Enum.EquipPlace, int>(), skillDic));

            equipDataDic.Add(1, new EquipmentData(1, 10001));
            equipDataDic.Add(2, new EquipmentData(2, 10002));
            equipDataDic.Add(3, new EquipmentData(3, 10003));
            equipDataDic.Add(4, new EquipmentData(4, 10004));
            equipDataDic.Add(5, new EquipmentData(5, 10005));
            equipDataDic.Add(6, new EquipmentData(6, 10006));
            equipDataDic.Add(7, new EquipmentData(7, 10007));
            equipDataDic.Add(8, new EquipmentData(8, 10008));
            equipDataDic.Add(9, new EquipmentData(9, 10009));
            equipDataDic.Add(10, new EquipmentData(10, 10010));
            equipDataDic.Add(11, new EquipmentData(11, 10011));
            equipDataDic.Add(12, new EquipmentData(12, 10012));
            equipDataDic.Add(13, new EquipmentData(13, 10013));
        }

        public GameData Clone()
        {
            var gd = new GameData(this.title, this.time);
            foreach (var v in roleDataDic)
            {
                gd.roleDataDic.Add(v.Key, v.Value.Clone());
            }
            foreach (var v in equipDataDic)
            {
                gd.equipDataDic.Add(v.Key, v.Value.Clone());
            }
            return gd;
        }
    }

    public class SampleGameData
    {
        public string title;
        public float time;

        public SampleGameData(string title, float time)
        {
            this.title = title;
            this.time = time;
        }
    }


    //region 协议部分 -------------------------------------------------------------------------------------------
    public struct UnwearEquipNDPU
    {
        public Enum.ErrorCode ret;
        public int roleUID;
        public List<int> unwearEquips;

        public UnwearEquipNDPU(Enum.ErrorCode ret, int roleUID, List<int> unwearEquips)
        {
            this.ret = ret;
            this.roleUID = roleUID;
            this.unwearEquips = unwearEquips;
        }
    }

    public struct WearEquipNDPU
    {
        public Enum.ErrorCode ret;
        public int roleUID;
        public List<int> wearEquips;
        public List<int> unwearEquips;

        public WearEquipNDPU(Enum.ErrorCode ret, int roleUID, List<int> wearEquips, List<int> unwearEquips)
        {
            this.ret = ret;
            this.roleUID = roleUID;
            this.wearEquips = wearEquips;
            this.unwearEquips = unwearEquips;
        }
    }
    //endregion -------------------------------------------------------------------------------------------------
}
