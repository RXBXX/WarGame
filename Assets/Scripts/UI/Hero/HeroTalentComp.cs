using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class HeroTalentComp : UIBase
    {
        private List<HeroTalentItem> _talents = new List<HeroTalentItem>();

        public HeroTalentComp(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
        }

        public void UpdateComp(int heroUID, List<int> talents = null)
        {
            ClearTalents();

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

                var insideDiameter = 80 * Mathf.Cos(30 / 180f * Mathf.PI);
                ui.SetPosition(new Vector2(60 + line * 80 + column % 2 * 40, 60 + column * insideDiameter));
                ui.UpdateItem(heroUID, config.ID, config.Icon);
                _talents.Add(ui);
            });

            _gCom.EnsureBoundsCorrect();
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

        private void ClearTalents()
        {
            foreach (var v in _talents)
            {
                v.Dispose();
            }
            _talents.Clear();
        }

        public override void Dispose(bool disposeGCom = false)
        {
            ClearTalents();
            base.Dispose(disposeGCom);
        }
    }
}
