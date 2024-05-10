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

        public void UpdateComp(int talentGroup, Dictionary<int, int> talentDic)
        {
            for (int i = 0; i < 13; i++)
            {
                _talents[i].UpdateItem(talentGroup * 1000 + i + 1, talentDic.ContainsKey(i + 1));
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
