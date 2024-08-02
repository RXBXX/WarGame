using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame;
using FairyGUI;

namespace WarGame.UI
{
    public class RecordPanel : UIBase
    {
        private Enum.RecordMode _mode;
        private GList _recordList;
        private List<string> _listDatas = new List<string>();
        private Dictionary<string, RecordItem> _recordItemDic = new Dictionary<string, RecordItem>();

        public RecordPanel(GComponent gCom, string customName, object[] args = null) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;

            var bg = GetGObjectChild<GLoader>("bg");
            bg.url = "UI/Background/CommonBG";
            bg.onClick.Add(() =>
            {
                UIManager.Instance.ClosePanel(name);
            });

            _recordList = (GList)_gCom.GetChild("recordList");
            _recordList.itemRenderer = ItemRenderer;
            _recordList.onClickItem.Add(OnClickItem);

            _mode = (Enum.RecordMode)args[0];

            UpdateRecords();

            _gCom.GetChild("closeBtn").onClick.Add(OnClickClose);

            EventDispatcher.Instance.AddListener(Enum.Event.DeleteRecordS2C, OnDeleteRecordS2C);
        }

        private void UpdateRecords()
        {
            _listDatas.Clear();
            if (_mode == Enum.RecordMode.Write)
            {
                _listDatas.Add("�´浵");
            }

            var gameDatas = DatasMgr.Instance.GetAllRecord();
            foreach (var v in gameDatas)
                _listDatas.Add(v);

            _recordList.numItems = _listDatas.Count;
        }

        private void ItemRenderer(int index, GObject item)
        {
            if (!_recordItemDic.ContainsKey(item.id))
                _recordItemDic.Add(item.id, UIManager.Instance.CreateUI<RecordItem>("RecordItem", item));

            if (_mode == Enum.RecordMode.Write && 0 == index)
            {
                _recordItemDic[item.id].Update(null);
            }
            else
            {
                var gd = DatasMgr.Instance.GetRecord(_listDatas[index]);
                _recordItemDic[item.id].Update(_listDatas[index]);
            }
        }

        private void OnClickItem(EventContext context)
        {
            UIManager.Instance.ClosePanel(name);

            var index = _recordList.GetChildIndex((GObject)context.data);

            if (_mode == Enum.RecordMode.Write)
            {
                if (0 == index)
                    DatasMgr.Instance.SaveGame();
                else
                    DatasMgr.Instance.SaveGame(_listDatas[index]);
            }
            else if (_mode == Enum.RecordMode.Read)
            {
                DatasMgr.Instance.StartGame(_listDatas[index]);
            }

            //DebugManager.Instance.Log(_mode);
            if (_mode == Enum.RecordMode.Read)
            {
                //DebugManager.Instance.Log("OpenScene:" + "StartGame");
                SceneMgr.Instance.StartGame();
            }
        }

        private void OnClickClose()
        {
            UIManager.Instance.ClosePanel(name);
        }

        private void OnDeleteRecordS2C(params object[] args)
        {
            UpdateRecords();
        }

        public override void Dispose(bool disposeGCom = false)
        {
            EventDispatcher.Instance.RemoveListener(Enum.Event.DeleteRecordS2C, OnDeleteRecordS2C);
            base.Dispose(disposeGCom);
        }
    }
}
