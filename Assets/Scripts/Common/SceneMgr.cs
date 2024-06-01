using UnityEngine;
using WarGame.UI;
using UnityEngine.AddressableAssets;

namespace WarGame
{
    public class SceneMgr : Singeton<SceneMgr>
    {
        private int _levelID;
        private BattleField _battleField = null;
        private GameObject _heroScene;
        private AsyncOperation _asyncOperation;
        private int _heroSceneID = 0;

        public override bool Init()
        {
            base.Init();

            return true;
        }

        public override void Update(float deltaTime)
        {
            if (null != _asyncOperation)
            {
                EventDispatcher.Instance.PostEvent(Enum.EventType.Scene_Load_Progress, new object[] { _asyncOperation.progress });
            }

            if (null != _battleField)
            {
                _battleField.Update(deltaTime);
            }
        }

        public override bool Dispose()
        {
            GameObject.Destroy(_heroScene);
            AssetMgr.Instance.ReleaseAsset(_heroSceneID);

            DestroyBattleFiled();

            base.Dispose();

            return true;
        }

        public void Touch(GameObject obj)
        {
            if (null == _battleField)
                return;
            _battleField.Touch(obj);
        }

        public void ClickBegin(GameObject obj)
        {
            if (null == _battleField)
                return;
            _battleField.ClickBegin(obj);
        }

        public void Click(GameObject obj)
        {
            if (null == _battleField)
                return;
            _battleField.Click(obj);
        }

        public void OpenBattleField(int levelID, bool restart)
        {
            _levelID = levelID;
            OpenScene("Assets/Scenes/MapScene.unity", restart);
        }

        //private void SceneLoaded(Scene scene, LoadSceneMode mode)
        //{
        //    _asyncOperation = null;
        //    UIManager.Instance.ClosePanel("LoadPanel");

        //    switch (scene.name)
        //    {
        //        case "MapScene":
        //            _battleField = new BattleField(_levelID);
        //            _battleField.Start();
        //            break;
        //        case "TransitionScene":
        //            UIManager.Instance.OpenPanel("Map", "MapPanel");
        //            break;
        //    }
        //}

        public void DestroyBattleFiled()
        {
            if (null == _battleField)
                return;

            _battleField.Dispose();
            _battleField = null;
            UIManager.Instance.ClosePanel("FightPanel");
            OpenScene("Assets/Scenes/TransitionScene.unity");
        }

        public void OpenHeroScene(params object[] args)
        {
            _heroSceneID = AssetMgr.Instance.LoadAssetAsync("Assets/Prefabs/HeroScene.prefab", (GameObject prefab) =>
            {
                _heroScene = GameObject.Instantiate<GameObject>(prefab);
                _heroScene.transform.position = Vector3.one * 10000;
                var heroCamera = _heroScene.transform.Find("Camera").GetComponent<Camera>();
                CameraMgr.Instance.SetMainCamera(heroCamera);
                UIManager.Instance.OpenPanel("Hero", "HeroPanel", new object[] { heroCamera.targetTexture });
                HUDManager.Instance.SetVisible(false);
            });
        }

        public void CloseHeroScene()
        {
            GameObject.Destroy(_heroScene);
            AssetMgr.Instance.ReleaseAsset(_heroSceneID);
            UIManager.Instance.ClosePanel("HeroPanel");
            HUDManager.Instance.SetVisible(true);
        }

        public Transform GetHeroRoot()
        {
            return _heroScene.transform.Find("HeroRoot");
        }

        public void OpenMapScene()
        {
            OpenScene("Assets/Scenes/TransitionScene.unity");
        }

        public void OpenScene(string scene, bool restart = false)
        {
            UIManager.Instance.OpenPanel("Load", "LoadPanel");

            AssetMgr.Instance.LoadSceneAsync(scene, (string name) =>
            {
                UIManager.Instance.ClosePanel("LoadPanel");
                switch (name)
                {
                    case "MapScene":
                        _battleField = new BattleField(_levelID, restart);
                        break;
                    case "TransitionScene":
                        UIManager.Instance.OpenPanel("Map", "MapPanel");
                        break;
                }
            });
        }
    }
}
