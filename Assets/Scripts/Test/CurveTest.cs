using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CurveTest : MonoBehaviour
{
    private float factor = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (factor >= 1)
            return;

        var pos = DOCurve.CubicBezier.GetPointOnSegment(Vector3.zero, new Vector3(0, 5, 0), new Vector3(5, 0, 0), new Vector3(5, 5, 0), factor);
        transform.position = pos;

        factor += Time.deltaTime;
    }
}
