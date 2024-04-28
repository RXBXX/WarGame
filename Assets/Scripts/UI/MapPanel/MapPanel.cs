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
        private GList _gList = null;

        public MapPanel(GComponent gCom, string name, object[] args = null) : base(gCom, name, args)
        {
            _gList = (GList)_gCom.GetChild("mapList");
            _gList.itemRenderer = ItemRenderer;
            _gList.onClickItem.Add(OnClickItem);

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

            _gList.numItems = _maps.Count;

            ((GLoader)_gCom.GetChild("bg")).url = "UI/Background/MapBG";

            _gCom.GetChild("heroBtn").onClick.Add(OnClickHero);
            _gCom.GetChild("closeBtn").onClick.Add(OnClickClose);

            GetChild<MapScroll>("mapScroll").SetIcon("UI/Background/Map");
        }

        private void ItemRenderer(int index, GObject item)
        {
            GComponent gCom = (GComponent)item;
            gCom.GetChild("title").text = _maps[index].key;
        }

        private void OnClickItem(EventContext context)
        {
            var index = _gList.GetChildIndex((GObject)context.data);
            UIManager.Instance.ClosePanel(name);
            SceneMgr.Instance.OpenScene(_maps[index].value);
        }

        private void OnClickHero()
        {
            SceneMgr.Instance.OpenHeroScene();
        }

        private void OnClickClose()
        {
            UIManager.Instance.ClosePanel(name);
        }
    }
}
