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

        public DialogPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _dialogBox = GetChild<DialogBox>("dialogBox");
            _dialogGroup = (int)args[0];
            _callback = (WGArgsCallback)args[1];

            var dialogGroupConfig = ConfigMgr.Instance.GetConfig<DialogGroupConfig>("DialogGroupConfig", _dialogGroup);
            _max = dialogGroupConfig.MaxIndex;
            _curIndex = 0;

            _gCom.onClick.Add(OnClick);
            GetGObjectChild<GButton>("autoBtn").onClick.Add(OnClickAuto);

            _blurID = RenderMgr.Instance.SetBlurBG(GetGObjectChild<GLoader>("BG"));
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
                DialogMgr.Instance.CloseDialog();
                if (null != _callback)
                    _callback();
                return;
            }

            var dialogConfig = ConfigMgr.Instance.GetConfig<DialogConfig>("DialogConfig", _dialogGroup * 1000 + _curIndex);
            _dialogBox.Play(dialogConfig.Context, dialogConfig.Role, OnDialogEnd);
        }

        private void OnClick(EventContext context)
        {
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

        private void OnDialogEnd(params object[] args)
        {
            if (_isAutoPlay)
                NextDialog();
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
