using UnityEngine;

namespace WarGame
{
    public class PostProcessing
    {
        protected Material _mat;

        public PostProcessing(params object[] args)
        { 
        
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
        }
    }
}
