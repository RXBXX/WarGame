using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class SettingsPanel : UIBase
    {
        private GList _optionList;
        private List<StringCallbackStruct> _optionDatas = new List<StringCallbackStruct>();

        public SettingsPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;

            _optionList = (GList)_gCom.GetChild("recordList");
            _optionList.itemRenderer = ItemRenderer;
            //_optionList.onClickItem.Add(OnClickItem);

            _gCom.GetChild("closeBtn").onClick.Add(OnClickClose);
            Init();
        }

        private void Init()
        {
            _optionDatas.Add(new StringCallbackStruct("保存", ()=> {
                UIManager.Instance.OpenPanel("Record", "RecordPanel", new object[] { Enum.RecordMode.Write });
            }));

            if (SceneMgr.Instance.IsInBattleField())
            {
                _optionDatas.Add(new StringCallbackStruct("退出战场", () =>
                {
                    UIManager.Instance.ClosePanel(this.name);
                    SceneMgr.Instance.DestroyBattleFiled();
                }));
            }

            _optionDatas.Add(new StringCallbackStruct("退出游戏", () =>
            {
                UIManager.Instance.ClosePanel(this.name);
                Game.Instance.Quit();
            }));

            _optionDatas.Add(new StringCallbackStruct("取消", () =>
            {
                UIManager.Instance.ClosePanel(this.name);
            }));

            _optionList.numItems = _optionDatas.Count;
        }

        private void ItemRenderer(int index, GObject item)
        {
            ((GButton)item).title = _optionDatas[index].title;
        }

        private void OnClickItem(EventContext context)
        {
            var index = _optionList.GetChildIndex((GObject)context.data);
            _optionDatas[index].callback();
        }

        private void OnClickClose()
        {
            UIManager.Instance.ClosePanel(name);
        }
    }
}
