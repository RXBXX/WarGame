using FairyGUI;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEditor;
using System.Text.RegularExpressions;

namespace WarGame.UI
{
    public struct MapLevelPair
    {
        public int configId;
        public bool open;

        public MapLevelPair(int configId, bool pass)
        {
            this.configId = configId;
            this.open = pass;
        }
    }

    public class MapPanel : UIBase
    {
        private MapScroll _map;
        private Vector2 _lastMousePos;

        public MapPanel(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        {
            ((GLoader)_gCom.GetChild("bg")).url = "UI/Background/MapBG";

            //_gCom.GetChild("heroBtn").onClick.Add(OnClickHero);
            _gCom.GetChild("closeBtn").onClick.Add(OnClickClose);

            _map = GetChild<MapScroll>("mapScroll");

            EventDispatcher.Instance.AddListener(Enum.Event.Map_Open_Event, OnMapOpen);
            EventDispatcher.Instance.AddListener(Enum.Event.ActiveLevelS2C, OnActiveLevelS2C);

            InitMap();
        }

        private void InitMap()
        {
            List<MapLevelPair> levels = new List<MapLevelPair>();
            ConfigMgr.Instance.ForeachConfig<LevelConfig>("LevelConfig", (config) =>
            {
                var isOpen = DatasMgr.Instance.IsLevelOpen(config.ID);
                if (config.Type == Enum.LevelType.Main || isOpen)
                {
                    levels.Add(new MapLevelPair(config.ID, isOpen));
                }
            });
            _map.Init("UI/Background/Map", levels);
        }

        public override void OnEnable()
        {
            var lastMainLevel = 0;
            foreach (var v in DatasMgr.Instance.GetOpenedLevels())
            {
                var config = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", v);
                if (config.Type == Enum.LevelType.Main && config.ID > lastMainLevel)
                {
                    lastMainLevel = config.ID;
                }
            }

            if (0 != lastMainLevel)
                _map.ScrollToLevel(lastMainLevel);

            var homeEvent = DatasMgr.Instance.GetHomeEvent();
            if (0 != homeEvent)
            {
                EventMgr.Instance.TriggerEvent(homeEvent);
                DatasMgr.Instance.SetHomeEvent(0);
            }
        }

        //private void OnClickHero()
        //{
        //    SceneMgr.Instance.OpenHeroScene();
        //}

        private void OnClickClose()
        {
            UIManager.Instance.ClosePanel(name);
            UIManager.Instance.OpenPanel("Main", "MainPanel");
        }

        public override void Update(float deltaTime)
        {
            if (!_map.IsHit(Stage.inst.touchTarget))
                return;

            var scroll = InputManager.Instance.GetAxis("Mouse ScrollWheel");
            if (0 != scroll)
            {
                _map.Zoom(InputManager.Instance.GetMousePos(), scroll);
            }

            if (InputManager.Instance.GetMouseButtonDown(0))
            {
                _lastMousePos = InputManager.Instance.GetMousePos();
            }
            else if (InputManager.Instance.GetMouseButton(0))
            {
                var mousePos = InputManager.Instance.GetMousePos();
                _map.Move(mousePos - _lastMousePos);
                _lastMousePos = mousePos;
            }
            _map.Update(deltaTime);
        }

        private void OnMapOpen(object[] args)
        {
            UIManager.Instance.OpenPanel("Fight", "FightTipsPanel", args);
            //var levelID = (int)args[0];
            ////if (!DatasMgr.Instance.IsLevelOpen(levelID))
            ////{
            ////    TipsMgr.Instance.Add("关卡没有开启！");
            ////    return;
            ////}

            //WGCallback cb = () => {
            //    UIManager.Instance.ClosePanel(name);
            //    SceneMgr.Instance.OpenBattleField(levelID, (bool)args[1]);
            //};
            //UIManager.Instance.OpenPanel("Common", "CommonTipsPanel", new object[] {
            //"请确保进入前，已做好万全准备，一旦进入将无法再为英雄进行升级等操作，直至赢得胜利。",
            //cb
            //});

        }

        private void OnActiveLevelS2C(params object[] args)
        {
            var level = (int)args[0];
            _map.OpenLevel(level);
            _map.ScrollToLevel(level);
        }


        public override void Dispose(bool disposeGCom = false)
        {
            _map.Dispose();
            base.Dispose(disposeGCom);
            EventDispatcher.Instance.RemoveListener(Enum.Event.Map_Open_Event, OnMapOpen);
            EventDispatcher.Instance.RemoveListener(Enum.Event.ActiveLevelS2C, OnActiveLevelS2C);
        }
    }
}
