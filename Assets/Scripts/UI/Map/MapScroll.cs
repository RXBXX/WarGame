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
        private MapSky _sky;
        private GButton _smithyBtn;
        private GButton _heroBtn;
        private float _maxScale = 1.2F;

        public MapScroll(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _smithyBtn = GetGObjectChild<GButton>("smithyBtn");
            _smithyBtn.title = ConfigMgr.Instance.GetTranslation("MapPanel_Smithy");
            _smithyBtn.onClick.Add(OnClickSmithy);
            _heroBtn = GetGObjectChild<GButton>("heroBtn");
            _heroBtn.title = ConfigMgr.Instance.GetTranslation("MapPanel_Camp");
            _heroBtn.onClick.Add(OnClickHero);
            _sky = GetChild<MapSky>("sky");
        }

        public void Init(string bg, List<int> levels)
        {
            SetIcon(bg);
            for (int i = 0; i < levels.Count; i++)
            {
                var ui = UIManager.Instance.CreateUI<MapMark>("Map", "MapMark");
                ui.Init(levels[i]);
                GCom.AddChildAt(ui.GCom, GCom.numChildren - 1);
                _levelsDic.Add(levels[i], ui);
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

            var newScale = Vector2.one * 1 / scale.x;
            foreach (var v in _levelsDic)
            {
                v.Value.SetScale(newScale);
            }

            _smithyBtn.scale = newScale;
            _heroBtn.scale = newScale;

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
            if (scale.x > _maxScale || scale.y > _maxScale)
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

            _sky.Update(timeDelta);
        }


        public void ScrollToLevel(int levelID)
        {
            GTween.Kill(_gCom);

            var config = ConfigMgr.Instance.GetConfig<LevelConfig>("LevelConfig", levelID);
            var pos = GRoot.inst.size / 2 - config.UIPos;
            if (IsMoveCrossBorder(ref pos, _gCom.scale))
                return;
            _tweener = _gCom.TweenMove(pos, 0.5F).OnComplete(()=> {
                GTween.Kill(_gCom);
            });
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
            GTween.Kill(_gCom);

            foreach (var v in _levelsDic)
            {
                v.Value.Dispose();
            }
            _levelsDic.Clear();
            base.Dispose(disposeGCom);
        }
    }
}
