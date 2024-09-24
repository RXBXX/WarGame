using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class HeroTalentComp : UIBase
    {
        private Dictionary<int, HeroTalentItem> _talentsDic = new Dictionary<int, HeroTalentItem>();

        public HeroTalentComp(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            Init();
        }

        private void Init()
        {
            Dictionary<int, int> talentColumns = new Dictionary<int, int>();
            ConfigMgr.Instance.ForeachConfig<TalentConfig>("TalentConfig", (config) =>
            {
                int line = 0, column = 0;
                TalentConfig tempConfig = config;
                while (0 != tempConfig.LastTalent)
                {
                    line++;
                    tempConfig = ConfigMgr.Instance.GetConfig<TalentConfig>("TalentConfig", tempConfig.LastTalent);
                }

                if (talentColumns.ContainsKey(tempConfig.ID))
                {
                    column = talentColumns[tempConfig.ID];
                }
                else
                {
                    column = talentColumns.Count;
                    talentColumns.Add(tempConfig.ID, column);
                }

                var ui = UIManager.Instance.CreateUI<HeroTalentItem>("Hero", "HeroTalentItem");
                ui.SetParent(GCom);

                ui.SetPosition(new Vector2(70 + line * 100, 50 + column * 110));
                //ui.UpdateItem(heroUID, config.ID);
                _talentsDic.Add(config.ID, ui);
            });
        }

        public void UpdateComp(int heroUID)
        {
            ConfigMgr.Instance.ForeachConfig<TalentConfig>("TalentConfig", (config) =>
            {
                _talentsDic[config.ID].UpdateItem(heroUID, config.ID);
            });

            _gCom.EnsureBoundsCorrect();
        }

        public void ActiveTalent(int talentID)
        {
            if (!_talentsDic.ContainsKey(talentID))
                return;
            _talentsDic[talentID].Active();
        }

        private void ClearTalents()
        {
            foreach (var v in _talentsDic)
            {
                v.Value.Dispose();
            }
            _talentsDic.Clear();
        }

        public override void Dispose(bool disposeGCom = false)
        {
            ClearTalents();
            base.Dispose(disposeGCom);
        }
    }
}
