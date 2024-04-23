using DG.Tweening;
using WarGame.UI;

namespace WarGame
{
    public class Game : Singeton<Game>
    {
        public override bool Init()
        {
            DebugManager.Instance.Log("Game.Init");
            base.Init();

            DebugManager.Instance.Init();
            EventDispatcher.Instance.Init();
            UIManager.Instance.Init();
            SceneMgr.Instance.Init();
            HUDManager.Instance.Init();
            CameraMgr.Instance.Init();
            InputManager.Instance.Init();
            MapTool.Instance.Init();
            MapManager.Instance.Init();
            LineMgr.Instance.Init();
            RoleManager.Instance.Init();

            DOTween.Init(true, true);

            return true;
        }

        public bool Start()
        {
            DebugManager.Instance.Log("Game.Start");
            UIManager.Instance.OpenPanel("Main", "MainPanel");
            return true;
        }

        public override void Update()
        {
            DebugManager.Instance.Update();
            EventDispatcher.Instance.Update();
            UIManager.Instance.Update();
            SceneMgr.Instance.Update();
            HUDManager.Instance.Update();
            CameraMgr.Instance.Update();
            InputManager.Instance.Update();
            MapTool.Instance.Update();
            MapManager.Instance.Update();
            LineMgr.Instance.Update();
            RoleManager.Instance.Update();
        }

        public override void LateUpdate()
        {
            DebugManager.Instance.LateUpdate();
            EventDispatcher.Instance.LateUpdate();
            UIManager.Instance.LateUpdate();
            SceneMgr.Instance.LateUpdate();
            HUDManager.Instance.LateUpdate();
            CameraMgr.Instance.LateUpdate();
            InputManager.Instance.LateUpdate();
            MapTool.Instance.LateUpdate();
            MapManager.Instance.LateUpdate();
            LineMgr.Instance.LateUpdate();
            RoleManager.Instance.LateUpdate();
        }

        public override bool Dispose()
        {
            base.Dispose();

            CameraMgr.Instance.Dispose();
            InputManager.Instance.Dispose();
            MapTool.Instance.Dispose();
            MapManager.Instance.Dispose();
            LineMgr.Instance.Dispose();
            RoleManager.Instance.Dispose();
            HUDManager.Instance.Dispose();
            SceneMgr.Instance.Dispose();
            UIManager.Instance.Dispose();
            EventDispatcher.Instance.Dispose();
            DebugManager.Instance.Dispose();
            return true;
        }
    }
}
