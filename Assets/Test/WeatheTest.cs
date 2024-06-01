using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class WhetherTest : MonoBehaviour
{
    private float time = 0;
    private float day = 60f;
    public Light _light;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        var dayTime = time % day / day;
        RenderSettings.skybox.SetFloat("_Exposure", 0.3F + (1 + Mathf.Sin(dayTime * 2 * Mathf.PI)) / 2);
        RenderSettings.skybox.SetFloat("_Rotation", dayTime * 360);
        _light.transform.rotation = Quaternion.Euler(new Vector3(45, dayTime * 360, 180));
        _light.intensity = 0.2F + (1 + Mathf.Sin(time % day / day * 2 * Mathf.PI)) / 2;
    }
}
