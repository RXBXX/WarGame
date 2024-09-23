using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace WarGame
{
    public class ElementLine
    {
        private int _segment = 5;
        private Vector3 _startPos;
        private Vector3 _endPos;
        private LineRenderer _lr;
        private float _droop = 1F;
        private int _elementMatAssetID;
        private GameObject _go;
        private List<Vector3> _poss = new List<Vector3>();
        //private Material _elementMat;
        private bool _started = false;
        private int _step = 0;
        private float _interval = 0.03F;
        private float _time = 0;

        public ElementLine(Vector3 startPos, Vector3 endPos, Color color, int segment = 6, float droop = 0.3f)
        {
            _go = new GameObject();
            _go.AddComponent<SortingGroup>().sortingOrder = 1;
            _lr = _go.AddComponent<LineRenderer>();
            _startPos = startPos;
            _endPos = endPos;
            _segment = segment;
            _droop = droop;
            UpdateLine(1);

            //DebugManager.Instance.Log(_startPos +"_"+_endPos);
            _elementMatAssetID = AssetsMgr.Instance.LoadAssetAsync<Material>("Assets/Materials/PowerLineMat.mat", (mat) =>
            {
                _lr.positionCount = segment + 1;
                _lr.material = mat;
                _lr.textureMode = LineTextureMode.Tile;
                _lr.widthCurve = new AnimationCurve(new Keyframe[4] { new Keyframe(0, 0), new Keyframe(0.1f, 0.1F), new Keyframe(0.9F, 0.1f), new Keyframe(1, 0) });
                _lr.material.SetColor("_Color", color);

                _started = true;
                //UpdateLine(1);
            });
        }

        private void UpdateLine(float lerp)
        {
            _poss.Add(_startPos);
            //_lr.SetPosition(0, _startPos);
            for (int i = 1; i < _segment; i++)
            {
                float index = i;
                var tempDroop = (index - _segment / 2.0F) / (_segment / 2.0F);
                tempDroop *= tempDroop;
                tempDroop *= -_droop;
                tempDroop += _droop;
                var frontPart = (_segment - index) / _segment;
                var backPart = index / _segment;
                var posY = _startPos.y * frontPart + _endPos.y * backPart + tempDroop;
                var posX = _startPos.x * frontPart + _endPos.x * backPart;
                var posZ = _startPos.z * frontPart + _endPos.z * backPart;
                _poss.Add(new Vector3(posX, posY, posZ));
                //_lr.SetPosition(i, new Vector3(posX, posY, posZ));
            }
            _poss.Add(_endPos);
            //_lr.SetPosition(_segment, _endPos);
        }

        public void Update(float deltaTime)
        {
            //DebugManager.Instance.Log("Update");
            if (!_started)
                return;
            if (_step >= _poss.Count)
                return;

            _time += deltaTime;
            if (_time > _interval)
            {
                //DebugManager.Instance.Log(_step + "_" + _poss[_step]);
                for (int i = _step; i < _poss.Count; i++)
                    _lr.SetPosition(i, _poss[_step]);
                _time = 0;
                _step += 1;
            }
        }

        public void Dispose()
        {
            AssetsMgr.Instance.Destroy(_lr);
            //AssetsMgr.Instance.Destroy(_elementMat);
            AssetsMgr.Instance.ReleaseAsset(_elementMatAssetID);
            _elementMatAssetID = 0;
            AssetsMgr.Instance.Destroy<GameObject>(_go);
        }
    }
}
