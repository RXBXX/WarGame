using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace WarGame
{
    public class HeroManager : Singeton<HeroManager>
    {
        private GameObject _hero;
        private int _moveIndex = 0;
        private List<string> _paths = new List<string>();
        private Tween _tween;

        public override bool Init()
        {
            base.Init();
            return true;
        }

        public void SetHero(GameObject obj)
        {
            _hero = obj;
        }

        public void Move(List<string> paths)
        {
            if (null != _tween)
            {
                _tween.Kill();
                _tween = null;
            }

            _moveIndex = paths.Count - 1 ;
            _paths = paths;
            Next();
        }

        private void Next()
        {
            if (_moveIndex < 0)
                return;
            var key = _paths[_moveIndex];
            if (null == key)
                return;
            if (!MapManager.Instance.ContainHexagon(key))
                return;
            var hexagon = MapManager.Instance.GetHexagon(key);
            if (null == hexagon)
                return;
            _tween = _hero.transform.DOMove(MapTool.Instance.FromCellPosToWorldPos(hexagon.position + new Vector3(0, 0.53F, 0)), 0.5f);
            _tween.onComplete = () =>
            {
                _tween.Kill();
                _tween = null;

                Next();
            };
            _moveIndex--;
        }

        public override bool Dispose()
        {
            base.Dispose();
            return true;
        }
    }
}
