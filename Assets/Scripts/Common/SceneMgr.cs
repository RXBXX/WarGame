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
        private string _curMap = null;
        private BattleField _battleField = null;
        private GameObject _heroScene;

        public override bool Init()
        {
            base.Init();

            SceneManager.sceneLoaded += SceneLoaded;

            return true;
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

        public void OpenBattleField(string mapDir)
        {
            _curMap = mapDir;
            SceneManager.LoadSceneAsync("MapScene");
        }

        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            switch (scene.name)
            {
                case "MapScene":
                    _battleField = new BattleField(_curMap);
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
            SceneManager.LoadSceneAsync("TransitionScene");
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
    }
}
