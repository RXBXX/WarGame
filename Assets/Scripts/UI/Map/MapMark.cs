using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class MapMark : UIBase
    {
        private int _level;
        public MapMark(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _gCom.onClick.Add(OnClick);
        }

        public void SetLevel(int level)
        {
            this._level = level;
        }

        private void OnClick()
        {
            EventDispatcher.Instance.PostEvent(Enum.EventType.Map_Open_Event, new object[]{ this._level });
        }

        public void SetGrayed(bool grayed)
        {
            _gCom.grayed = grayed;
        }
    }
}
