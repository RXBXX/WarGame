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

        public List<IntFloatPair> GetAttrs()
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

        public RoleData(int UID, int configId, int level, Dictionary<int, int> talentDic = null, Dictionary<Enum.EquipPlace, int> equipmentDic = null, Dictionary<int, int> skillDic = null)
        {
            this.UID = UID;
            this.configId = configId;
            this.level = level;
            if (null != talentDic)
                this.talentDic = talentDic;
            if (null != equipmentDic)
                this.equipmentDic = equipmentDic;
            if (null != skillDic)
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

        public RoleJobConfig GetJobConfig()
        {
            return ConfigMgr.Instance.GetConfig<RoleJobConfig>("RoleJobConfig", GetConfig().Job);
        }

        public ElementConfig GetElementConfig()
        {
            return ConfigMgr.Instance.GetConfig<ElementConfig>("ElementConfig", (int)GetConfig().Element);
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

            if (ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", (int)attrType).ValueType == Enum.ValueType.Percentage)
                return value / 100.0f;
            else
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
            return new RoleData(this.UID, this.configId, this.level, talentDic, cloneEquipDic, cloneSkillDic);
        }

        public void Dispose()
        { 
        
        }
    }

    /// <summary>
    /// 关卡中角色数据
    /// </summary>
    [Serializable]
    public class LevelRoleData : RoleData
    {
        public float HP;
        public float Rage;
        public string hexagonID;
        public List<IntFloatPair> buffs = new List<IntFloatPair>();
        //public Dictionary<Enum.AttrType, float> attrDic = new Dictionary<Enum.AttrType, float>();
        public Dictionary<Enum.EquipPlace, EquipmentData> equipDataDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
        public Enum.RoleState state;

        public LevelRoleData(int UID, int configId, int level, Enum.RoleState state, Dictionary<Enum.EquipPlace, EquipmentData> equipDataDic, Dictionary<int, int> talentDic) : base(UID, configId, level, talentDic, null)
        {
            this.equipDataDic = equipDataDic;
            this.state = state;
            this.HP = GetAttribute(Enum.AttrType.HP);
            this.Rage = 0;
        }

        public override float GetAttribute(Enum.AttrType attrType)
        {
            var value = base.GetAttribute(attrType);

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
                this.buffs.Add(new IntFloatPair(buffs[i], buffConfig.Duration));
            }
        }

        public void ExcuteBuffs()
        {
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                var buffConfig = ConfigMgr.Instance.GetConfig<BufferConfig>("BufferConfig", buffs[i].id);
                var attrType = (Enum.AttrType)buffConfig.Attr.id;
                UpdateAttr(attrType, buffConfig.Attr.value);

                if (buffs[i].value - 1 <= 0)
                {
                    buffs.RemoveAt(i);
                }
                else
                {
                    buffs[i] = new IntFloatPair(buffs[i].id, buffs[i].value - 1);
                }
            }
        }

        public void UpdateAttr(Enum.AttrType type, float deltaValue)
        {
            if (0 == deltaValue)
                return;

            switch (type)
            {
                case Enum.AttrType.HP:
                    HP += deltaValue;
                    HP = MathF.Min(MathF.Max(0, HP), GetAttribute(type));
                    break;
                case Enum.AttrType.Rage:
                    Rage += deltaValue;
                    Rage = MathF.Min(MathF.Max(0, Rage), GetAttribute(type));
                    break;
            }

            EventDispatcher.Instance.PostEvent(Enum.EventType.Role_Attr_Change, new object[] { UID, type, deltaValue });
        }

        public new LevelRoleData Clone()
        {
            var cloneEquipDataDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
            foreach (var v in equipDataDic)
            {
                cloneEquipDataDic.Add(v.Key, v.Value.Clone());
            }

            var clone = new LevelRoleData(this.UID, this.configId, this.level, state, cloneEquipDataDic, new Dictionary<int, int>());
            clone.hexagonID = hexagonID;
            clone.HP = HP;
            clone.Rage = Rage;
            clone.state = state;

            var cloneBuffs = new List<IntFloatPair>();
            foreach (var v in buffs)
                cloneBuffs.Add(v);
            clone.buffs = cloneBuffs;

            return clone;
        }

        public void Dispose()
        { 
        
        }
    }

    /// <summary>
    /// 关卡数据
    /// </summary>
    [Serializable]
    public class LevelData
    {
        public int configId;
        public bool isEnter;
        public bool isRead;
        public bool isReady;
        public bool pass;
        public List<LevelRoleData> heros = new List<LevelRoleData>();
        public List<LevelRoleData> enemys = new List<LevelRoleData>();

        public LevelData(int configId)
        {
            this.configId = configId;
        }

        public void Clear()
        {
            isEnter = false;
            isRead = false;
            isReady = false;
            pass = false;
            heros.Clear();
            enemys.Clear();
    }

        public LevelData Clone()
        {
            var clone = new LevelData(configId);
            clone.isEnter = isEnter;
            clone.isRead = isRead;
            clone.pass = pass;
            clone.isReady = isReady;
            var cloneHeros = new List<LevelRoleData>();
            foreach (var v in heros)
                cloneHeros.Add(v.Clone());
            clone.heros = cloneHeros;

            var cloneEnemys = new List<LevelRoleData>();
            foreach (var v in enemys)
                cloneEnemys.Add(v.Clone());
            clone.enemys = cloneEnemys;

            return clone;
        }
    }

    [Serializable]
    public class GameData
    {
        public bool isNew = true;
        public string title;
        public float time;
        public Dictionary<int, RoleData> roleDataDic = new Dictionary<int, RoleData>();
        public Dictionary<int, EquipmentData> equipDataDic = new Dictionary<int, EquipmentData>();
        public Dictionary<int, LevelData> levelDataDic = new Dictionary<int, LevelData>();
        private int _heroStartUID = 10000;
        private int _equipStartUID = 30000;

        public GameData(string title, float time)
        {
            this.title = title;
            this.time = time;

            AddHero(10001, 1);
            AddHero(10002, 1);
            AddHero(10003, 1);
            AddHero(10004, 1);

            AddEquip(10001);
            AddEquip(10002);
            AddEquip(10003);
            AddEquip(10004);
            AddEquip(10005);
            AddEquip(10006);
            AddEquip(10007);
            AddEquip(10008);
            AddEquip(10009);
            AddEquip(10010);
            AddEquip(10011);
            AddEquip(10012);
            AddEquip(10013);
        }

        public void AddHero(int configID, int level)
        {
            var roleData = new RoleData(_heroStartUID + roleDataDic.Count + 1, configID, level);
            roleDataDic.Add(roleData.UID, roleData);
        }

        public void AddEquip(int configId)
        {
            var equipData = new EquipmentData(_equipStartUID + equipDataDic.Count + 1, configId);
            equipDataDic.Add(equipData.UID, equipData);
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

        public SampleGameData(string title, float time = 0)
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
