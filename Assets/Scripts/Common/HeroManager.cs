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
        private List<Vector3> _points = new List<Vector3>();
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

        public void Move(List<Vector3> points)
        {
            if (null != _tween)
            {
                _tween.Kill();
                _tween = null;
            }

            _moveIndex = points.Count - 1 ;
            _points = points;
            Next();
        }

        private void Next()
        {
            if (_moveIndex < 0)
                return;
            _tween = _hero.transform.DOMove(_points[_moveIndex], 0.5f);
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
