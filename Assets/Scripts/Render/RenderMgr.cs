using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using FairyGUI;

namespace WarGame
{
    public class BlurStruct
    {
        public int ID;
        public int _assetID;
        public Coroutine _coroutine;
        public RenderTexture _src;
        public RenderTexture _dest;
        public GLoader _loader;

        public BlurStruct(int id, GLoader loader)
        {
            this.ID = id;

            _loader = loader;
            _assetID = AssetsMgr.Instance.LoadAssetAsync<Material>("Assets/Materials/BlurMat.mat", (mat) =>
            {
                _src = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
                CameraMgr.Instance.MainCamera.targetTexture = _src;
                _coroutine = CoroutineMgr.Instance.StartCoroutine(BlurIEnumerator(mat));
            });
        }

        private IEnumerator BlurIEnumerator(Material mat)
        {
            yield return null;

            _coroutine = null;

            _dest = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);

            Graphics.Blit(_src, _dest, mat);

            CameraMgr.Instance.MainCamera.targetTexture = null;

            RenderTexture.ReleaseTemporary(_src);
            _src = null;

            _loader.texture = new NTexture(_dest);
        }


        public void Dispose()
        {
            AssetsMgr.Instance.ReleaseAsset(_assetID);
            _assetID = 0;

            if (null != _src)
            {
                RenderTexture.ReleaseTemporary(_src);
                _src = null;
            }

            CameraMgr.Instance.MainCamera.targetTexture = null;

            if (null != _coroutine)
            {
                CoroutineMgr.Instance.StopCoroutine(_coroutine);
                _coroutine = null;
            }

            if (null != _dest)
            {
                RenderTexture.ReleaseTemporary(_dest);
                _dest = null;
            }


        }
    }

    public class RenderMgr : Singeton<RenderMgr>
    {
        private Dictionary<Enum.PostProcessingType, PostProcessing> _ppDic = new Dictionary<Enum.PostProcessingType, PostProcessing>();
        //private PostProcessing _pp;
        private Dictionary<int, BlurStruct> _blurDic = new Dictionary<int, BlurStruct>();
        private Dictionary<string, GPUInstancedGroup> _GPUInstancedDic = new Dictionary<string, GPUInstancedGroup>();

        //private Mesh _instacedMesh;
        //private Material _instanceMat;
        //private ComputeBuffer argsBuffer;
        //private ComputeBuffer positionBuffer;
        //private const int INSTANCE_COUNT = 100;

        public void OpenPostProcessiong(Enum.PostProcessingType type)
        {
            if (_ppDic.ContainsKey(type))
                return;

            if (type == Enum.PostProcessingType.Gray)
            {
                var _pp = new GrayPP();
                _pp.Setup();
                _ppDic.Add(type, _pp);
            }
            else if (type == Enum.PostProcessingType.Fog)
            {
                var _pp = new FogPP();
                _pp.Setup();
                _ppDic.Add(type, _pp);
            }
        }

        public void ClosePostProcessiong(Enum.PostProcessingType type)
        {
            if (!_ppDic.ContainsKey(type))
                return;

            _ppDic[type].Clear();
            _ppDic.Remove(type);

            if (_ppDic.Count <= 0)
                CameraMgr.Instance.MainCamera.GetComponent<CameraRender>().enabled = false;
        }

        public override void Update(float deltaTime)
        {
            foreach (var v in _GPUInstancedDic)
                v.Value.Draw();

            foreach (var v in _ppDic)
                v.Value.Update(deltaTime);
        }

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            RenderTexture temp1 = RenderTexture.GetTemporary(source.width, source.height);
            RenderTexture temp2 = RenderTexture.GetTemporary(source.width, source.height);

            // 初始输入为source
            RenderTexture currentSource = source;
            RenderTexture currentDestination = temp1;

            // 遍历字典中的每个后处理特效
            foreach (var v in _ppDic)
            {
                // 渲染当前特效
                v.Value.Render(currentSource, currentDestination);

                // 交换source和destination
                RenderTexture temp = currentSource;
                currentSource = currentDestination;
                currentDestination = temp;
            }

            // 将最终结果复制到destination
            Graphics.Blit(currentSource, destination);

            // 释放临时RenderTextures
            RenderTexture.ReleaseTemporary(temp1);
            RenderTexture.ReleaseTemporary(temp2);
        }

        public int SetBlurBG(GLoader loader)
        {
            if (null == CameraMgr.Instance.MainCamera)
                return 0;

            var newID = 0;
            foreach (var v in _blurDic)
            {
                if (v.Value.ID >= newID)
                {
                    newID = v.Value.ID + 1;
                }
            }
            _blurDic.Add(newID, new BlurStruct(newID, loader));
            return newID;
        }

        public void ReleaseBlurBG(int id)
        {
            if (!_blurDic.ContainsKey(id))
                return;
            _blurDic[id].Dispose();
            _blurDic.Remove(id);
        }

        private void InitializeBuffers()
        {
            //// 初始化实例位置数据
            //List<Vector3> positions = new List<Vector3>();
            //for (int i = 0; i < INSTANCE_COUNT; i++)
            //{
            //    for (int j = 0; j < INSTANCE_COUNT; j++)
            //    {
            //        var coor = new Vector3(i - INSTANCE_COUNT / 2 + 20, 0, j - INSTANCE_COUNT / 2 + 20);
            //        if (!ContainHexagon(MapTool.Instance.GetHexagonKey(coor)))
            //            positions.Add(MapTool.Instance.GetPosFromCoor(coor));
            //    }
            //}

            //// 创建 ComputeBuffer
            //positionBuffer = new ComputeBuffer(positions.Count, sizeof(float) * 3);
            //positionBuffer.SetData(positions);

            //// 为材质设置 ComputeBuffer
            //_instanceMat.SetBuffer("_PositionBuffer", positionBuffer);

            //// 初始化绘制参数
            //uint[] args = new uint[5] { _instacedMesh.GetIndexCount(0), (uint)positions.Count, 0, 0, 0 };
            //argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            //argsBuffer.SetData(args);
        }

        void ClearBuffers()
        {
            //if (positionBuffer != null) positionBuffer.Release();
            //if (argsBuffer != null) argsBuffer.Release();

            //_instacedMesh = null;
            //_instanceMat = null;
        }

        public void DrawMap()
        {
            //foreach (var v in _instancedDic)
            //    v.Value.Draw();

            //return;
            //if (null == _instanceMat)
            //    return;
            //if (null == _instacedMesh)
            //    return;

            //Graphics.DrawMeshInstancedIndirect(_instacedMesh, 0, _instanceMat, new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000)), argsBuffer);
        }


        /// <summary>
        /// 添加gpu intanced实例
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="pos"></param>
        /// <param name="callback"></param>
        public void AddMeshInstanced(string prefab, Vector3 pos, Vector3 scale, string blockName = null, float blockParams = 0, LoadAssetCB<GameObject> callback = null)
        {
            if (!_GPUInstancedDic.ContainsKey(prefab))
            {
                _GPUInstancedDic[prefab] = new GPUInstancedGroup(prefab, callback);
            }
            _GPUInstancedDic[prefab].AddInstance(pos, scale, blockName, blockParams);
        }

        /// <summary>
        /// 删除gpu intanced实例
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="pos"></param>
        /// <param name="callback"></param>
        public void RemoveMeshInstanced(string prefab)
        {
            if (!_GPUInstancedDic.ContainsKey(prefab))
                return;
            _GPUInstancedDic.Remove(prefab);
        }

        public override bool Dispose()
        {
            foreach (var v in _GPUInstancedDic)
                v.Value.Dispose();
            _GPUInstancedDic.Clear();

            foreach (var v in _ppDic)
                v.Value.Clear();
            _ppDic.Clear();

            return base.Dispose();
        }
    }
}
