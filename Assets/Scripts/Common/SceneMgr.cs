using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace WarGame
{
    public class SceneMgr : Singeton<SceneMgr>
    {
        private int _levelID;
        private BattleField _battleField = null;
        private GameObject _heroScene;
        private AsyncOperation _asyncOperation;

        public override bool Init()
        {
            base.Init();

            SceneManager.sceneLoaded += SceneLoaded;

            return true;
        }

        public override void Update(float deltaTime)
        {
            if (null != _asyncOperation)
            {
                EventDispatcher.Instance.PostEvent(Enum.EventType.Scene_Load_Progress, new object[] { _asyncOperation.progress });
            }
        }

        public override bool Dispose()
        {
            base.Dispose();

            SceneManager.sceneLoaded -= SceneLoaded;
            DestroyBattleFiled();

            return true;
        }

        public void Touch(GameObject obj)
        {
            if (null == _battleField)
                return;
            _battleField.Touch(obj);
        }

        public void Click(GameObject obj)
        {
            if (null == _battleField)
                return;
            _battleField.Click(obj);
        }

        public void OpenBattleField(int levelID)
        {
            _levelID = levelID;
            OpenScene("MapScene");
        }

        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _asyncOperation = null;
            UIManager.Instance.ClosePanel("LoadPanel");

            switch (scene.name)
            {
                case "MapScene":
                    _battleField = new BattleField(_levelID);
                    _battleField.Start();
                    break;
                case "TransitionScene":
                    UIManager.Instance.OpenPanel("Map", "MapPanel");
                    break;
            }
        }

        public void DestroyBattleFiled()
        {
            if (null == _battleField)
                return;

            _battleField.Dispose();
            _battleField = null;
            UIManager.Instance.ClosePanel("FightPanel");
            OpenScene("TransitionScene");
        }

        public void OpenHeroScene(params object[] args)
        {
            var prefab = AssetMgr.Instance.LoadAsset<GameObject>("Assets/Prefabs/HeroScene.prefab");
            _heroScene = GameObject.Instantiate<GameObject>(prefab);
            _heroScene.transform.position = Vector3.one * 10000;
            CameraMgr.Instance.SetMainCamera(_heroScene.transform.Find("Camera").GetComponent<Camera>());
            UIManager.Instance.OpenPanel("Hero", "HeroPanel", args);
            HUDManager.Instance.SetVisible(false);
        }

        public void CloseHeroScene()
        {
            GameObject.Destroy(_heroScene);
            UIManager.Instance.ClosePanel("HeroPanel");
            HUDManager.Instance.SetVisible(true);
        }

        public Transform GetHeroRoot()
        {
            return _heroScene.transform.Find("HeroRoot");
        }

        public void OpenMapScene()
        {
            SceneManager.LoadScene("TransitionScene");
        }

        public void OpenScene(string scene)
        {
            _asyncOperation = SceneManager.LoadSceneAsync(scene);
            UIManager.Instance.OpenPanel("Load", "LoadPanel");
        }
    }
}
