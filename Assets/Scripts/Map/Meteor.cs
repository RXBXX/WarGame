using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class Meteor : Ornament
    {
        private Vector3 _dropPos;
        private Vector3 _targetPos;
        private float _dropDuration = 100.0f;
        private float _dropTime = 0;
        private TrailRenderer _trail;

        public Meteor(int id, int configID, int hexagonID, float scale, WGVector3 rotation) : base(id, configID, hexagonID, scale, rotation)
        {
        }

        protected override void OnCreate(GameObject prefab)
        {
            base.OnCreate(prefab);
            _trail = _gameObject.GetComponent<TrailRenderer>();
            _trail.startWidth = 1;
            _trail.startWidth = _scale;

            _dropTime = _dropDuration;

            if (Application.isPlaying)
                InitDrop();
        }

        private void InitDrop()
        {
            _targetPos = MapManager.Instance.GetHexagon(_hexagonID).GetPosition();
            _dropPos = _targetPos + new Vector3(5000, 2000, 5000);

            _dropTime = Random.Range(0, _dropDuration);
            _gameObject.transform.position = Vector3.Lerp(_dropPos, _targetPos, _dropTime / _dropDuration);
            _trail.Clear();
        }

        private void ResetDrop()
        {
            _dropTime = 0;
            _gameObject.transform.position = _dropPos;
            _trail.Clear();
        }

        public override void Update(float deltatime)
        {
            if (!IsCreated())
                return;

            _dropTime += deltatime;

            if (_dropTime > _dropDuration)
            {
                CameraMgr.Instance.ShakePosition();
                ResetDrop();
            }

            _gameObject.transform.position = Vector3.Lerp(_dropPos, _targetPos, _dropTime / _dropDuration);
        }
    }
}
