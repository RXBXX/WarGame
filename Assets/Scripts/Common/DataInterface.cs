using System.Collections.Generic;
using System;
using UnityEngine;

namespace WarGame
{
    [Serializable]
    public class EquipmentData
    {
        public int UID;
        public int id;

        public EquipmentData(int UID, int id)
        {
            this.UID = UID;
            this.id = id;
        }

        public int GetConfigID()
        {
            return id;
        }

        public string GetName()
        {
            return GetConfig().GetTranslation("Name");
        }

        public string GetIcon()
        {
            return GetConfig().Icon;
        }

        public EquipmentConfig GetConfig()
        {
            return ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", id);
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
            return new EquipmentData(this.UID, this.id);
        }
    }

    [Serializable]
    public class RoleData
    {
        public int UID;
        public int configId;
        public int level;
        public Dictionary<Enum.EquipPlace, int> equipmentDic = new Dictionary<Enum.EquipPlace, int>();
        public List<int> talents = new List<int>();

        public RoleData(int UID, int configId, int level)
        {
            this.UID = UID;
            this.configId = configId;
            this.level = level;
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
            //DebugManager.Instance.Log(configId * 1000 + level);
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

            var attrConfigID = (int)attrType;
            var attrConfig = ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", attrConfigID);
            if (null == attrConfig)
            {
                DebugManager.Instance.Log("Didn't find Attr:" + attrConfigID);
                return value;
            }

            if (attrConfig.ValueType == Enum.ValueType.Percentage)
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

            var attrConfigID = (int)attrType;
            var attrConfig = ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", attrConfigID);
            if (null == attrConfig)
            {
                DebugManager.Instance.Log("Didn't find Attr:" + attrConfigID);
                return value;
            }

            if (attrConfig.ValueType == Enum.ValueType.Percentage)
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
                    var equipConfig = ConfigMgr.Instance.GetConfig<EquipmentConfig>("EquipmentConfig", equipData.id);
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


            var attrConfigID = (int)attrType;
            var attrConfig = ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", attrConfigID);
            if (null == attrConfig)
            {
                DebugManager.Instance.Log("Didn't find Attr:" + attrConfigID);
                return value;
            }

            if (attrConfig.ValueType == Enum.ValueType.Percentage)
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
            var roleData = new RoleData(this.UID, this.configId, this.level);

            foreach (var v in this.equipmentDic)
            {
                roleData.equipmentDic.Add(v.Key, v.Value);
            }

            foreach (var v in talents)
            {
                roleData.talents.Add(v);
            }

            return roleData;
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
        public int hexagonID;
        public int bornHexagonID;
        public List<BuffPair> buffs = new List<BuffPair>();
        public Dictionary<Enum.EquipPlace, EquipmentData> equipDataDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
        public Enum.RoleState state;
        public int cloneRole;

        public LevelRoleData(int UID, int configId, int level, int bornHexagonID, Enum.RoleState state, Dictionary<Enum.EquipPlace, EquipmentData> equipDataDic, List<int> talents) : base(UID, configId, level)
        {
            this.equipDataDic = equipDataDic;
            this.state = state;
            this.Rage = 0;
            this.bornHexagonID = bornHexagonID;
            this.hexagonID = bornHexagonID;
            if (null != talents)
                this.talents = talents;
            this.HP = GetAttribute(Enum.AttrType.HP);
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
        public void AddBuffs(List<int> buffs, Enum.RoleType type, WGArgsCallback callback = null)
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

                if (null != callback)
                    callback(new object[] { buff.id, Enum.BuffUpdate.Delete, attrType, 0.0F });
            }
            else
            {
                var attrUpdateValue = 0.0f;
                if (buffConfig.EffectType == Enum.BuffAttrEffectType.Overlay)
                {
                    attrUpdateValue = buffConfig.Attr.value;
                    attrUpdateValue = UpdateAttr(attrType, attrUpdateValue);
                }

                var buffUpdateType = Enum.BuffUpdate.None;
                if (buffs[i].value >= buffConfig.Duration)
                {
                    buffUpdateType = Enum.BuffUpdate.Add;
                }
                var buff = buffs[i];
                buffs[i] = new BuffPair(buffs[i].id, buffs[i].value - 1, buffs[i].initiatorType);
                if (null != callback)
                    callback(new object[] { buff.id, buffUpdateType, attrType, attrUpdateValue });
            }
        }

        public float UpdateAttr(Enum.AttrType type, float deltaValue)
        {
            if (0 == deltaValue)
                return 0;

            float oldValue = 0.0f, newValue = 0.0f;
            switch (type)
            {
                case Enum.AttrType.HP:
                    oldValue = HP;
                    HP += deltaValue;
                    HP = MathF.Min(MathF.Max(0, HP), GetAttribute(type));
                    newValue = HP;
                    break;
                case Enum.AttrType.Rage:
                    oldValue = Rage;
                    Rage += deltaValue;
                    Rage = MathF.Min(MathF.Max(0, Rage), GetAttribute(type));
                    newValue = Rage;
                    break;
            }

            deltaValue = newValue - oldValue;
            EventDispatcher.Instance.PostEvent(Enum.Event.Role_Attr_Change, new object[] { UID, type, deltaValue });
            return deltaValue;
        }

        public new LevelRoleData Clone()
        {
            var cloneEquipDataDic = new Dictionary<Enum.EquipPlace, EquipmentData>();
            foreach (var v in equipDataDic)
            {
                cloneEquipDataDic.Add(v.Key, v.Value.Clone());
            }

            var cloneTalents = new List<int>();
            foreach (var v in talents)
            {
                cloneTalents.Add(v);
            }
            DebugManager.Instance.Log(cloneTalents.Count);

            var clone = new LevelRoleData(this.UID, this.configId, this.level, bornHexagonID, state, cloneEquipDataDic, cloneTalents);
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

    [Serializable]
    public class BonfireData
    {
        public int UID;
        public int ConfigID;
        public int Hexagon;
        public float FiredTime;

        public BonfireData(int UID, int configID, int hexagon)
        {
            this.UID = UID;
            this.ConfigID = configID;
            this.Hexagon = hexagon;
        }

        public bool IsFired()
        {
            if (0 == FiredTime)
                return false;

            return TimeMgr.Instance.GetGameTime() - FiredTime <= GetConfig().Duration;
        }

        public void OutFire()
        {
            //FiredTime = TimeMgr.Instance.GetGameTime();
        }

        public void Fire()
        {
            FiredTime = TimeMgr.Instance.GetGameTime();
        }

        public BonfireConfig GetConfig()
        {
            return ConfigMgr.Instance.GetConfig<BonfireConfig>("BonfireConfig", ConfigID);
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
        public List<LevelRoleData> heros;
        public List<LevelRoleData> enemys;
        public List<BonfireData> bonfires;
        public Dictionary<int, int> itemsDic;
        public Dictionary<Enum.RoleType, Dictionary<int, Dictionary<Enum.AttrType, float>>> reportDic;
        public int minPassRound = 0;

        public LevelData(int configId)
        {
            this.configId = configId;
            this.heros = new List<LevelRoleData>();
            this.enemys = new List<LevelRoleData>();
            this.itemsDic = new Dictionary<int, int>();
            this.reportDic = new Dictionary<Enum.RoleType, Dictionary<int, Dictionary<Enum.AttrType, float>>>();
        }

        public void Clear()
        {
            Stage = Enum.LevelStage.None;
            Round = 0;
            heros.Clear();
            enemys.Clear();
            itemsDic.Clear();
        }

        private void AddItem(int id, int value)
        {
            if (!itemsDic.ContainsKey(id))
                itemsDic.Add(id, 0);
            itemsDic[id] += value;
        }

        private void RemoveItem(int id, int value)
        {
            if (!itemsDic.ContainsKey(id))
                return;
            itemsDic[id] = Mathf.Max(0, itemsDic[id] - value);
        }

        public Dictionary<int, int> GetItems()
        {
            return itemsDic;
        }


        public void AddItems(List<TwoIntPair> items)
        {
            foreach (var v in items)
                AddItem(v.id, v.value);
            EventDispatcher.Instance.PostEvent(Enum.Event.Item_Update);
        }

        public void RemoveItems(List<TwoIntPair> items)
        {
            foreach (var v in items)
                RemoveItem(v.id, v.value);
            EventDispatcher.Instance.PostEvent(Enum.Event.Item_Update);
        }

        public void ClearItems()
        {
            itemsDic.Clear();
            EventDispatcher.Instance.PostEvent(Enum.Event.Item_Update);
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
        public int homeEvent;
        public Dictionary<int, int> itemsDic = new Dictionary<int, int>();
        //缓存上阵的英雄
        public List<int> SelectedHeros;
        public float Time;

        public RecordData(string ID, string title)
        {
            this.ID = ID;
            this.title = title;
            this.createTime = TimeMgr.Instance.GetUnixTimestamp();
            Time = 250F;
        }

        public void AddHero(int heroID)
        {
            if (roleDataDic.ContainsKey(heroID))
                return;
            var heroConfig = ConfigMgr.Instance.GetConfig<HeroConfig>("HeroConfig", heroID);
            var roleData = new RoleData(heroID, heroConfig.RoleID, heroConfig.Level);
            roleDataDic.Add(roleData.UID, roleData);
        }

        public int AddEquip(int configId)
        {
            var equipData = new EquipmentData(30000 + equipDataDic.Count + 1, configId);
            equipDataDic.Add(equipData.UID, equipData);
            return equipData.UID;
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

        public bool ContainEquip(int id)
        {
            return equipDataDic.ContainsKey(id);
        }

        public string GetIcon()
        {
            var lastMainLevel = 10001;
            foreach (var v in levelDataDic)
            {
                var config = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", v.Key);
                if (config.Type == Enum.LevelType.Main && config.ID > lastMainLevel)
                {
                    lastMainLevel = config.ID;
                }
            }

            return ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", lastMainLevel).Icon;
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
            { Enum.SoundType.Music, 0.8F},
            { Enum.SoundType.Audio, 0.8F},
        };
        public int Language;
        public bool SkipBattle;

        public GameData()
        {
            var language = Application.systemLanguage;
            switch (language)
            {
                case SystemLanguage.ChineseSimplified:
                    Language = 2;
                    break;
                case SystemLanguage.Russian:
                    Language = 3;
                    break;
                case SystemLanguage.Spanish:
                    Language = 4;
                    break;
                case SystemLanguage.Portuguese:
                    Language = 5;
                    break;
                case SystemLanguage.German:
                    Language = 6;
                    break;
                case SystemLanguage.Japanese:
                    Language = 7;
                    break;
                case SystemLanguage.French:
                    Language = 8;
                    break;
                case SystemLanguage.Polish:
                    Language = 9;
                    break;
                case SystemLanguage.Korean:
                    Language = 10;
                    break;
                case SystemLanguage.ChineseTraditional:
                    Language = 11;
                    break;
                case SystemLanguage.Turkish:
                    Language = 12;
                    break;
                default:
                    Language = 1;
                    break;
            }
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
            return (_customRecordDic.Count + 1).ToString();
        }

        public void StartNewGame()
        {
            var rd = new RecordData(GetID(), GetTitle());
            //rd.levelDataDic.Add(10001, new LevelData(10001));
            rd.AddHero(10001);
            rd.AddHero(10002);

            rd.AddEquip(10016);
            rd.AddEquip(10056);

            //rd.AddItem((int)Enum.ItemType.EquipRes, 10000);
            //rd.AddItem((int)Enum.ItemType.LevelRes, 10000);
            //rd.AddItem((int)Enum.ItemType.TalentRes, 10000);

            _customRecordDic.Add(rd.ID, rd);
            _usingDataID = rd.ID;
        }

        public void Save()
        {
            if (null == _usingDataID)
                return;

            if (!_customRecordDic.ContainsKey(_usingDataID))
                return;

            _customRecordDic[_usingDataID].Save();
        }

        public void Start(string id)
        {
            _usingDataID = id;
        }

        public void Update(float deltaTime)
        {
            if (null == _usingDataID)
                return;
            GetUsingRecord().Time += deltaTime;
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

        public void DeleteRecord(string id)
        {
            if (!_customRecordDic.ContainsKey(id))
                return;
            _customRecordDic.Remove(id);
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
