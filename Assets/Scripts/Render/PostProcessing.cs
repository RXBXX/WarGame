using UnityEngine;

namespace WarGame
{
    public class PostProcessing
    {
        protected Material _mat;

        public virtual void Setup()
        {
            CameraMgr.Instance.MainCamera.GetComponent<CameraRender>().enabled = true;
        }

        public virtual void Render(RenderTexture source, RenderTexture destination)
        {
        
        }

        public virtual void Clear()
        {
            CameraMgr.Instance.MainCamera.GetComponent<CameraRender>().enabled = false;
        }
    }
}
