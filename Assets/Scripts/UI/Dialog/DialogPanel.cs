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

        public DialogPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            UILayer = Enum.UILayer.PopLayer;
            _dialogBox = GetChild<DialogBox>("dialogBox");
            _dialogGroup = (int)args[0];
            var dialogGroupConfig = ConfigMgr.Instance.GetConfig<DialogGroupConfig>("DialogGroupConfig", _dialogGroup);
            _max = dialogGroupConfig.MaxIndex;
            _curIndex = 0;

            _gCom.onClick.Add(OnClick);
            GetUIChild<GButton>("autoBtn").onClick.Add(OnClickAuto);
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
                return;
            }

            var dialogConfig = ConfigMgr.Instance.GetConfig<DialogConfig>("DialogConfig", _dialogGroup * 1000 + _curIndex);
            var roleIcon = ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", dialogConfig.Role).DialogIcon;
            _dialogBox.Play(dialogConfig.Context, roleIcon, OnDialogEnd);
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
    }
}