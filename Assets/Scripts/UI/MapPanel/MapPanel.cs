using FairyGUI;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace WarGame.UI
{
    public class MapPanel : UIBase
    {
        private List<string> _maps = new List<string>();
        private GList _gList = null;
        private string mapDir = null;

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
                    _maps.Add(files[i]);
                }
            }

            _gList.numItems = _maps.Count;

            ((GLoader)_gCom.GetChild("bg")).url = "UI/Background/MapBG";
        }

        private void ItemRenderer(int index, GObject item)
        {
            GComponent gCom = (GComponent)item;
            gCom.GetChild("title").text = _maps[index];
        }

        private void OnClickItem(EventContext context)
        {
            var index = _gList.GetChildIndex((GObject)context.data);
            mapDir = _maps[index];

            UIManager.Instance.ClosePanel(this.name);
            SceneManager.LoadSceneAsync("MapScene");
            SceneManager.sceneLoaded += SceneLoaded;
        }

        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= SceneLoaded;

            SceneMgr.Instance.Create(mapDir);
        }
    }
}
