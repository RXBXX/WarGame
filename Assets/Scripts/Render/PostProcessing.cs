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

        public virtual void Update(float deltaTime)
        { 
        
        }

        public virtual RenderTexture Render(RenderTexture source, RenderTexture destination)
        {
            return destination;
        }

        public virtual void Clear()
        {
            //CameraMgr.Instance.MainCamera.GetComponent<CameraRender>().enabled = false;
        }
    }
}
