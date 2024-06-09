using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class MainPanel : UIBase
    {
        private GList _optionList;
        private List<StringCallbackStruct> _optionDatas = new List<StringCallbackStruct>();

        public MainPanel(GComponent gCom, string customName, object[] args = null) : base(gCom, customName, args)
        {
            ((GLoader)_gCom.GetChild("bg")).url = "UI/Background/MainBG";

            _optionList = (GList)_gCom.GetChild("recordList");
            _optionList.itemRenderer = ItemRenderer;
            _optionList.onClickItem.Add(OnClickItem);

            Init();
        }

        private void Init()
        {
            _optionDatas.Add(new StringCallbackStruct("新游戏", ()=> {
                DatasMgr.Instance.StartNewGame();
                SceneMgr.Instance.StartGame();
            }));
            if (DatasMgr.Instance.GetAllRecord().Count > 0)
            {
                _optionDatas.Add(new StringCallbackStruct("继续游戏", () => {
                    UIManager.Instance.OpenPanel("Record", "RecordPanel", new object[] {Enum.RecordMode.Read });
                }));
            }
            _optionDatas.Add(new StringCallbackStruct("设置", ()=> {
                UIManager.Instance.OpenPanel("Settings", "SettingsPanel");
            }));
            _optionDatas.Add(new StringCallbackStruct("退出游戏",()=> {
                Game.Instance.Quit();
            }));

            _optionList.numItems = _optionDatas.Count;
        }

        private void ItemRenderer(int index, GObject item)
        {
            ((GButton)item).title = _optionDatas[index].title;
        }

        private void OnClickItem(EventContext context)
        {
            UIManager.Instance.ClosePanel(name);

            var index = _optionList.GetChildIndex((GObject)context.data);
            _optionDatas[index].callback();
        }
    }
}
