using UnityEngine;

public class ExcludeFromGrayScale : MonoBehaviour
{
    public bool excludeFromGrayScale = false;

    private void Start()
    {
        if (excludeFromGrayScale)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.SetFloat("_ExcludedObject", 1);
            }
        }
    }
}
