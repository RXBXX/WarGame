using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainTest : MonoBehaviour
{
    public int step = 5;
    public GameObject startGO;
    public GameObject endGO;
    public LineRenderer lr;
    public float droop = 1F;
    public int positionCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        positionCount = step + 1;
        lr.positionCount = positionCount;
        var startPos = startGO.transform.position;
        var endPos = endGO.transform.position;
        lr.SetPosition(0, startPos);
        for (int i = 1; i < lr.positionCount - 1; i++)
        {
            float index = i;
            var tempDroop = Mathf.Sqrt(droop * (1 - Mathf.Abs((index - step / 2.0F) / (step / 2.0F))));
            var frontPart = (step - index) / step;
            var backPart = index / step;
            var posY = startPos.y * frontPart + endPos.y * backPart  - tempDroop;
            var posX = startPos.x * frontPart + endPos.x * backPart;
            var posZ = startPos.z * frontPart + endPos.z * backPart;
            lr.SetPosition(i, new Vector3(posX, posY, posZ));
        }
        lr.SetPosition(positionCount - 1, endPos);
    }

    // Update is called once per frame
    void Update()
    {
        if (positionCount != step + 1)
        {
            positionCount = step + 1;
            lr.positionCount = positionCount;
        }

        var startPos = startGO.transform.position;
        var endPos = endGO.transform.position;
        lr.SetPosition(0, startPos);
        for (int i = 1; i < lr.positionCount - 1; i++)
        {
            float index = i;
            var tempDroop = Mathf.Sqrt(droop * (1 - Mathf.Abs((index - step / 2.0F) / (step / 2.0F))));
            Debug.Log(tempDroop);
            var frontPart = (step - index) / step;
            var backPart = index / step;
            var posY = startPos.y * frontPart + endPos.y * backPart - tempDroop;
            var posX = startPos.x * frontPart + endPos.x * backPart;
            var posZ = startPos.z * frontPart + endPos.z * backPart;
            lr.SetPosition(i, new Vector3(posX, posY, posZ));
        }
        lr.SetPosition(positionCount - 1, endPos);
    }
}
