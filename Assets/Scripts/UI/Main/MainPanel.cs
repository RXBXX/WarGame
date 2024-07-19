using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class MainPanel : UIBase
    {
        private GList _optionList;
        private List<StringCallbackStruct> _tabDatas = new List<StringCallbackStruct>();

        public MainPanel(GComponent gCom, string customName, object[] args = null) : base(gCom, customName, args)
        {
            ((GLoader)_gCom.GetChild("bg")).url = "UI/Background/CommonBG";

            _optionList = (GList)_gCom.GetChild("recordList");
            _optionList.itemRenderer = ItemRenderer;
            _optionList.onClickItem.Add(OnClickItem);

            Init();
        }

        private void Init()
        {
            _tabDatas.Add(new StringCallbackStruct(ConfigMgr.Instance.GetTranslation("MainPanel_NewGame"), ()=> {
                UIManager.Instance.ClosePanel(name);

                DatasMgr.Instance.StartNewGame();
                SceneMgr.Instance.StartGame();
            }));
            if (DatasMgr.Instance.GetAllRecord().Count > 0)
            {
                _tabDatas.Add(new StringCallbackStruct(ConfigMgr.Instance.GetTranslation("MainPanel_ContinueGame"), () => {
                    UIManager.Instance.OpenPanel("Record", "RecordPanel", new object[] {Enum.RecordMode.Read });
                }));
            }
            _tabDatas.Add(new StringCallbackStruct(ConfigMgr.Instance.GetTranslation("MainPanel_Settings"), ()=> {
                UIManager.Instance.OpenPanel("Settings", "SettingsPanel");
            }));
            _tabDatas.Add(new StringCallbackStruct(ConfigMgr.Instance.GetTranslation("MainPanel_Exit"), ()=> {
                Game.Instance.Quit();
            }));

            _optionList.numItems = _tabDatas.Count;
        }

        private void ItemRenderer(int index, GObject item)
        {
            ((GButton)item).title = _tabDatas[index].title;
        }

        private void OnClickItem(EventContext context)
        {

            var index = _optionList.GetChildIndex((GObject)context.data);
            _tabDatas[index].callback();
        }
    }
}
