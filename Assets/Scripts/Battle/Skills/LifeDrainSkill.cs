namespace WarGame
{
    public class LifeDrainSkill : FierceAttackSkill
    {
        public LifeDrainSkill(int id, int initiatorID, int levelID) : base(id, initiatorID, levelID)
        { }

        public override void HandleFightEvents(int sender, string stateName, string secondStateName)
        {
            if (sender != _initiatorID)
                return;

            if ("Attack" == stateName && "Take" == secondStateName)
            {
                BattleMgr.Instance.DoLifeDrain(ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", _levelID).Element, _initiatorID, _targets[0]);
            }
        }
    }
}
