
using UnityEngine;

namespace WarGame
{
    public class RenderMgr : Singeton<RenderMgr>
    {
        private PostProcessing _pp;

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
    }
}
