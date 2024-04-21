using DG.Tweening;
using WarGame.UI;

namespace WarGame
{
    public class Game : Singeton<Game>
    {
        private bool _isInited;
        private bool _isStarted;

        public override bool Init()
        {
            if (_isInited)
                return false;
            _isInited = true;

            base.Init();

            DebugManager.Instance.Init();
            EventDispatcher.Instance.Init();
            InputManager.Instance.Init();
            HUDManager.Instance.Init();
            MapManager.Instance.Init();
            MapTool.Instance.Init();
            UIManager.Instance.Init();
            RoleManager.Instance.Init();
            SceneMgr.Instance.Init();

            DOTween.Init(true, true);

            return true;
        }

        public bool Start()
        {
            if (_isStarted)
                return false;
            _isStarted = true;

            UIManager.Instance.OpenPanel("Main", "MainPanel");
            return true;
        }

        public void Update()
        {
            InputManager.Instance.Update();
            HUDManager.Instance.Update();
            RoleManager.Instance.Update();
        }

        public void LateUpdate()
        {
            InputManager.Instance.LateUpdate();
        }

        public override bool Dispose()
        {
            base.Dispose();

            SceneMgr.Instance.Dispose();
            MapManager.Instance.Dispose();
            MapTool.Instance.Dispose();
            UIManager.Instance.Dispose();
            RoleManager.Instance.Dispose();
            HUDManager.Instance.Dispose();
            InputManager.Instance.Dispose();
            EventDispatcher.Instance.Dispose();
            DebugManager.Instance.Dispose();
            return true;
        }
    }
}
