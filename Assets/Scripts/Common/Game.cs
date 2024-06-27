using DG.Tweening;
using WarGame.UI;
using UnityEngine;

namespace WarGame
{
    public class Game : Singeton<Game>
    {
        public override bool Init()
        {
            //DebugManager.Instance.Log("Game.Init");
            base.Init();

            AudioMgr.Instance.Init();
            TimeMgr.Instance.Init();
            DebugManager.Instance.Init();
            EventDispatcher.Instance.Init();
            DatasMgr.Instance.Init();
            ConfigMgr.Instance.Init();
            CoroutineMgr.Instance.Init();
            UIManager.Instance.Init();
            TipsMgr.Instance.Init();
            DialogMgr.Instance.Init();
            SceneMgr.Instance.Init();
            HUDManager.Instance.Init();
            CameraMgr.Instance.Init();
            InputManager.Instance.Init();
            MapTool.Instance.Init();
            MapManager.Instance.Init();
            LineMgr.Instance.Init();
            RoleManager.Instance.Init();
            AttributeMgr.Instance.Init();
            EventMgr.Instance.Init();

            DOTween.Init(true, true);
            DOTween.debugMode = true;

            return true;
        }

        public bool Start()
        {
            UIManager.Instance.OpenPanel("Login", "LoginPanel");

            AudioMgr.Instance.PlayMusic("Assets/Audios/loop521.wav");
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
            RenderMgr.Instance.Update(deltaTime);
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

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            RenderMgr.Instance.OnRenderImage(source, destination);
        }

        public override bool Dispose()
        {
            EventMgr.Instance.Dispose();
            AttributeMgr.Instance.Dispose();
            CameraMgr.Instance.Dispose();
            InputManager.Instance.Dispose();
            MapTool.Instance.Dispose();
            MapManager.Instance.Dispose();
            SceneMgr.Instance.Dispose();
            LineMgr.Instance.Dispose();
            HUDManager.Instance.Dispose();
            RoleManager.Instance.Dispose();
            DialogMgr.Instance.Dispose();
            TipsMgr.Instance.Dispose();
            UIManager.Instance.Dispose();
            CoroutineMgr.Instance.Dispose();
            ConfigMgr.Instance.Dispose();
            DatasMgr.Instance.Dispose();
            EventDispatcher.Instance.Dispose();
            DebugManager.Instance.Dispose();
            TimeMgr.Instance.Dispose();
            AudioMgr.Instance.Dispose();

            System.GC.Collect();
            return base.Dispose();
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}
