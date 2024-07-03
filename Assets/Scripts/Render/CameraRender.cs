using UnityEngine;

namespace WarGame
{
    public class CameraRender : MonoBehaviour
    {
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Game.Instance.OnRenderImage(source, destination);
        }

    }
}
