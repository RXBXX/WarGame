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
        private GObject _lock;

        public MapMark(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _name = GetUIChild<GTextField>("title");
            _desc = GetUIChild<GTextField>("desc");
            _typeC = GetController("type");
            _lock = GetUIChild<GObject>("lock");
            _gCom.onClick.Add(OnClick);
        }

        public void Init(int levelID, bool isOpen, Enum.LevelType type, string name, string desc)
        {
            _levelType = type;
            _levelID = levelID;
            _name.text = name;
            _desc.text = desc;
            _typeC.SetSelectedIndex((int)type - 1);
            _lock.visible = !isOpen;
        }

        private void OnClick()
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.Map_Open_Event, new object[] { _levelID });
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
    }
}
