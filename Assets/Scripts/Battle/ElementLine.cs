using UnityEngine;
using UnityEngine.Rendering;

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
        //private Material _elementMat;

        public ElementLine(Vector3 startPos, Vector3 endPos, Color color, int segment = 6, float droop = 0.3f)
        {
            _go = new GameObject();
            _go.AddComponent<SortingGroup>().sortingOrder = 1;
            _lr = _go.AddComponent<LineRenderer>();
            _startPos = startPos;
            _endPos = endPos;
            _segment = segment;
            _droop = droop;

            _elementMatAssetID = AssetsMgr.Instance.LoadAssetAsync<Material>("Assets/Materials/PowerLineMat.mat", (mat) =>
            {
                //_elementMat = mat;
                _lr.positionCount = segment + 1;
                _lr.material = mat;
                _lr.textureMode = LineTextureMode.Tile;
                _lr.widthCurve = new AnimationCurve(new Keyframe[4] { new Keyframe(0, 0), new Keyframe(0.1f, 0.1F), new Keyframe(0.9F, 0.1f), new Keyframe(1, 0) });
                //_lr.startWidth = 0.1f;
                //_lr.endWidth = 0.1f;
                _lr.material.SetColor("_Color", color);
                //_lr.textureScale = new Vector2(8, 1);

                UpdateLine(1);
            });
        }

        private void UpdateLine(float lerp)
        {
            _lr.SetPosition(0, _startPos);
            for (int i = 1; i < _lr.positionCount - 1; i++)
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
                _lr.SetPosition(i, new Vector3(posX, posY, posZ));
            }

            _lr.SetPosition(_segment, _endPos);
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
