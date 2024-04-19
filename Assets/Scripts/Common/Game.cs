using DG.Tweening;
using WarGame.UI;

namespace WarGame
{
    public class Game : Singeton<Game>
    {
        public override bool Init()
        {
            base.Init();

            MapManager.Instance.Init();
            MapTool.Instance.Init();
            UIManager.Instance.Init();
            InputManager.Instance.Init();
            DebugManager.Instance.Init();

            DOTween.Init(true, true);

            return true;
        }

        public bool Start()
        {
            UIManager.Instance.OpenPanel("Main", "MainPanel");
            return true;
        }

        public void Update()
        {
            InputManager.Instance.Update();
        }

        public void LateUpdate()
        {
            InputManager.Instance.LateUpdate();
        }

        public override bool Dispose()
        {
            base.Dispose();

            MapManager.Instance.Dispose();
            MapTool.Instance.Dispose();
            UIManager.Instance.Dispose();
            InputManager.Instance.Dispose();
            DebugManager.Instance.Dispose();
            return true;
        }
    }
}
