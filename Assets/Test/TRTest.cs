using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TRTest : MonoBehaviour
{
    public GameObject dizzy;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            dizzy.SetActive(false);
            var tweener = dizzy.transform.DOMove(new Vector3(5,5,5), 1);
            tweener.onComplete = () => {
                var trs = dizzy.GetComponentsInChildren<TrailRenderer>();
                foreach (var v in trs)
                    v.Clear();
                dizzy.SetActive(true);
              } ;
        }
    }
}
