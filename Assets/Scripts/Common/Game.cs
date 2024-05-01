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
            DatasMgr.Instance.Init();
            ConfigMgr.Instance.Init();
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

        public override void Update(float deltaTime)
        {
            DebugManager.Instance.Update(deltaTime);
            EventDispatcher.Instance.Update(deltaTime);
            UIManager.Instance.Update(deltaTime);
            SceneMgr.Instance.Update(deltaTime);
            HUDManager.Instance.Update(deltaTime);
            CameraMgr.Instance.Update(deltaTime);
            InputManager.Instance.Update(deltaTime);
            MapTool.Instance.Update(deltaTime);
            MapManager.Instance.Update(deltaTime);
            LineMgr.Instance.Update(deltaTime);
            RoleManager.Instance.Update(deltaTime);
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
            SceneMgr.Instance.Dispose();
            LineMgr.Instance.Dispose();
            HUDManager.Instance.Dispose();
            RoleManager.Instance.Dispose();
            UIManager.Instance.Dispose();
            ConfigMgr.Instance.Dispose();
            DatasMgr.Instance.Dispose();
            EventDispatcher.Instance.Dispose();
            DebugManager.Instance.Dispose();
            return true;
        }
    }
}
