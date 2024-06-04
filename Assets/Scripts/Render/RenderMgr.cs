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
            _assetID = AssetMgr.Instance.LoadAssetAsync<Material>("Assets/Materials/BlurMat.mat", (mat) =>
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
            AssetMgr.Instance.ReleaseAsset(_assetID);
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


        private PostProcessing _pp;
        private Dictionary<int, BlurStruct> _blurDic = new Dictionary<int, BlurStruct>();

        public void OpenPostProcessiong(Enum.PostProcessingType type)
        {
            if (type == Enum.PostProcessingType.Gray)
            {
                _pp = new GrayPP();
                _pp.Setup();
            }
        }

        public void ClosePostProcessiong()
        {
            if (null == _pp)
                return;
            _pp.Clear();
            _pp = null;
        }

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (null == _pp)
                return;
            _pp.Render(source, destination);
        }

        public int SetBlurBG(GLoader loader)
        {
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
    }
}
