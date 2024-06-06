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

            _gCom.GetChild("heroBtn").onClick.Add(OnClickHero);
            _gCom.GetChild("closeBtn").onClick.Add(OnClickClose);

            _map = GetChild<MapScroll>("mapScroll");

            EventDispatcher.Instance.AddListener(Enum.EventType.Map_Open_Event, OnMapOpen);

            InitMap();
        }

        private void InitMap()
        {
            List<MapLevelPair> levels = new List<MapLevelPair>();
            ConfigMgr.Instance.ForeachConfig<LevelConfig>("LevelConfig", (config) =>
            {
                levels.Add(new MapLevelPair(config.ID, DatasMgr.Instance.IsLevelOpen(config.ID)));
            });
            _map.Init("UI/Background/Map", levels);
        }

        private void OnClickHero()
        {
            SceneMgr.Instance.OpenHeroScene();
        }

        private void OnClickClose()
        {
            UIManager.Instance.ClosePanel(name);
            UIManager.Instance.OpenPanel("Record", "RecordPanel");
        }

        public override void Update(float deltaTime)
        {
            var scroll = InputManager.Instance.GetAxis("Mouse ScrollWheel");
            if (0 != scroll && _map.IsHit(Stage.inst.touchTarget))
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
            var levelID = (int)args[0];
            if (!DatasMgr.Instance.IsLevelOpen(levelID))
            {
                TipsMgr.Instance.Add("关卡没有开启！");
                return;
            }
            SceneMgr.Instance.OpenBattleField(levelID, (bool)args[1]);
        }

        public override void Dispose(bool disposeGCom = false)
        {
            _map.Dispose();
            base.Dispose(disposeGCom);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Map_Open_Event, OnMapOpen);
        }
    }
}
