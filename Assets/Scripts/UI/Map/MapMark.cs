using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class MapMark : UIBase
    {
        private int _levelID;
        private Controller _typeC;
        private GTextField _name;
        private GTextField _desc;
        private Enum.LevelType _levelType;
        //private GObject _lock;
        private Controller _showBtnC;
        //private GButton _goOnBtn;
        //private GButton _restartBtn;
        private Controller _isPass;

        public MapMark(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _name = GetGObjectChild<GTextField>("title");
            _desc = GetGObjectChild<GTextField>("desc");
            _typeC = GetController("type");
            //_lock = GetGObjectChild<GObject>("lock");
            _showBtnC = GetController("showBtn");
            _isPass = GetController("isPass");
            //_goOnBtn = GetGObjectChild<GButton>("goOnBtn");
            //_goOnBtn.onClick.Add(OnClickGoOn);
            //_restartBtn = GetGObjectChild<GButton>("restartBtn");
            //_restartBtn.onClick.Add(OnClickRestart);
            _gCom.onClick.Add(OnClick);
            Stage.inst.onTouchBegin.Add(OnTouchBegin);
        }

        public override void Dispose(bool disposeGCom = false)
        {
            Stage.inst.onTouchBegin.Remove(OnTouchBegin);
            base.Dispose(disposeGCom);
        }

        public void Init(int levelID)
        {
            _levelID = levelID;
            var config = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", levelID);
            _levelType = config.Type;
            _name.text = config.GetTranslation("Name");
            _desc.text = config.GetTranslation("Desc");
            _isPass.SetSelectedIndex(DatasMgr.Instance.IsLevelPass(_levelID) ? 1 : 0);
            SetPosition(config.UIPos);
            SetVisible(DatasMgr.Instance.IsLevelOpen(_levelID));
        }

        private void OnClick()
        {
            EventDispatcher.Instance.PostEvent(Enum.Event.Map_Open_Event, new object[] { _levelID, false });

            //_showBtnC.SetSelectedIndex(1);
        }

        public void SetGrayed(bool grayed)
        {
            _gCom.grayed = grayed;
        }

        public override void Update(float timeDelta)
        {
            var visible = _gCom.displayObject == Stage.inst.touchTarget;
            if (visible != _desc.visible)
                _desc.visible = visible;
        }

        public void OnChangeLOD(int lod)
        {
            if (_levelType == Enum.LevelType.Branch)
            {
                SetVisible(lod == 0);
            }
        }

        //private void OnClickGoOn(EventContext context)
        //{
        //    context.StopPropagation();

        //    _showBtnC.SetSelectedIndex(0);
        //    EventDispatcher.Instance.PostEvent(Enum.Event.Map_Open_Event, new object[] { _levelID, false});
        //}

        //private void OnClickRestart(EventContext context)
        //{
        //    context.StopPropagation();

        //    _showBtnC.SetSelectedIndex(0);
        //    EventDispatcher.Instance.PostEvent(Enum.Event.Map_Open_Event, new object[] { _levelID, true});
        //}

        private void OnTouchBegin()
        {
            bool isTouch = false;
            var touchTarget = Stage.inst.touchTarget;
            while (null != touchTarget)
            {
                if (touchTarget == GCom.displayObject)
                {
                    isTouch = true;
                    break;
                }
                touchTarget = touchTarget.parent;
            }

            if (!isTouch)
                _showBtnC.SetSelectedIndex(0);
        }

        public void Active()
        {
            SetVisible(true);
            //_gCom.grayed = false;
            //_lock.visible = false;
        }
    }
}
