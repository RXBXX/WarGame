using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class BattleMgr : Singeton<BattleMgr>
    {
        private Dictionary<Enum.RoleType, Dictionary<int, Dictionary<Enum.AttrType, float>>> _reportDic;

        public void InitReports(Dictionary<Enum.RoleType, Dictionary<int, Dictionary<Enum.AttrType, float>>> reportDic)
        {
            _reportDic = reportDic;
            //_reportDic = new Dictionary<Enum.RoleType, Dictionary<int, Dictionary<Enum.AttrType, float>>>();
            //_reportDic[Enum.RoleType.Hero] = new Dictionary<int, Dictionary<Enum.AttrType, float>>();
            //_reportDic[Enum.RoleType.Enemy] = new Dictionary<int, Dictionary<Enum.AttrType, float>>();
            //foreach (var v in RoleManager.Instance.GetAllRoles())
            //{
            //    _reportDic[v.Type][v.ID] = new Dictionary<Enum.AttrType, float>();
            //}
        }

        ///计算元素克制加成
        public float GetElementAdd(Enum.Element levelElement, int _initiatorID, int targetID = 0)
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);

            var add = 0.0f;
            var initiatorElement = initiator.GetElement();

            //计算技能目标的属性克制
            if (0 != targetID)
            {
                var target = RoleManager.Instance.GetRole(targetID);
                var elementConfig = target.GetElementConfig();
                if (elementConfig.Restrain == initiatorElement)
                {
                    add -= elementConfig.RestrainValue;
                }

                var initiatorEC = initiator.GetElementConfig();
                if (initiatorEC.Restrain == target.GetElement())
                {
                    add += initiatorEC.RestrainValue;
                }
            }

            //计算队友元素加成
            var hexagon = MapManager.Instance.GetHexagon(initiator.Hexagon);
            foreach (var v in MapManager.Instance.Dicections)
            {
                var roleID = RoleManager.Instance.GetRoleIDByHexagonID(MapTool.Instance.GetHexagonKey(hexagon.coor + v));
                if (0 == roleID || roleID == _initiatorID)
                    continue;

                var role = RoleManager.Instance.GetRole(roleID);
                var elementConfig = role.GetElementConfig();
                if (initiator.Type == role.Type && elementConfig.Reinforce == initiatorElement)
                {
                    add += elementConfig.ReinforceValue;
                }
            }

            //计算关卡元素加成
            if (levelElement != Enum.Element.None)
            {
                var levelElementConfig = ConfigMgr.Instance.GetConfig<ElementConfig>("ElementConfig", (int)levelElement);
                if (levelElementConfig.Restrain == initiatorElement)
                {
                    add -= levelElementConfig.RestrainValue;
                }

                if (levelElementConfig.Reinforce == initiatorElement)
                {
                    add += levelElementConfig.RestrainValue;
                }
            }

            return add;
        }

        public float GetDayAdd(Enum.RoleType type)
        {
            var dayType = TimeMgr.Instance.GetDayType();
            var config = ConfigMgr.Instance.GetConfig<DayConfig>("DayConfig", (int)dayType);
            if (type == Enum.RoleType.Enemy)
                return config.ReinforceEnemy;
            else
                return config.ReinforceHero;
        }

        //public float GetAttackPower(int initiatorID, int targetID)
        //{
        //    var initiator = RoleManager.Instance.GetRole(initiatorID);
        //    var target = RoleManager.Instance.GetRole(targetID);

        //    var add = GetElementAdd(initiatorID, targetID);
        //    var initiatorPhysicalAttack = initiator.GetAttribute(Enum.AttrType.PhysicalAttack) * (1 + add);
        //    var initiatorPhysicalAttackRatio = initiator.GetAttribute(Enum.AttrType.PhysicalAttackRatio) * (1 + add);
        //    var initiatorMagicAttack = initiator.GetAttribute(Enum.AttrType.MagicAttack) * (1 + add);
        //    var initiatorMagicAttackRatio = initiator.GetAttribute(Enum.AttrType.MagicAttackRatio) * (1 + add);
        //    var initiatorPhysicalPenetrateRatio = initiator.GetAttribute(Enum.AttrType.PhysicalPenetrateRatio) * (1 + add);
        //    var initiatorMagicPenetrateRatio = initiator.GetAttribute(Enum.AttrType.MagicPenetrateRatio) * (1 + add);

        //    var targetPhysicalDefense = target.GetAttribute(Enum.AttrType.PhysicalDefense);
        //    var targetMagicDefense = target.GetAttribute(Enum.AttrType.MagicDefense);

        //    var physicalHurt = initiatorPhysicalAttack * (1 + initiatorPhysicalAttackRatio) - (1 - initiatorPhysicalPenetrateRatio) * targetPhysicalDefense;
        //    var magicHurt = initiatorMagicAttack * (1 + initiatorMagicAttackRatio) - (1 - initiatorMagicPenetrateRatio) * targetMagicDefense;
        //    return physicalHurt + magicHurt;
        //}

        /// <summary>
        /// 获取物理攻击力
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        public float GetPhysicalAttackPower(Enum.Element levelElement, int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);

            var add = GetElementAdd(levelElement, initiatorID, targetID) + GetDayAdd(initiator.Type);
            var initiatorPhysicalAttack = initiator.GetAttribute(Enum.AttrType.PhysicalAttack) * (1 + add);
            var initiatorPhysicalAttackRatio = initiator.GetAttribute(Enum.AttrType.PhysicalAttackRatio) * (1 + add);

            return initiatorPhysicalAttack * (1 + initiatorPhysicalAttackRatio);
        }

        /// <summary>
        /// 获取魔法攻击力
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        public float GetMagicAttackPower(Enum.Element levelElement, int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);

            var add = GetElementAdd(levelElement, initiatorID, targetID) + GetDayAdd(initiator.Type);
            var initiatorMagicAttack = initiator.GetAttribute(Enum.AttrType.MagicAttack) * (1 + add);
            var initiatorMagicAttackRatio = initiator.GetAttribute(Enum.AttrType.MagicAttackRatio) * (1 + add);

            return initiatorMagicAttack * (1 + initiatorMagicAttackRatio);
        }

        /// <summary>
        /// 获取物理防御力
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        public float GetPhysicalDefensePower(Enum.Element levelElement, int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var target = RoleManager.Instance.GetRole(targetID);

            var add = GetElementAdd(levelElement, initiatorID, targetID) + GetDayAdd(initiator.Type);
            var initiatorPhysicalPenetrateRatio = initiator.GetAttribute(Enum.AttrType.PhysicalPenetrateRatio) * (1 + add);

            var targetPhysicalDefense = target.GetAttribute(Enum.AttrType.PhysicalDefense);

            return (1 - initiatorPhysicalPenetrateRatio) * targetPhysicalDefense;
        }

        /// <summary>
        /// 获取魔法防御力
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        public float GetMagicDefensePower(Enum.Element levelElement, int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var target = RoleManager.Instance.GetRole(targetID);

            var add = GetElementAdd(levelElement, initiatorID, targetID) + GetDayAdd(initiator.Type);
            var initiatorMagicPenetrateRatio = initiator.GetAttribute(Enum.AttrType.MagicPenetrateRatio) * (1 + add);

            var targetMagicDefense = target.GetAttribute(Enum.AttrType.MagicDefense);

            return (1 - initiatorMagicPenetrateRatio) * targetMagicDefense;
        }

        /// <summary>
        /// 获取治愈力
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        public float GetCurePower(Enum.Element levelElement, int initiatorID, int targetID, bool addReport = false)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var add = GetElementAdd(levelElement, initiatorID, targetID) + GetDayAdd(initiator.Type);
            var curePower = initiator.GetAttribute(Enum.AttrType.Cure) * (1 + add);

            curePower = Mathf.Floor(curePower);

            if (addReport)
                AddReport(initiatorID, Enum.AttrType.Cure, curePower);
            return curePower;
        }

        /// <summary>
        /// 获取鼓舞力
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        public float GetInspirePower(Enum.Element levelElement, int initiatorID, int targetID, bool addReport = false)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var add = GetElementAdd(levelElement, initiatorID, targetID) + GetDayAdd(initiator.Type);
            var ragePower = initiator.GetAttribute(Enum.AttrType.Inspire) * (1 + add);

            ragePower = Mathf.Floor(ragePower);

            if (addReport)
                AddReport(initiatorID, Enum.AttrType.Rage, ragePower);
            return ragePower;
        }

        public string GetAttributeStr(int id, float value)
        {
            var attrConfig = ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", id);
            var valueStr = attrConfig.ValueType == Enum.ValueType.Int ? value.ToString() : string.Format("{0}%", value * 100);
            return value > 0 ? "+" + valueStr : valueStr;
            //return attrConfig.ValueType == Enum.ValueType.Int ? value.ToString() : string.Format("{0}%", value * 100);
        }

        public string GetAttributeColorStr(int id, float value)
        {
            var attrConfig = ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", id);
            var valueStr = attrConfig.ValueType == Enum.ValueType.Int ? value.ToString() : string.Format("{0}%", value * 100);
            if (value > 0)
                return "[color=#00a8ed]+" + valueStr + "[/color]";
            else
                return "[color=#ce4a35]" + valueStr + "[/color]";
            //return attrConfig.ValueType == Enum.ValueType.Int ? value.ToString() : string.Format("{0}%", value * 100);
        }


        public float GetAttackValue(Enum.Element levelElement, int initiatorID, int targetID, bool addReport = false, float multiply = 1)
        {
            var physicalDefense = GetPhysicalDefensePower(levelElement, initiatorID, targetID);
            var magicDefense = GetMagicDefensePower(levelElement, initiatorID, targetID);

            var phyAttack = GetPhysicalAttackPower(levelElement, initiatorID, targetID);
            var magAttack = GetMagicAttackPower(levelElement, initiatorID, targetID);

            var physicalHurt = Mathf.Max(0, phyAttack - physicalDefense);
            var magicHurt = Mathf.Max(0, magAttack - magicDefense);

            //DebugManager.Instance.Log(targetID + " 物理伤害：" + physicalHurt + " 魔法伤害：" + magicHurt + " Multiply:" + multiply);
            physicalHurt *= multiply;
            magicHurt *= multiply;

            physicalHurt = Mathf.Floor(physicalHurt);
            magicHurt = Mathf.Floor(magicHurt);

            if (addReport)
            {
                AddReport(targetID, Enum.AttrType.PhysicalDefense, physicalDefense);
                AddReport(targetID, Enum.AttrType.MagicDefense, magicDefense);
                AddReport(initiatorID, Enum.AttrType.PhysicalAttack, phyAttack);
                AddReport(initiatorID, Enum.AttrType.MagicAttack, magAttack);
            }
            //DebugManager.Instance.Log(physicalHurt + magicHurt);
            return physicalHurt + magicHurt;
        }

        /// <summary>
        /// 普通攻击
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        public void DoAttack(Enum.Element levelElement, int initiatorID, List<int> targets)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            foreach (var v in targets)
            {
                var target = RoleManager.Instance.GetRole(v);

                var dodgeRatio = target.GetAttribute(Enum.AttrType.DodgeRatio);
                var rd = Random.Range(0, 1.0f);
                if (rd < dodgeRatio)
                {
                    target.Dodge();
                }
                else
                {
                    var hurt = GetAttackValue(levelElement, initiatorID, v, true);
                    target.Hit(hurt, initiator.GetAttackEffect(), initiator.ID);
                    target.AddBuffs(initiator.GetAttackBuffs(), initiator.Type);
                    CameraMgr.Instance.ShakePosition();
                }
            }
        }

        /// <summary>
        /// 后撤攻击
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        public void DoRelocateAttack(Enum.Element levelElement, int initiatorID, List<int> targets)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            initiator.ClearRage();
            foreach (var v in targets)
            {
                var target = RoleManager.Instance.GetRole(v);
                var hurt = GetAttackValue(levelElement, initiatorID, v, true);
                target.Hit(hurt, initiator.GetAttackEffect(), initiator.ID);
                target.AddBuffs(initiator.GetAttackBuffs(), initiator.Type);
                CameraMgr.Instance.ShakePosition();
            }
        }

        /// <summary>
        /// 锁链攻击
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        public void DoChainAttack(Enum.Element levelElement, int initiatorID, List<int> targets)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            initiator.ClearRage();

            for (int i = targets.Count - 1; i >= 0; i--)
            {
                var target = RoleManager.Instance.GetRole(targets[i]);
                var hurt = GetAttackValue(levelElement, initiatorID, targets[i], true);
                target.Hit(hurt, initiator.GetAttackEffect(), initiator.ID);
                target.AddBuffs(initiator.GetAttackBuffs(), initiator.Type);
            }
            CameraMgr.Instance.ShakePosition();
        }

        /// <summary>
        /// 治疗
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        public void DoCure(Enum.Element levelElement, int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var target = RoleManager.Instance.GetRole(targetID);

            initiator.ClearRage();

            var cure = GetCurePower(levelElement, initiatorID, targetID, true);
            target.Cured(cure);
        }

        /// <summary>
        /// 群体治疗
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targets"></param>
        public void DoMassHeal(Enum.Element levelElement, int initiatorID, List<int> targets)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            initiator.ClearRage();

            foreach (var v in targets)
            {
                var target = RoleManager.Instance.GetRole(v);
                var cure = GetCurePower(levelElement, initiatorID, v, true);
                target.Cured(cure);
            }
        }

        /// <summary>
        /// 鼓舞
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targets"></param>
        public void DoInspire(Enum.Element levelElement, int initiatorID, List<int> targets)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            initiator.ClearRage();
            foreach (var v in targets)
            {
                var target = RoleManager.Instance.GetRole(v);
                target.Inspired(GetInspirePower(levelElement, initiatorID, v, true));
            }
        }

        /// <summary>
        /// 潜行
        /// </summary>
        /// <param name="initiatorID"></param>
        public void DoStealth(int initiatorID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            initiator.ClearRage();
            initiator.AddBuffs(new List<int> { (int)Enum.Buff.Cloaking }, initiator.Type);
        }

        /// <summary>
        /// 致命一击
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        public void DoCriticalHit(Enum.Element levelElement, int initiatorID, int targetID, float multiply)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            initiator.ClearRage();
            var target = RoleManager.Instance.GetRole(targetID);
            var hurt = GetAttackValue(levelElement, initiatorID, targetID, true, Random.Range(1.0f, multiply));
            target.Hit(hurt, initiator.GetAttackEffect(), initiator.ID);
            target.AddBuffs(initiator.GetAttackBuffs(), initiator.Type);
            CameraMgr.Instance.ShakePosition();
        }

        /// <summary>
        /// 分身
        /// </summary>
        /// <returns></returns>
        public Role DoClone(int initiatorID, int hexagon, int cloneUID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            initiator.ClearRage();
            var data = initiator.Clone(hexagon, cloneUID);
            return RoleManager.Instance.CreateRole(initiator.Type, data);
        }

        /// <summary>
        /// 晕眩
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        public void DoDizzy(Enum.Element levelElement, int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            initiator.ClearRage();

            var target = RoleManager.Instance.GetRole(targetID);
            var hurt = GetAttackValue(levelElement, initiatorID, targetID, true);
            target.Hit(hurt, initiator.GetAttackEffect(), initiator.ID);
            target.AddBuffs(initiator.GetAttackBuffs(), initiator.Type);
            target.AddBuffs(new List<int> { (int)Enum.Buff.Dizzy }, initiator.Type);
            CameraMgr.Instance.ShakePosition();
        }

        /// <summary>
        /// 额外一击
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        public void DoExtraTurn(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var target = RoleManager.Instance.GetRole(targetID);

            initiator.ClearRage();
            target.ExtraTurn();
        }

        /// <summary>
        /// 群体物理护盾
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targets"></param>
        public void DoMassPhyShiled(int initiatorID, List<int> targets)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            initiator.ClearRage();
            foreach (var v in targets)
            {
                var target = RoleManager.Instance.GetRole(v);
                target.AddShield(new List<int> { (int)Enum.Buff.MassPhyShield }, initiator.Type);
            }
        }

        /// <summary>
        /// 群体魔法护盾
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targets"></param>
        public void DoMassMagShiled(int initiatorID, List<int> targets)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            initiator.ClearRage();
            foreach (var v in targets)
            {
                var target = RoleManager.Instance.GetRole(v);
                target.AddShield(new List<int> { (int)Enum.Buff.MassMagShield }, initiator.Type);
            }
        }

        /// <summary>
        /// 单体物理护盾
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targets"></param>
        public void DoSinglePhyShiled(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            initiator.ClearRage();
            var target = RoleManager.Instance.GetRole(targetID);
            target.AddShield(new List<int> { (int)Enum.Buff.SinglePhyShield }, initiator.Type);
        }

        /// <summary>
        /// 单体魔法护盾
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targets"></param>
        public void DoSingleMagShiled(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            initiator.ClearRage();
            var target = RoleManager.Instance.GetRole(targetID);
            target.AddShield(new List<int> { (int)Enum.Buff.SingleMagShield }, initiator.Type);
        }

        /// <summary>
        /// 轮盘赌
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        public void DoRoulette(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var target = RoleManager.Instance.GetRole(targetID);

            initiator.ClearRage();

            var initiatorHP = initiator.GetHP();
            var targetHP = target.GetHP();
            var rd = Random.Range(0, initiatorHP + targetHP);
            if (rd < initiatorHP)
            {
                target.Hit(targetHP, initiator.GetAttackEffect(), initiator.ID);
                target.AddBuffs(initiator.GetAttackBuffs(), initiator.Type);
                AddReport(initiatorID, Enum.AttrType.MagicAttack, targetHP);
            }
            else
            {
                target.Hit(0, initiator.GetAttackEffect(), initiator.ID);
                initiator.Hit(initiatorHP, null, 0);
                AddReport(targetID, Enum.AttrType.MagicAttack, initiatorHP);
            }

            CameraMgr.Instance.ShakePosition();
        }

        /// <summary>
        /// 生命汲取
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        public void DoLifeDrain(Enum.Element levelElement, int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            initiator.ClearRage();
            var target = RoleManager.Instance.GetRole(targetID);

            var hurt = GetAttackValue(levelElement, initiatorID, targetID, true);
            target.Hit(hurt, initiator.GetAttackEffect(), initiator.ID);
            target.AddBuffs(initiator.GetAttackBuffs(), initiator.Type);

            initiator.AddHP(Mathf.Floor(hurt * 0.20F));

            CameraMgr.Instance.ShakePosition();
        }

        /// <summary>
        /// 群体减怒
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targets"></param>
        public void DoRageReduction(int initiatorID, List<int> targets, float ratio)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            initiator.ClearRage();

            foreach (var v in targets)
            {
                var target = RoleManager.Instance.GetRole(v);
                target.ReduceRage(ratio);
            }
        }

        public bool IsReadySpecialSkill(int initiatorID, Enum.Skill skill)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var rageEnough = initiator.GetRage() >= initiator.GetAttribute(Enum.AttrType.Rage);
            if (skill == Enum.Skill.Clone)
                return rageEnough && !initiator.HaveCloneRole();
            else
                return rageEnough;
        }

        public void AddReport(int id, Enum.AttrType attrType, float value)
        {
            //if (null == _reportDic)
            //    _reportDic = new Dictionary<Enum.RoleType, Dictionary<int, Dictionary<Enum.AttrType, float>>>();

            var initiator = RoleManager.Instance.GetRole(id);
            foreach (var v in RoleManager.Instance.GetAllRoles())
            {
                if (id == v.Data.cloneRole)
                {
                    id = v.ID;
                    break;
                }
            }

            var role = RoleManager.Instance.GetRole(id);
            if (!_reportDic.ContainsKey(role.Type))
            { 
                _reportDic[role.Type] = new Dictionary<int, Dictionary<Enum.AttrType, float>>();
            }
            if (!_reportDic[role.Type].ContainsKey(id))
            {
                _reportDic[role.Type][id] = new Dictionary<Enum.AttrType, float>();
            }
            if (!_reportDic[role.Type][id].ContainsKey(attrType))
            {
                _reportDic[role.Type][id][attrType] = 0;
            }

            _reportDic[role.Type][id][attrType] += value;
        }

        public Dictionary<Enum.RoleType, Dictionary<int, Dictionary<Enum.AttrType, float>>> GetReports()
        {
            return _reportDic;
        }

        public void ClearReports()
        {
            _reportDic = null;
        }
    }
}