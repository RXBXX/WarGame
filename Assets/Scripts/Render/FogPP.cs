//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class FogPP :PostProcessing
    {
        private int _assetID;

        public FogPP(params object[] args) : base(args)
        {
            _assetID = AssetsMgr.Instance.LoadAssetAsync<Material>("Assets/Materials/VolumeCloud.mat", (Material mat) => {
                _mat = mat;
            });

            var cam = CameraMgr.Instance.MainCamera;
            if (null == cam)
                return;
            cam.depthTextureMode = DepthTextureMode.Depth;
        }

        public override void Update(float deltaTime)
        {
            var cam = CameraMgr.Instance.MainCamera;
            if (null == cam)
                return;

            Matrix4x4 mt = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false) * cam.worldToCameraMatrix;
            mt = mt.inverse;
            Shader.SetGlobalMatrix("_InvVP", mt);
        }

        public override RenderTexture Render(RenderTexture source, RenderTexture destination)
        {
            if (null == _mat)
                return null;
            Graphics.Blit(source, destination, _mat);
            return destination;
        }

        public override void Clear()
        {
            AssetsMgr.Instance.ReleaseAsset(_assetID);
            _assetID = 0;
            base.Clear();
        }
    }
}