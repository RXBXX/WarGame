using FairyGUI;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class HeroTalentComp : UIBase
    {
        private int _heroUID;
        private List<HeroTalentItem> _talents = new List<HeroTalentItem>();
        public HeroTalentComp(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            for (int i = 0; i < 13; i++)
            {
                _talents.Add(GetChild<HeroTalentItem>("talent_" + i));
            }

            EventDispatcher.Instance.AddListener(Enum.EventType.HeroTalentActiveS2C, OnHeroTalentActiveS2C);
        }

        public void UpdateComp(int heroUID, int talentGroup, Dictionary<int, int> talentDic = null)
        {
            this._heroUID = heroUID;
            for (int i = 0; i < 13; i++)
            {
                _talents[i].UpdateItem(heroUID, talentGroup * 1000 + i + 1, null!= talentDic ? talentDic.ContainsKey(i + 1):false);
            }
        }

        private void OnHeroTalentActiveS2C(params object[] args)
        {
            DebugManager.Instance.Log("ONHeroTalent");
            if (_heroUID != (int)args[0])
                return;
            DebugManager.Instance.Log("ONHeroTalent 11111");
            var talentID = (int)args[1];
            foreach (var v in _talents)
            {
                if (v.GetID() == talentID)
                {
                    DebugManager.Instance.Log("ONHeroTalent 2222");
                    v.Active();
                    break;
                }
            }
        }

        public override void Dispose(bool disposeGCom = false)
        {
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HeroTalentActiveS2C, OnHeroTalentActiveS2C);

            for (int i = 0; i < 13; i++)
            {
                _talents[i].Dispose();
            }
            base.Dispose(disposeGCom);
        }
    }
}
