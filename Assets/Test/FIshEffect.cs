using UnityEngine;


public class FisheyeEffect : MonoBehaviour
{
    public Material mat;

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, mat);
    }
}
