using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class GPUInstanced
    {
        private Mesh _mesh;
        private Material _mat;
        private int count;
        private Matrix4x4[] _matrixs = new Matrix4x4[1024];
        private List<Vector3> _offsets = new List<Vector3>();
        private Dictionary<string, List<float>> _floatArray = new Dictionary<string, List<float>>();
        private MaterialPropertyBlock _block = new MaterialPropertyBlock();

        public GPUInstanced(Mesh mesh, Material mat)
        {
            this._mesh = mesh;
            this._mat = mat;

            //_floatArray.Add("_TexIndex", new List<float>());
            //for (int i = 0; i < 32; i++)
            //    _floatArray["_TexIndex"].Add(i%2);

            //_block.SetFloatArray("_TexIndex", _floatArray["_TexIndex"]);
        }

        public void AddOffsets(Vector3 pos)
        {
            _offsets.Add(pos);
        }

        public void AddMatrix(Vector3 position, Vector3 scale)
        {
            foreach (var v in _offsets)
            {
                _matrixs[count] = Matrix4x4.TRS(v + position, Quaternion.identity, scale);
                count++;
            }
        }

        public void AddFloatArray(string blockName, float blockParams)
        {
            if (!_floatArray.ContainsKey(blockName))
            {
                _floatArray.Add(blockName, new List<float>());
            }

            _floatArray[blockName].Add(blockParams);

            //分配属性时，需要先清楚已经分配的属性，避免绘制异常
            _block.Clear();
            _block.SetFloatArray(blockName, _floatArray[blockName]);
        }

        public void Draw()
        {
            Graphics.DrawMeshInstanced(_mesh, 0, _mat, _matrixs, count, _block, UnityEngine.Rendering.ShadowCastingMode.Off, false, (int)Enum.Layer.Gray);
        }

        public void Dispose()
        {
            _mesh = null;
            _mat = null;
        }
    }

    public class GPUInstancedGroup
    {
        private int _assetID;
        private string _prefab;
        private List<Vector3> _positions = new List<Vector3>();
        private List<Vector3> _scales = new List<Vector3>();
        private Dictionary<int, GPUInstanced> _childInstanceds = new Dictionary<int, GPUInstanced>();
        private Dictionary<string, List<float>> _floatArray = new Dictionary<string, List<float>>();

        public GPUInstancedGroup(string prefab, LoadAssetCB<GameObject> callback = null)
        {
            _prefab = prefab;
            _assetID = AssetsMgr.Instance.LoadAssetAsync<GameObject>(_prefab, (prefab) =>
            {
                if (null != callback)
                    callback(prefab);

                FindingGPUInstanced(prefab);

                OnChange();
            });
        }

        public void FindingGPUInstanced(GameObject go)
        {
            MeshFilter mf;
            if (go.TryGetComponent<MeshFilter>(out mf))
            {
                var ms = mf.sharedMesh;
                var meshInstanceID = ms.GetInstanceID();
                if (!_childInstanceds.ContainsKey(meshInstanceID))
                {
                    _childInstanceds.Add(meshInstanceID, new GPUInstanced(ms, go.GetComponent<MeshRenderer>().sharedMaterial));
                }
                _childInstanceds[meshInstanceID].AddOffsets(go.transform.position);
            }

            var childCount = go.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                FindingGPUInstanced(go.transform.GetChild(i).gameObject);
            }
        }

        public void AddInstance(Vector3 position, Vector3 scale, string blockName = null, float blockParams = 0)
        {
            _positions.Add(position);
            _scales.Add(scale);

            if (!_floatArray.ContainsKey(blockName))
                _floatArray[blockName] = new List<float>();
            _floatArray[blockName].Add(blockParams);
        }

        private void OnChange()
        {
            if (_childInstanceds.Count <= 0)
                return;


            foreach (var v in _childInstanceds)
            {
                for (int i = 0; i < _positions.Count; i++)
                {
                    v.Value.AddMatrix(_positions[i], _scales[i]);
                }

                foreach (var v1 in _floatArray)
                {
                    for (int i = 0; i < v1.Value.Count; i++)
                    {
                        v.Value.AddFloatArray(v1.Key, v1.Value[i]);
                    }
                }
            }
            _positions.Clear();
            _scales.Clear();
        }

        public void Draw()
        {
            if (_positions.Count > 0)
            {
                OnChange();
            }

            foreach (var v in _childInstanceds)
                v.Value.Draw();
        }

        public void Dispose()
        {
            if (_assetID > 0)
            {
                AssetsMgr.Instance.ReleaseAsset(_assetID);
                _assetID = 0;
            }

            foreach (var v in _childInstanceds)
                v.Value.Dispose();
            _childInstanceds.Clear();
        }
    }
}
