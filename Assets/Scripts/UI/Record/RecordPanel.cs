using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame;
using FairyGUI;

namespace WarGame.UI
{
    public class RecordPanel : UIBase
    {
        private GList _recordList;
        private List<SampleGameData> _listDatas = new List<SampleGameData>();

        public RecordPanel(GComponent gCom, string customName, object[] args = null) : base(gCom, customName, args)
        {
            ((GLoader)_gCom.GetChild("bg")).url = "UI/Background/MainBG";

            _recordList = (GList)_gCom.GetChild("recordList");
            _recordList.itemRenderer = ItemRenderer;    
            _recordList.onClickItem.Add(OnClickItem);

            _listDatas.Add(new SampleGameData("ÐÂÓÎÏ·", Time.time));
  
            var gameDatas = DatasMgr.Instance.GetGameDatas();
            foreach (var v in gameDatas)
                _listDatas.Add(v);

            _recordList.numItems = _listDatas.Count;
        }

        private void ItemRenderer(int index, GObject item)
        {
            ((GButton)item).title = _listDatas[index].title;
        }

        private void OnClickItem(EventContext context)
        {
            var index = _recordList.GetChildIndex((GObject)context.data);
            if (0 == index)
                DatasMgr.Instance.NewGameData("´æµµ_" + Time.time);
            else
                DatasMgr.Instance.ReadGameData(_listDatas[index].title);
            SceneMgr.Instance.OpenMapScene();
        }

    }
}
