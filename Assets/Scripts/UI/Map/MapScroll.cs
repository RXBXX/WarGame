using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class MapScroll : UIBase
    {
        private int _lod = 0;
        private Dictionary<int, MapMark> _levelsDic = new Dictionary<int, MapMark>();
        private GTweener _tweener = null;

        public MapScroll(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            GetGObjectChild<GButton>("smithyBtn").onClick.Add(OnClickSmithy);
            GetGObjectChild<GButton>("heroBtn").onClick.Add(OnClickHero);
        }

        public void Init(string bg, List<MapLevelPair> levels)
        {
            SetIcon(bg);
            for (int i = 0; i < levels.Count; i++)
            {
                var config = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", levels[i].configId);
                var ui = UIManager.Instance.CreateUI<MapMark>("Map", "MapMark");
                ui.Init(levels[i].configId, levels[i].open, config.Type, config.Name, config.Desc);
                ui.SetParent(_gCom);
                ui.SetPosition(config.UIPos);
                _levelsDic.Add(levels[i].configId, ui);
            }
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

            if (scale.x >= 1)
            {
                if (0 != _lod)
                    OnChangeLOD(0);
            }
            else
            {
                if (1 != _lod)
                    OnChangeLOD(1);
            }

            foreach (var v in _levelsDic)
            {
                v.Value.SetScale(Vector2.one * 1 / scale.x);
            }

            _gCom.scale = scale;
            _gCom.xy = pos;
        }

        public void Move(Vector2 deltaPos)
        {
            deltaPos.x = -deltaPos.x;
            deltaPos = _gCom.xy - deltaPos;
            if (IsMoveCrossBorder(ref deltaPos, _gCom.scale))
                return;

            _gCom.xy = deltaPos;
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

        private void SetIcon(string url)
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

        private void OnChangeLOD(int lod)
        {
            _lod = lod;
            foreach (var v in _levelsDic)
            {
                v.Value.OnChangeLOD(lod);
            }
        }

        public override void Update(float timeDelta)
        {
            foreach (var v in _levelsDic)
            {
                v.Value.Update(timeDelta);
            }
        }


        public void ScrollToLevel(int levelID)
        {
            if (null != _tweener)
            {
                _tweener.Kill();
                _tweener = null;
            }

            var config = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", levelID);
            var pos = GRoot.inst.size / 2 - config.UIPos;
            if (IsMoveCrossBorder(ref pos, _gCom.scale))
                return;
            _tweener = _gCom.TweenMove(pos, 0.5F);
        }

        public void OpenLevel(int level)
        {
            _levelsDic[level].Active();
        }


        private void OnClickHero()
        {
            SceneMgr.Instance.OpenHeroScene();
        }

        private void OnClickSmithy()
        {
            UIManager.Instance.OpenPanel("Smithy", "SmithyPanel");
        }


        public override void Dispose(bool disposeGCom = false)
        {
            if (null == _tweener)
            {
                _tweener.Kill();
                _tweener = null;
            }
            foreach (var v in _levelsDic)
            {
                v.Value.Dispose();
            }
            _levelsDic.Clear();
            base.Dispose(disposeGCom);
        }
    }
}
