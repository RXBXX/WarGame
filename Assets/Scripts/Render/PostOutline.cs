using UnityEngine;

[ExecuteInEditMode]
public class PostOutline : MonoBehaviour
{
    public Shader shader;
    public Material material;
    public Color EdgeColor;
    [Range(0, 1)]
    public float EdgeThreshold;
    [Range(0, 1)]
    public float edgeOnly;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (shader != null && material == null)
        {
            material = new Material(shader);
        }
        if (material != null)
        {
            material.SetColor("_EdgeColor", EdgeColor);
            material.SetFloat("_EdgeThreshold", EdgeThreshold);
            material.SetFloat("_EdgeOnly", edgeOnly);
            Graphics.Blit(source, destination, material);
        }
        else
            Graphics.Blit(source, destination);
    }
}
