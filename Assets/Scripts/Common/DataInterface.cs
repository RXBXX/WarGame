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

        public List<IntFloatPair> GetAttrs()
        {
            return GetConfig().Attrs;
        }

        public virtual float GetAttribute(Enum.AttrType attrType)
        {
            var value = 0.0F;

            //英雄属性
            var attrs = GetConfig().Attrs;
            foreach (var v in attrs)
            {
                if ((Enum.AttrType)v.id == attrType)
                {
                    value += v.value;
                    break;
                }
            }

            if (ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", (int)attrType).ValueType == Enum.ValueType.Percentage)
                return value / 100.0f;
            else
                return value;
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
        public List<int> talents = new List<int>();

        public RoleData(int UID, int configId, int level, List<int> talents = null, Dictionary<Enum.EquipPlace, int> equipmentDic = null, Dictionary<int, int> skillDic = null)
        {
            this.UID = UID;
            this.configId = configId;
            this.level = level;
            if (null != talents)
                this.talents = talents;
            if (null != equipmentDic)
                this.equipmentDic = equipmentDic;
            if (null != skillDic)
                this.skillDic = skillDic;
        }

        public int GetLevel()
        {
            return level;
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

        //英雄等级属性
        public float GetStarAttribute(Enum.AttrType attrType)
        {
            var value = 0.0F;
            var attrs = GetStarConfig().Attrs;
            foreach (var v in attrs)
            {
                if ((Enum.AttrType)v.id == attrType)
                {
                    value = v.value;
                    break;
                }
            }

            if (ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", (int)attrType).ValueType == Enum.ValueType.Percentage)
                return value / 100.0f;
            else
                return value;
        }

        //天赋属性
        public float GetTalentAttribute(Enum.AttrType attrType)
        {
            var value = 0.0F;

            foreach (var v in talents)
            {
                var config = ConfigMgr.Instance.GetConfig<TalentConfig>("TalentConfig", v);
                foreach (var v1 in config.Attrs)
                {
                    if ((Enum.AttrType)v1.id == attrType)
                    {
                        value += v1.value;
                    }
                }
            }

            if (ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", (int)attrType).ValueType == Enum.ValueType.Percentage)
                return value / 100.0f;
            else
                return value;
        }

        //获取装备属性
        public float GetEquipAttribute(Enum.AttrType attrType)
        {
            var value = 0.0F;

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

        public virtual float GetAttribute(Enum.AttrType attrType)
        {
            var value = 0.0F;

            value += GetStarAttribute(attrType);
            value += GetTalentAttribute(attrType);
            value += GetEquipAttribute(attrType);

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
            return new RoleData(this.UID, this.configId, this.level, talents, cloneEquipDic, cloneSkillDic);
        }

        public virtual void Dispose()
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
        public string bornHexagonID;
        public List<BuffPair> buffs = new List<BuffPair>();
        public Dictionary<Enum.EquipPlace, EquipmentData> equipDataDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
        public Enum.RoleState state;
        public int cloneRole;

        public LevelRoleData(int UID, int configId, int level, string bornHexagonID, Enum.RoleState state, Dictionary<Enum.EquipPlace, EquipmentData> equipDataDic, List<int> talents) : base(UID, configId, level, talents, null)
        {
            this.equipDataDic = equipDataDic;
            this.state = state;
            this.HP = GetAttribute(Enum.AttrType.HP);
            this.Rage = 0;
            this.bornHexagonID = bornHexagonID;
            this.hexagonID = bornHexagonID;
        }

        public override float GetAttribute(Enum.AttrType attrType)
        {
            var value = base.GetAttribute(attrType);

            foreach (var v in equipDataDic)
            {
                value += v.Value.GetAttribute(attrType);
            }

            //计算buff属性
            foreach (var v in buffs)
            {
                var buffConfig = ConfigMgr.Instance.GetConfig<BufferConfig>("BufferConfig", v.id);
                if (buffConfig.EffectType != Enum.BuffAttrEffectType.Const)
                    continue;

                if ((Enum.AttrType)buffConfig.Attr.id == attrType)
                {
                    if (ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", buffConfig.Attr.id).ValueType == Enum.ValueType.Percentage)
                        value += buffConfig.Attr.value / 100.0f;
                    else
                        value += buffConfig.Attr.value;
                }
            }

            return value;
        }

        //添加buff
        public void AddBuffs(List<int> buffs, Enum.RoleType type, WGArgsCallback callback)
        {
            var startIndex = this.buffs.Count;
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                var buffConfig = ConfigMgr.Instance.GetConfig<BufferConfig>("BufferConfig", buffs[i]);
                this.buffs.Add(new BuffPair(buffs[i], buffConfig.Duration, type));
            }

            for (int i = startIndex; i < this.buffs.Count; i++)
            {
                UpdateBuff(i, type, callback);
            }
        }

        public void UpdateBuffs(Enum.RoleType type, WGArgsCallback callback)
        {
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                if (buffs[i].initiatorType != type)
                    continue;
                //DebugManager.Instance.Log(ConfigMgr.Instance.GetConfig<BufferConfig>("BufferConfig", buffs[i].id).Name);
                UpdateBuff(i, type, callback);
            }
        }

        private void UpdateBuff(int i, Enum.RoleType type, WGArgsCallback callback)
        {
            var buffConfig = ConfigMgr.Instance.GetConfig<BufferConfig>("BufferConfig", buffs[i].id);
            var attrType = (Enum.AttrType)buffConfig.Attr.id;
            if (buffs[i].value <= 0)
            {
                var buff = buffs[i];
                buffs.RemoveAt(i);

                callback(new object[] { buff.id, Enum.BuffUpdate.Delete, attrType, 0.0F });
            }
            else
            {
                var attrUpdateValue = 0.0f;
                if (buffConfig.EffectType == Enum.BuffAttrEffectType.Overlay)
                {
                    attrUpdateValue = buffConfig.Attr.value;
                    UpdateAttr(attrType, attrUpdateValue);
                }

                var buffUpdateType = Enum.BuffUpdate.None;
                if (buffs[i].value >= buffConfig.Duration)
                {
                    buffUpdateType = Enum.BuffUpdate.Add;
                }
                var buff = buffs[i];
                buffs[i] = new BuffPair(buffs[i].id, buffs[i].value - 1, buffs[i].initiatorType);
                callback(new object[] { buff.id, buffUpdateType, attrType, attrUpdateValue });
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

            EventDispatcher.Instance.PostEvent(Enum.Event.Role_Attr_Change, new object[] { UID, type, deltaValue });
        }

        public new LevelRoleData Clone()
        {
            var cloneEquipDataDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
            foreach (var v in equipDataDic)
            {
                cloneEquipDataDic.Add(v.Key, v.Value.Clone());
            }

            var clone = new LevelRoleData(this.UID, this.configId, this.level, bornHexagonID, state, cloneEquipDataDic, new List<int>());
            clone.hexagonID = hexagonID;
            clone.bornHexagonID = bornHexagonID;
            clone.HP = HP;
            clone.Rage = Rage;
            clone.state = state;

            var cloneBuffs = new List<BuffPair>();
            foreach (var v in buffs)
                cloneBuffs.Add(v);
            clone.buffs = cloneBuffs;

            return clone;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }

    /// <summary>
    /// 关卡数据
    /// </summary>
    [Serializable]
    public class LevelData
    {
        public int configId;
        public Enum.LevelStage Stage = Enum.LevelStage.None;
        public int Round;
        public Enum.ActionType actionType;
        public List<LevelRoleData> heros = new List<LevelRoleData>();
        public List<LevelRoleData> enemys = new List<LevelRoleData>();

        public LevelData(int configId)
        {
            this.configId = configId;
        }

        public void Clear()
        {
            Stage = Enum.LevelStage.None;
            Round = 0;
            heros.Clear();
            enemys.Clear();
        }

        public LevelData Clone()
        {
            var clone = new LevelData(configId);
            clone.Stage = Stage;
            clone.Round = Round;
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
    public class RecordData
    {
        public string ID;
        public string title;
        public long createTime;
        public long saveTime;
        public long duration;
        public bool isNew = true;
        public Dictionary<int, RoleData> roleDataDic = new Dictionary<int, RoleData>();
        public Dictionary<int, EquipmentData> equipDataDic = new Dictionary<int, EquipmentData>();
        public Dictionary<int, LevelData> levelDataDic = new Dictionary<int, LevelData>();
        private int _equipStartUID = 30000;
        public int homeEvent;
        public Dictionary<int, int> itemsDic = new Dictionary<int, int>();

        public RecordData(string ID, string title)
        {
            this.ID = ID;
            this.title = title;
            this.createTime = TimeMgr.Instance.GetUnixTimestamp();
        }

        public void AddHero(int heroID)
        {
            if (roleDataDic.ContainsKey(heroID))
                return;
            var heroConfig = ConfigMgr.Instance.GetConfig<HeroConfig>("HeroConfig", heroID);
            var roleData = new RoleData(heroID, heroConfig.RoleID, heroConfig.Level);
            roleDataDic.Add(roleData.UID, roleData);
        }

        public void AddEquip(int configId)
        {
            var equipData = new EquipmentData(_equipStartUID + equipDataDic.Count + 1, configId);
            equipDataDic.Add(equipData.UID, equipData);
        }

        public void AddItem(int itemId, int num)
        {
            if (!itemsDic.ContainsKey(itemId))
                itemsDic.Add(itemId, 0);

            itemsDic[itemId] += num;
        }

        public void RemoveItem(int itemId, int num)
        {
            if (!itemsDic.ContainsKey(itemId))
                return;
            itemsDic[itemId] = Math.Max(itemsDic[itemId] - num, 0);
        }

        public void Save()
        {
            duration += Math.Min(TimeMgr.Instance.GetUnixTimestamp() - saveTime, TimeMgr.Instance.GetGameDuration());
            saveTime = TimeMgr.Instance.GetUnixTimestamp();
        }

        public float GetDuration()
        {
            return duration + TimeMgr.Instance.GetGameDuration();
        }

        public RecordData Clone()
        {
            var gd = new RecordData(this.ID, this.title);
            gd.duration = duration;
            gd.saveTime = saveTime;
            gd.isNew = isNew;

            foreach (var v in roleDataDic)
            {
                gd.roleDataDic.Add(v.Key, v.Value.Clone());
            }
            foreach (var v in equipDataDic)
            {
                gd.equipDataDic.Add(v.Key, v.Value.Clone());
            }
            foreach (var v in levelDataDic)
            {
                gd.levelDataDic.Add(v.Key, v.Value.Clone());
            }
            return gd;
        }
    }

    [Serializable]
    public class GameData
    {
        public string Version = "0.0.0.01";
        private string _usingDataID;
        public Dictionary<string, RecordData> _customRecordDic = new Dictionary<string, RecordData>();
        public Dictionary<Enum.SoundType, float> SoundVolumeDic = new Dictionary<Enum.SoundType, float>()
        {
            { Enum.SoundType.Music, 0.05F},
            { Enum.SoundType.Audio, 0.1F},
        };
        public int Language;
        public bool SkipBattle;

        public GameData()
        {
        }

        public RecordData GetUsingRecord()
        {
            return _customRecordDic[_usingDataID];
        }

        public string GetID()
        {
            return "Record_" + (_customRecordDic.Count + 1);
        }

        public string GetTitle()
        {
            return "存档_" + (_customRecordDic.Count + 1);
        }

        public void StartNewGame()
        {
            var rd = new RecordData(GetID(), GetTitle());
            //rd.levelDataDic.Add(10001, new LevelData(10001));
            rd.AddHero(10001);
            rd.AddHero(10002);

            //ConfigMgr.Instance.ForeachConfig<EquipmentConfig>("EquipmentConfig", (config) =>
            //{
            //    rd.AddEquip(config.ID);
            //});
            //rd.AddEquip(10001);
            //rd.AddEquip(10002);
            //rd.AddEquip(10003);
            //rd.AddEquip(10004);
            //rd.AddEquip(10005);
            //rd.AddEquip(10006);
            //rd.AddEquip(10007);
            //rd.AddEquip(10008);
            //rd.AddEquip(10009);
            //rd.AddEquip(10010);
            //rd.AddEquip(10011);
            //rd.AddEquip(10012);
            //rd.AddEquip(10013);

            _customRecordDic.Add(rd.ID, rd);
            _usingDataID = rd.ID;
        }

        public void Save()
        {
            if (null == _usingDataID)
                return;
            _customRecordDic[_usingDataID].Save();
        }

        public void Start(string id)
        {
            _usingDataID = id;
        }

        public void SaveRecord(string id = null)
        {
            if (id == _usingDataID)
            {
                _customRecordDic[_usingDataID].Save();
                return;
            }

            var rd = _customRecordDic[_usingDataID].Clone();
            rd.Save();

            if (null != id)
            {
                rd.ID = _customRecordDic[id].ID;
                rd.title = _customRecordDic[id].title;
                _customRecordDic[id] = rd;
            }
            else
            {
                rd.ID = GetID();
                rd.title = GetTitle();
                _customRecordDic.Add(rd.ID, rd);
            }
        }

        public RecordData GetRecordData(string id)
        {
            return _customRecordDic[id];
        }

        public List<string> GetAllRecordDatas()
        {
            List<string> gameDatas = new List<string>();
            foreach (var v in _customRecordDic)
            {
                gameDatas.Add(v.Value.ID);
            }
            return gameDatas;
        }
    }

    //region 协议部分 -------------------------------------------------------------------------------------------
    public struct UnwearEquipNDPU
    {
        public Enum.ErrorCode ret;
        public int roleUID;
        public List<int> unwearEquips;

        //是否是从穿戴协议触发，避免ui重复刷新
        public bool fromWear;

        public UnwearEquipNDPU(Enum.ErrorCode ret, int roleUID, List<int> unwearEquips, bool fromWear)
        {
            this.ret = ret;
            this.roleUID = roleUID;
            this.unwearEquips = unwearEquips;
            this.fromWear = fromWear;
        }
    }

    public struct WearEquipNDPU
    {
        public Enum.ErrorCode ret;
        public int roleUID;
        public List<int> wearEquips;

        public WearEquipNDPU(Enum.ErrorCode ret, int roleUID, List<int> wearEquips)
        {
            this.ret = ret;
            this.roleUID = roleUID;
            this.wearEquips = wearEquips;
        }
    }
    //endregion -------------------------------------------------------------------------------------------------
}
