using FairyGUI;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEditor;
using System.Text.RegularExpressions;

namespace WarGame.UI
{
    public class MapPanel : UIBase
    {
        private struct Pair
        {
            public string key;
            public string value;

            public Pair(string key, string value)
            {
                this.key = key;
                this.value = value;
            }
        }

        private List<Pair> _maps = new List<Pair>();
        private MapScroll _map;
        private Vector2 _lastMousePos;

        public MapPanel(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        {
            string dir = Application.dataPath + "/Maps";

            var files = Directory.GetFiles(dir);
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].Contains(".json") && !files[i].Contains(".json.meta"))
                {
                    var path = files[i].Replace('\\', '/');
                    var regex = new Regex(@"[^\\/]+(?=\.[^\.\\/]+$)");
                    var m = regex.Match(path);
                    if (m.Success)
                    {
                        _maps.Add(new Pair(m.Value, files[i]));
                    }
                }
            }

            ((GLoader)_gCom.GetChild("bg")).url = "UI/Background/MapBG";

            _gCom.GetChild("heroBtn").onClick.Add(OnClickHero);
            _gCom.GetChild("closeBtn").onClick.Add(OnClickClose);

            _map = GetChild<MapScroll>("mapScroll");
            _map.SetIcon("UI/Background/Map");

            EventDispatcher.Instance.AddListener(Enum.EventType.Map_Open_Event, OnMapOpen);
        }

        //private void ItemRenderer(int index, GObject item)
        //{
        //    GComponent gCom = (GComponent)item;
        //    gCom.GetChild("title").text = _maps[index].key;
        //}

        //private void OnClickItem(EventContext context)
        //{
        //    var index = _gList.GetChildIndex((GObject)context.data);
        //    UIManager.Instance.ClosePanel(name);
        //    SceneMgr.Instance.OpenScene(_maps[index].value);
        //}

        private void OnClickHero()
        {
            SceneMgr.Instance.OpenHeroScene();
        }

        private void OnClickClose()
        {
            UIManager.Instance.ClosePanel(name);
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
        }

        private void OnMapOpen(object[] args)
        {
            var level = (int)args[0];

            SceneMgr.Instance.OpenBattleField("E:/WarGame/Assets/Maps/Map_1.json");
        }

        public override void Dispose(bool disposeGCom = false)
        {
            base.Dispose(disposeGCom);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Map_Open_Event, OnMapOpen);
        }
    }
}
