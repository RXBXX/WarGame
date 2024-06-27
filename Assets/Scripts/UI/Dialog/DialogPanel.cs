using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class DialogPanel : UIBase
    {
        private int _dialogGroup = 0;
        private int _max = 0;
        private int _curIndex = 0;
        private DialogBox _dialogBox;
        private bool _isAutoPlay = false;
        private WGArgsCallback _callback;
        private int _blurID;
        private GList _optionList;
        private List<int> _optionsData;

        public DialogPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;

            _dialogBox = GetChild<DialogBox>("dialogBox");
            _dialogGroup = (int)args[0];
            _callback = (WGArgsCallback)args[1];

            var dialogGroupConfig = ConfigMgr.Instance.GetConfig<DialogGroupConfig>("DialogGroupConfig", _dialogGroup);
            _max = dialogGroupConfig.MaxIndex;
            _curIndex = 0;

            _gCom.onClick.Add(OnClick);
            GetGObjectChild<GButton>("autoBtn").onClick.Add(OnClickAuto);

            //_blurID = RenderMgr.Instance.SetBlurBG(GetGObjectChild<GLoader>("BG"));
            _optionList = GetGObjectChild<GList>("optionList");
            _optionList.itemRenderer = OnOptionRenderer;
            _optionList.onClickItem.Add(OnOptionClick);

            NextDialog();
        }

        public override void Update(float deltaTime)
        {
            _dialogBox.Update(deltaTime);
        }

        private void NextDialog()
        {
            _curIndex += 1;
            if (_curIndex > _max)
            {
                var groupConfig = ConfigMgr.Instance.GetConfig<DialogGroupConfig>("DialogGroupConfig", _dialogGroup);
                if (null != groupConfig.Options)
                {
                    _optionsData = groupConfig.Options;
                    _optionList.visible = true;
                    _optionList.numItems = _optionsData.Count;
                }
                else
                {
                    OnDialogGroupEnd();
                }
            }
            else
            {
                var dialogConfig = ConfigMgr.Instance.GetConfig<DialogConfig>("DialogConfig", _dialogGroup * 1000 + _curIndex);
                _dialogBox.Play(dialogConfig.Context, dialogConfig.Role, OnDialogEnd);
            }
        }

        private void OnClick(EventContext context)
        {
            if (null != _optionsData && _optionsData.Count > 0)
                return;

            if (_isAutoPlay)
                return;

            if (_dialogBox.IsPlaying())
            {
                _dialogBox.Complete();
            }
            else
            {
                NextDialog();
            }
        }

        private void OnClickAuto(EventContext context)
        {
            context.StopPropagation();

            _isAutoPlay = !_isAutoPlay;
            if (_isAutoPlay && !_dialogBox.IsPlaying())
            {
                NextDialog();
            }
        }

        //当前文本结束
        private void OnDialogEnd(params object[] args)
        {
            if (_isAutoPlay)
                CoroutineMgr.Instance.StartCoroutine(OnDialogEndIEnumerator());
        }

        //每段文本之间的间隔
        private IEnumerator OnDialogEndIEnumerator()
        {
            yield return new WaitForSeconds(0.5f);
            NextDialog();
        }

        private void OnOptionRenderer(int index, GObject item)
        {
            ((GButton)item).title = ConfigMgr.Instance.GetConfig<DialogOptionConfig>("DialogOptionConfig", _optionsData[index]).Title;
        }

        private void OnOptionClick(EventContext context)
        {
            var index = _optionList.GetChildIndex((GObject)context.data);
            OnDialogGroupEnd(ConfigMgr.Instance.GetConfig<DialogOptionConfig>("DialogOptionConfig", _optionsData[index]).Event);
        }

        private void OnDialogGroupEnd(int nextEvent = 0)
        {
            DialogMgr.Instance.CloseDialog();

            if (0 == nextEvent)
            {
                if (null != _callback)
                    _callback();
            }
            else
            {
                EventMgr.Instance.TriggerEvent(nextEvent, _callback);
            }
        }

        public override void Dispose(bool disposeGCom = false)
        {
            if (0 != _blurID)
            {
                RenderMgr.Instance.ReleaseBlurBG(_blurID);
                _blurID = 0;
            }
            base.Dispose(disposeGCom);
        }
    }
}
