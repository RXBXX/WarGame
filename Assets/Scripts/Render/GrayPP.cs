using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class GrayPP :PostProcessing
    {
        private Camera _depthCamera;
        private Camera _colorCamera;
        private RenderTexture _depthRT;
        private RenderTexture _colorRT;

        public override void Setup()
        {
            AssetMgr.Instance.LoadAssetAsync<Material>("Assets/Materials/GrayMat.mat", (Material mat)=> {
                _mat = mat;
                _mat.SetTexture("_ExclusionMap", _colorRT);
                _mat.SetTexture("_ExclusionMapDepth", _depthRT);
                _mat.SetFloat("_StartTime", TimeMgr.Instance.GetTimeSinceLevelLoad());
            });
            var mainCamera = CameraMgr.Instance.MainCamera;
            _depthCamera = mainCamera.transform.Find("DepthCamera").GetComponent<Camera>();
            _depthCamera.gameObject.SetActive(true);
            _depthRT = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
            _depthCamera.targetTexture = _depthRT;

            _colorCamera = mainCamera.transform.Find("ColorCamera").GetComponent<Camera>();
            _colorCamera.gameObject.SetActive(true);
            _colorRT = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
            _colorCamera.GetComponent<Camera>().targetTexture = _colorRT;

            base.Setup();
        }

        public override void Render(RenderTexture source, RenderTexture destination)
        {
            if (null == _mat)
                return;
            Graphics.Blit(source, destination, _mat);
        }

        public override void Clear()
        {
            base.Clear();

            _depthCamera.gameObject.SetActive(false);
            _depthCamera.targetTexture = null;
            RenderTexture.ReleaseTemporary(_depthRT);

            _colorCamera.gameObject.SetActive(false);
            _colorCamera.targetTexture = null;
            RenderTexture.ReleaseTemporary(_colorRT);
        }
    }
}