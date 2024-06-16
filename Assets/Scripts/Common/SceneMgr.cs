using UnityEngine;
using WarGame.UI;
using UnityEngine.AddressableAssets;

namespace WarGame
{
    public class SceneMgr : Singeton<SceneMgr>
    {
        private BattleField _battleField = null;
        private GameObject _heroScene;
        //private AsyncOperation _asyncOperation;
        private int _heroSceneID = 0;

        public BattleField BattleField
        {
            get { return _battleField; }
        }

        public override bool Init()
        {
            base.Init();

            return true;
        }

        public override void Update(float deltaTime)
        {
            //if (null != _asyncOperation)
            //{
            //    EventDispatcher.Instance.PostEvent(Enum.EventType.Scene_Load_Progress, new object[] { _asyncOperation.progress });
            //}

            if (null != _battleField)
            {
                _battleField.Update(deltaTime);
            }
        }

        public override bool Dispose()
        {
            GameObject.Destroy(_heroScene);
            AssetsMgr.Instance.ReleaseAsset(_heroSceneID);

            DestroyBattleFiled();

            base.Dispose();

            return true;
        }

        public bool IsInBattleField()
        {
            return null != _battleField;
        }

        public void FocusIn(GameObject obj)
        {
            if (null == _battleField)
                return;
            _battleField.FocusIn(obj);
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

        public void ClickEnd()
        {
            if (null == _battleField)
                return;
            _battleField.ClickEnd();
        }

        public void RightClickBegin(GameObject obj)
        {
            if (null == _battleField)
                return;
            _battleField.RightClickBegin(obj);
        }

        public void RightClickEnd()
        {
            if (null == _battleField)
                return;
            _battleField.RightClickEnd();
        }

        private void OpenScene(string scene, System.Action<string> callback)
        {
            //DebugManager.Instance.Log("OpenScene");

            System.GC.Collect();
            AssetsMgr.Instance.UnloadUnusedAssets();

            //UIManager.Instance.OpenPanel("Load", "LoadPanel");
            AssetsMgr.Instance.LoadSceneAsync(scene, (string name) =>
            {
                //DebugManager.Instance.Log("OpenScene 1111");
                UIManager.Instance.ClosePanel("LoadPanel");
                callback(name);
            });
        }

        public void StartGame(string id = null)
        {
            //DebugManager.Instance.Log("OpenScene:" + DatasMgr.Instance.IsNewGameData());
            if (DatasMgr.Instance.IsNewGameData())
            {
                StoryMgr.Instance.PlayStory(10001, true, (args) =>
                {
                    DatasMgr.Instance.SetGameDataDirty();
                    OpenScene("Assets/Scenes/TransitionScene.unity", (name)=> {
                        UIManager.Instance.OpenPanel("Map", "MapPanel");
                    });
                });
            }
            else
            {
                OpenScene("Assets/Scenes/TransitionScene.unity", (name) => {
                    UIManager.Instance.OpenPanel("Map", "MapPanel");
                });
            }
        }

        public void OpenBattleField(int levelID, bool restart)
        {
            OpenScene("Assets/Scenes/MapScene.unity", (name) => {
                _battleField = new BattleField(levelID, restart);
            });
        }

        public void DestroyBattleFiled()
        {
            if (null == _battleField)
                return;

            _battleField.Dispose();
            _battleField = null;

            StartGame();
        }

        public void OpenHeroScene(params object[] args)
        {
            _heroSceneID = AssetsMgr.Instance.LoadAssetAsync("Assets/Prefabs/HeroScene.prefab", (GameObject prefab) =>
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
            AssetsMgr.Instance.ReleaseAsset(_heroSceneID);
            UIManager.Instance.ClosePanel("HeroPanel");
            HUDManager.Instance.SetVisible(true);
        }

        public Transform GetHeroRoot()
        {
            return _heroScene.transform.Find("HeroRoot");
        }
    }
}
