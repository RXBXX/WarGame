using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class GrayPP :PostProcessing
    {
        private int _assetID;
        //private Camera _depthCamera;
        private Camera _colorCamera;
        //private RenderTexture _depthRT;
        private RenderTexture _colorRT;

        public GrayPP(params object[] args) : base(args)
        {
            _assetID = AssetsMgr.Instance.LoadAssetAsync<Material>("Assets/Materials/GrayMat.mat", (Material mat) => {
                _mat = mat;
                _mat.SetTexture("_ExclusionMap", _colorRT);
                _mat.SetFloat("_StartTime", TimeMgr.Instance.GetTimeSinceLevelLoad());
            });
            var mainCamera = CameraMgr.Instance.MainCamera;

            _colorCamera = mainCamera.transform.Find("ColorCamera").GetComponent<Camera>();
            _colorCamera.gameObject.SetActive(true);
            _colorRT = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
            _colorCamera.GetComponent<Camera>().targetTexture = _colorRT;
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
            _colorCamera.gameObject.SetActive(false);
            _colorCamera.targetTexture = null;
            RenderTexture.ReleaseTemporary(_colorRT);

            AssetsMgr.Instance.ReleaseAsset(_assetID);
            _assetID = 0;
            base.Clear();
        }
    }
}