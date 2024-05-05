using FairyGUI;

namespace WarGame.UI
{
    public class HeroSkillComp : UIBase
    {
        public HeroSkillComp(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        { }

        public void UpdateComp()
        { }
        //private void UpdateSkills(int roleUID)
        //{
        //    var roleData = DatasMgr.Instance.GetRoleData(roleUID);
        //    var roleConfig = ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", roleData.configId);
        //    var commonSkillConfig = ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", roleConfig.CommonSkill.id);
        //    _commonSkill.title = commonSkillConfig.Name;
        //    var specialSkillConfig = ConfigMgr.Instance.GetConfig<SkillConfig>("SkillConfig", roleConfig.SpecialSkill.id);
        //    _specialSkill.title = specialSkillConfig.Name;
        //}
    }
}
