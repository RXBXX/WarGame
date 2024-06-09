using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class GPUInstanced
    {
        private Mesh _mesh;
        private Material _mat;
        private List<Matrix4x4> _matrixs = new List<Matrix4x4>();
        private List<Vector3> _offsets = new List<Vector3>();

        public GPUInstanced(Mesh mesh, Material mat)
        {
            this._mesh = mesh;
            this._mat = mat;
        }

        public void AddOffsets(Vector3 pos)
        {
            _offsets.Add(pos);
        }

        public void AddPosition(Vector3 position)
        {
            foreach (var v in _offsets)
                _matrixs.Add(Matrix4x4.TRS(v + position, Quaternion.identity, Vector3.one));
        }

        public void Draw()
        {
            Graphics.DrawMeshInstanced(_mesh, 0, _mat, _matrixs);
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
        private Dictionary<int, GPUInstanced> _childInstanceds = new Dictionary<int, GPUInstanced>();

        public GPUInstancedGroup(string prefab, LoadAssetCB<GameObject> callback = null)
        {
            _prefab = prefab;
            _assetID = AssetMgr.Instance.LoadAssetAsync<GameObject>(_prefab, (prefab) =>
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

        public void AddInstance(Vector3 position)
        {
            _positions.Add(position);
        }

        private void OnChange()
        {
            if (_childInstanceds.Count <= 0)
                return;

            foreach (var v in _positions)
            {
                foreach (var v1 in _childInstanceds)
                    v1.Value.AddPosition(v);
            }
            _positions.Clear();
        }

        public void Draw()
        {
            if(_positions.Count > 0)
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
                AssetMgr.Instance.ReleaseAsset(_assetID);
                _assetID = 0;
            }

            foreach (var v in _childInstanceds)
                v.Value.Dispose();
            _childInstanceds.Clear();
        }
    }
}
