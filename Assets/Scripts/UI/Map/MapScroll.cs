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

        public void Zoom(Vector2 mousePos, float zoomDelta)
        {
            mousePos.y = Screen.height - mousePos.y;

            var uiPosX = mousePos.x / Screen.width * GRoot.inst.width;
            var uiPosY = mousePos.y / Screen.height * GRoot.inst.height;

            var centerX = (uiPosX - _gCom.x) / _gCom.scaleX;
            var centerY = (uiPosY - _gCom.y) / _gCom.scaleY;

            var scale = _gCom.scale + Vector2.one * zoomDelta;
            centerX *= scale.x;
            centerY *= scale.y;
            var pos = new Vector2(uiPosX - centerX, uiPosY - centerY);
            if (IsZoomCrossBorder(ref pos, ref scale))
                return;

            _gCom.scale = scale;
            _gCom.xy = pos;
        }

        public void Move(Vector2 pos)
        {
            pos.x = -pos.x;
            pos = _gCom.xy - pos;
            if (IsMoveCrossBorder(ref pos, _gCom.scale))
                return;

            _gCom.xy = pos;
        }

        private bool IsZoomCrossBorder(ref Vector2 pos, ref Vector2 scale)
        {
            if (scale.x > 2 || scale.y > 2)
                return true;
            else if (_gCom.width * scale.x < GRoot.inst.width || _gCom.height * scale.y < GRoot.inst.height)
            {
                var scaleX = GRoot.inst.width / _gCom.width;
                var scaleY = GRoot.inst.height / _gCom.height;
                var minScale = Mathf.Max(scaleX, scaleY);
                scale = new Vector2(minScale, minScale);
            }
            if (pos.x > 0)
                pos.x = 0;
            else if (pos.x < -_gCom.width * scale.x + GRoot.inst.width)
                pos.x = -_gCom.width * scale.x + GRoot.inst.width;
            if (pos.y > 0)
                pos.y = 0;
            else if (pos.y < -_gCom.height * scale.y + GRoot.inst.height)
                pos.y = -_gCom.height * scale.y + GRoot.inst.height;

            return false;
        }

        private bool IsMoveCrossBorder(ref Vector2 pos, Vector2 scale)
        {
            if (pos.x > 0)
                pos.x = 0;
            else if (pos.x < -_gCom.width * scale.x + GRoot.inst.width)
                pos.x = -_gCom.width * scale.x + GRoot.inst.width;
            if (pos.y > 0)
                pos.y = 0;
            else if (pos.y < -_gCom.height * scale.y + GRoot.inst.height)
                pos.y = -_gCom.height * scale.y + GRoot.inst.height;
            return false;
        }

        public void SetIcon(string url)
        {
            ((GLoader)_gCom.GetChild("map")).url = url;
        }

        public bool IsHit(DisplayObject target)
        {
            while (null != target)
            {
                if (target == _gCom.displayObject)
                    return true;
                target = target.parent;
            }
            return false;
        }

        public void SetLevel(int curLevel)
        {
            for (int i = 0; i <= 8; i++)
            {
                var mark = GetChild<MapMark>("level_" + i);
                mark.SetLevel(i);
                mark.SetGrayed(curLevel < i);
            }
        }
    }
}
