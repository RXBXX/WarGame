using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class BattleMgr : Singeton<BattleMgr>
    {
        private Dictionary<Enum.RoleType, Dictionary<int, Dictionary<Enum.AttrType, float>>> _reportDic;

        ///����Ԫ�ؿ��Ƽӳ�
        public float GetElementAdd(int _initiatorID, int targetID = 0)
        {
            var initiator = RoleManager.Instance.GetRole(_initiatorID);

            var add = 0.0f;
            var initiatorElement = initiator.GetElement();

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

            return add;
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
        /// ��ȡ��������
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        public float GetPhysicalAttackPower(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);

            var add = GetElementAdd(initiatorID, targetID);
            var initiatorPhysicalAttack = initiator.GetAttribute(Enum.AttrType.PhysicalAttack) * (1 + add);
            var initiatorPhysicalAttackRatio = initiator.GetAttribute(Enum.AttrType.PhysicalAttackRatio) * (1 + add);

            return initiatorPhysicalAttack * (1 + initiatorPhysicalAttackRatio);
        }

        /// <summary>
        /// ��ȡħ��������
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        public float GetMagicAttackPower(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);

            var add = GetElementAdd(initiatorID, targetID);
            var initiatorMagicAttack = initiator.GetAttribute(Enum.AttrType.MagicAttack) * (1 + add);
            var initiatorMagicAttackRatio = initiator.GetAttribute(Enum.AttrType.MagicAttackRatio) * (1 + add);

            return initiatorMagicAttack * (1 + initiatorMagicAttackRatio);
        }

        /// <summary>
        /// ��ȡ���������
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        public float GetPhysicalDefensePower(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var target = RoleManager.Instance.GetRole(targetID);

            var add = GetElementAdd(initiatorID, targetID);
            var initiatorPhysicalPenetrateRatio = initiator.GetAttribute(Enum.AttrType.PhysicalPenetrateRatio) * (1 + add);

            var targetPhysicalDefense = target.GetAttribute(Enum.AttrType.PhysicalDefense);

            return (1 - initiatorPhysicalPenetrateRatio) * targetPhysicalDefense;
        }

        /// <summary>
        /// ��ȡħ��������
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        public float GetMagicDefensePower(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var target = RoleManager.Instance.GetRole(targetID);

            var add = GetElementAdd(initiatorID, targetID);
            var initiatorMagicPenetrateRatio = initiator.GetAttribute(Enum.AttrType.MagicPenetrateRatio) * (1 + add);

            var targetMagicDefense = target.GetAttribute(Enum.AttrType.MagicDefense);

            return (1 - initiatorMagicPenetrateRatio) * targetMagicDefense;
        }

        /// <summary>
        /// ��ȡ������
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        public float GetCurePower(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);

            var add = GetElementAdd(initiatorID, targetID);
            return initiator.GetAttribute(Enum.AttrType.Cure) * (1 + add);
        }

        /// <summary>
        /// ��ȡ������
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        public float GetInspirePower(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var add = GetElementAdd(initiatorID, targetID);
            return 20 * (1 + add);
        }

        public string GetAttributeStr(int id, float value)
        {
            var attrConfig = ConfigMgr.Instance.GetConfig<AttrConfig>("AttrConfig", id);
            return attrConfig.ValueType == Enum.ValueType.Int ? value.ToString() : string.Format("{0}%", value * 100);
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        public void DoAttack(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var target = RoleManager.Instance.GetRole(targetID);

            var dodgeRatio = target.GetAttribute(Enum.AttrType.DodgeRatio);
            var rd = Random.Range(0, 1.0f);
            if (rd < dodgeRatio)
            {
                target.Dodge();
            }
            else
            {
                var physicalDefense = GetPhysicalDefensePower(initiatorID, targetID);
                AddReport(targetID, Enum.AttrType.PhysicalDefense, physicalDefense);

                var magicDefense = GetMagicDefensePower(initiatorID, targetID);
                AddReport(targetID, Enum.AttrType.MagicDefense, magicDefense);

                var physicalHurt = GetPhysicalAttackPower(initiatorID, targetID) - physicalDefense;
                AddReport(initiatorID, Enum.AttrType.PhysicalAttack, physicalHurt);

                var magicHurt = GetMagicAttackPower(initiatorID, targetID) - magicDefense;
                AddReport(initiatorID, Enum.AttrType.MagicAttack, magicHurt);

                var hurt = Mathf.Max(0, physicalHurt + magicHurt);
                target.Hit(hurt, initiator.GetAttackEffect(), initiator.ID);
                target.AddBuffs(initiator.GetAttackBuffs(), initiator.Type);
                CameraMgr.Instance.ShakePosition();
            }
        }

        /// <summary>
        /// ��������
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        public void DoChainAttack(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var target = RoleManager.Instance.GetRole(targetID);

            var physicalDefense = GetPhysicalDefensePower(initiatorID, targetID);
            AddReport(targetID, Enum.AttrType.PhysicalDefense, physicalDefense);

            var magicDefense = GetMagicDefensePower(initiatorID, targetID);
            AddReport(targetID, Enum.AttrType.MagicDefense, magicDefense);

            var physicalHurt = GetPhysicalAttackPower(initiatorID, targetID) - physicalDefense;
            AddReport(initiatorID, Enum.AttrType.PhysicalAttack, physicalHurt);

            var magicHurt = GetMagicAttackPower(initiatorID, targetID) - magicDefense;
            AddReport(initiatorID, Enum.AttrType.MagicAttack, magicHurt);

            var hurt = Mathf.Max(0, physicalHurt + magicHurt);
            target.Hit(hurt, initiator.GetAttackEffect(), initiator.ID);
            target.AddBuffs(initiator.GetAttackBuffs(), initiator.Type);
            CameraMgr.Instance.ShakePosition();
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        public void DoCure(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var target = RoleManager.Instance.GetRole(targetID);

            initiator.ClearRage();

            var cure = GetCurePower(initiatorID, targetID);
            AddReport(initiatorID, Enum.AttrType.Cure, cure);
            target.Cured(cure);
        }

        /// <summary>
        /// Ⱥ������
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targets"></param>
        public void DoMassHeal(int initiatorID, List<int> targets)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            initiator.ClearRage();

            foreach (var v in targets)
            {
                var target = RoleManager.Instance.GetRole(v);
                var cure = GetCurePower(initiatorID, v);
                AddReport(initiatorID, Enum.AttrType.Cure, cure);
                target.Cured(cure);
            }
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targets"></param>
        public void DoInspire(int initiatorID, List<int> targets)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            initiator.ClearRage();
            foreach (var v in targets)
            {
                var target = RoleManager.Instance.GetRole(v);
                target.Inspired(GetInspirePower(initiatorID, v));
            }
        }

        /// <summary>
        /// Ǳ��
        /// </summary>
        /// <param name="initiatorID"></param>
        public void DoStealth(int initiatorID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            initiator.ClearRage();
            initiator.AddBuffs(new List<int> { (int)Enum.Buff.Cloaking }, initiator.Type);
        }

        /// <summary>
        /// ����һ��
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        public void DoCriticalHit(int initiatorID, int targetID, float multiply)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var target = RoleManager.Instance.GetRole(targetID);

            var dodgeRatio = target.GetAttribute(Enum.AttrType.DodgeRatio);
            var rd = Random.Range(0, 1.0f);
            if (rd < dodgeRatio)
            {
                target.Dodge();
            }
            else
            {
                var physicalDefense = GetPhysicalDefensePower(initiatorID, targetID);
                AddReport(targetID, Enum.AttrType.PhysicalDefense, physicalDefense);

                var magicDefense = GetMagicDefensePower(initiatorID, targetID);
                AddReport(targetID, Enum.AttrType.MagicDefense, magicDefense);

                var physicalHurt = GetPhysicalAttackPower(initiatorID, targetID) - physicalDefense;
                AddReport(initiatorID, Enum.AttrType.PhysicalAttack, physicalHurt);

                var magicHurt = GetMagicAttackPower(initiatorID, targetID) - magicDefense;
                AddReport(initiatorID, Enum.AttrType.MagicAttack, magicHurt);

                var hurt = physicalHurt + magicHurt;
                hurt *= Random.Range(1, multiply);
                hurt = Mathf.Max(0, hurt);
                target.Hit(hurt, initiator.GetAttackEffect(), initiator.ID);
                target.AddBuffs(initiator.GetAttackBuffs(), initiator.Type);
                CameraMgr.Instance.ShakePosition();
            }
        }

        /// <summary>
        /// ����
        /// </summary>
        /// <returns></returns>
        public int DoClone(int initiatorID, int hexagon)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var data = initiator.Clone(hexagon);
            RoleManager.Instance.CreateRole(initiator.Type, data);
            return data.UID;
        }

        /// <summary>
        /// ��ѣ
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        public void DoDizzy(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var target = RoleManager.Instance.GetRole(targetID);

            initiator.ClearRage();

            var physicalDefense = GetPhysicalDefensePower(initiatorID, targetID);
            AddReport(targetID, Enum.AttrType.PhysicalDefense, physicalDefense);

            var magicDefense = GetMagicDefensePower(initiatorID, targetID);
            AddReport(targetID, Enum.AttrType.MagicDefense, magicDefense);

            var physicalHurt = GetPhysicalAttackPower(initiatorID, targetID) - physicalDefense;
            AddReport(initiatorID, Enum.AttrType.PhysicalAttack, physicalHurt);

            var magicHurt = GetMagicAttackPower(initiatorID, targetID) - magicDefense;
            AddReport(initiatorID, Enum.AttrType.MagicAttack, magicHurt);

            var hurt = Mathf.Max(0, physicalHurt + magicHurt);
            target.Hit(hurt, initiator.GetAttackEffect(), initiator.ID);
            target.AddBuffs(initiator.GetAttackBuffs(), initiator.Type);

            target.AddBuffs(new List<int> { (int)Enum.Buff.Dizzy }, initiator.Type);
        }

        /// <summary>
        /// ����һ��
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
        /// Ⱥ��������
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targets"></param>
        public void DoMassPhyShiled(int initiatorID, List<int> targets)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);

            foreach (var v in targets)
            {
                var target = RoleManager.Instance.GetRole(v);
                target.AddShield(new List<int> { (int)Enum.Buff.MassPhyShield }, initiator.Type);
            }
        }

        /// <summary>
        /// Ⱥ��ħ������
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targets"></param>
        public void DoMassMagShiled(int initiatorID, List<int> targets)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);

            foreach (var v in targets)
            {
                var target = RoleManager.Instance.GetRole(v);
                target.AddShield(new List<int> { (int)Enum.Buff.MassMagShield }, initiator.Type);
            }
        }

        /// <summary>
        /// ����������
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targets"></param>
        public void DoSinglePhyShiled(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var target = RoleManager.Instance.GetRole(targetID);
            target.AddShield(new List<int> { (int)Enum.Buff.SinglePhyShield }, initiator.Type);
        }

        /// <summary>
        /// ����ħ������
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targets"></param>
        public void DoSingleMagShiled(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var target = RoleManager.Instance.GetRole(targetID);
            target.AddShield(new List<int> { (int)Enum.Buff.SingleMagShield }, initiator.Type);
        }

        /// <summary>
        /// ���̶�
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        public void DoRoulette(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var target = RoleManager.Instance.GetRole(targetID);

            var initiatorHP = initiator.GetHP();
            var targetHP = target.GetHP();
            var rd = Random.Range(0, initiatorHP + targetHP);
            if (rd < initiatorHP)
            {
                target.Hit(targetHP, initiator.GetAttackEffect(), initiator.ID);
                target.AddBuffs(initiator.GetAttackBuffs(), initiator.Type);
            }
            else
            {
                target.Hit(0, initiator.GetAttackEffect(), initiator.ID);
                initiator.Hit(initiatorHP, null, 0);
            }

            DebugManager.Instance.Log(initiator.DeadFlag);
            CameraMgr.Instance.ShakePosition();
        }

        /// <summary>
        /// ������ȡ
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        public void DoLefeDrain(int initiatorID, int targetID)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);
            var target = RoleManager.Instance.GetRole(targetID);

            var physicalDefense = GetPhysicalDefensePower(initiatorID, targetID);
            AddReport(targetID, Enum.AttrType.PhysicalDefense, physicalDefense);

            var magicDefense = GetMagicDefensePower(initiatorID, targetID);
            AddReport(targetID, Enum.AttrType.MagicDefense, magicDefense);

            var physicalHurt = GetPhysicalAttackPower(initiatorID, targetID) - physicalDefense;
            AddReport(initiatorID, Enum.AttrType.PhysicalAttack, physicalHurt);

            var magicHurt = GetMagicAttackPower(initiatorID, targetID) - magicDefense;
            AddReport(initiatorID, Enum.AttrType.MagicAttack, magicHurt);

            var hurt = Mathf.Max(0, physicalHurt + magicHurt);
            target.Hit(hurt, initiator.GetAttackEffect(), initiator.ID);
            target.AddBuffs(initiator.GetAttackBuffs(), initiator.Type);

            initiator.AddHP(hurt);

            CameraMgr.Instance.ShakePosition();
        }

        /// <summary>
        /// Ⱥ���ŭ
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targets"></param>
        public void DoRageReduction(int initiatorID, List<int> targets, float ratio)
        {
            var initiator = RoleManager.Instance.GetRole(initiatorID);

            foreach (var v in targets)
            {
                var target = RoleManager.Instance.GetRole(v);
                target.ReduceRage(ratio);
            }
        }

        public void InitReports()
        {
            _reportDic = new Dictionary<Enum.RoleType, Dictionary<int, Dictionary<Enum.AttrType, float>>>();
            _reportDic[Enum.RoleType.Hero] = new Dictionary<int, Dictionary<Enum.AttrType, float>>();
            _reportDic[Enum.RoleType.Enemy] = new Dictionary<int, Dictionary<Enum.AttrType, float>>();
            foreach (var v in RoleManager.Instance.GetAllRoles())
            {
                _reportDic[v.Type][v.ID] = new Dictionary<Enum.AttrType, float>();
            }
        }

        public void AddReport(int id, Enum.AttrType attrType, float value)
        {
            //if (null == _reportDic)
            //    _reportDic = new Dictionary<Enum.RoleType, Dictionary<int, Dictionary<Enum.AttrType, float>>>();

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