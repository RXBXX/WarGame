using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class MapScroll : UIBase
    {
        public MapScroll(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        { 

        }

        public void ZoomIn()
        { }

        public void ZoomOut()
        { }

        public void SetIcon(string url)
        {
            ((GLoader)_gCom.GetChild("map")).url = url;
        }
    }
}
