using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class HeroTalentComp : UIBase
    {
        private List<HeroTalentItem> _talents = new List<HeroTalentItem>();
        public HeroTalentComp(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            for (int i = 0; i < 13; i++)
            {
                _talents.Add(GetChild<HeroTalentItem>("talent_" + i));
            }
        }

        public void UpdateComp(int heroUID, int talentGroup, Dictionary<int, int> talentDic = null)
        {
            for (int i = 0; i < 13; i++)
            {
                _talents[i].UpdateItem(heroUID, talentGroup * 1000 + i + 1, null!= talentDic ? talentDic.ContainsKey(i + 1):false);
            }
        }

        public void ActiveTalent(int talentID)
        {
            foreach (var v in _talents)
            {
                if (v.GetID() == talentID)
                {
                    v.Active();
                    break;
                }
            }
        }

        public override void Dispose(bool disposeGCom = false)
        {
            for (int i = 0; i < 13; i++)
            {
                _talents[i].Dispose();
            }
            base.Dispose(disposeGCom);
        }
    }
}
