using DG.Tweening;
using UnityEngine;

namespace WarGame
{
    public class MapObject
    {
        protected GameObject _gameObject;

        protected int _layer = 0;

        public virtual void ChangeToArenaSpace(Vector3 pos, float duration)
        {
            _gameObject.transform.DOMove(pos, duration);
        }

        public virtual void ChangeToMapSpace()
        {
        }

        public void SetLayer(int layer)
        {
            SetLayerRecursion(_gameObject.transform, layer);
        }

        public void RecoverLayer()
        {
            SetLayerRecursion(_gameObject.transform, _layer);
        }

        private void SetLayerRecursion(Transform tran, int layer)
        {
            var childCount = tran.childCount;
            for (int i = 0; i < childCount; i++)
            {
                SetLayerRecursion(tran.GetChild(i), layer);
            }
            tran.gameObject.layer = layer;
        }
    }
}
