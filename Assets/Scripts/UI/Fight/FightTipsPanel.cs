using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class FightTipsPanel : UIBase
    {
        private int _levelID;
        private int _soundID;

        public FightTipsPanel(GComponent gCom, string customName, params object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;

            _levelID = (int)args[0];
            var levelConfig = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", _levelID);
            GetGObjectChild<GTextField>("title").text = levelConfig.GetTranslation("Name");
            GetGObjectChild<GTextField>("desc").text = levelConfig.GetTranslation("TargetDesc");
            GetGObjectChild<GTextField>("tips").text = ConfigMgr.Instance.GetTranslation("FightTipsPanel_Tips");

            var cancelBtn = GetGObjectChild<GButton>("cancelBtn");
            cancelBtn.title = ConfigMgr.Instance.GetTranslation("FightTipsPanel_Cancel");
            cancelBtn.onClick.Add(OnCancel);

            
            if (DatasMgr.Instance.IsLevelEntered(_levelID))
            {
                var startBtn = GetGObjectChild<GButton>("startBtn");
                startBtn.title = ConfigMgr.Instance.GetTranslation("FightTipsPanel_Continue");
                startBtn.onClick.Add(OnStart);

                var restartBtn = GetGObjectChild<GButton>("restartBtn");
                restartBtn.visible = true;
                restartBtn.title = ConfigMgr.Instance.GetTranslation("FightTipsPanel_Restart");
                restartBtn.onClick.Add(OnRestart);

            }
            else
            {
                var startBtn = GetGObjectChild<GButton>("startBtn");
                startBtn.title = ConfigMgr.Instance.GetTranslation("FightTipsPanel_Start");
                startBtn.onClick.Add(OnStart);

                var restartBtn = GetGObjectChild<GButton>("restartBtn");
                restartBtn.visible = false;
            }

            _soundID = AudioMgr.Instance.PlaySound("Assets/Audios/Warning.mp3", true);
        }

        public override void Dispose(bool disposeGCom = false)
        {
            if (0 != _soundID)
            {
                AudioMgr.Instance.StopSound(_soundID);
                _soundID = 0;
            }
            base.Dispose(disposeGCom);
        }

        private void OnStart()
        {
            UIManager.Instance.ClosePanel(name);
            UIManager.Instance.ClosePanel("MapPanel");
            SceneMgr.Instance.OpenBattleField(_levelID, false);
        }

        private void OnCancel()
        {
            UIManager.Instance.ClosePanel(name);
        }

        private void OnRestart()
        {
            UIManager.Instance.ClosePanel(name);
            UIManager.Instance.ClosePanel("MapPanel");
            SceneMgr.Instance.OpenBattleField(_levelID, true);
        }
    }
}
