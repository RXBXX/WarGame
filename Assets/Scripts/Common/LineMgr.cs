using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WarGame
{
    public class LineMgr : Singeton<LineMgr>
    {
        private LineRenderer _lineRenderer = null;
        public override bool Init()
        {
            base.Init();

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Line.prefab");
            _lineRenderer = GameObject.Instantiate(prefab).GetComponent<LineRenderer>();
            _lineRenderer.transform.rotation = Quaternion.Euler(90, 0, 0);

            return true;
        }

        public void SetLine(List<Vector3> points)
        {
            _lineRenderer.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                _lineRenderer.SetPosition(i, points[i]);
            }
        }

        public override bool Dispose()
        {
            base.Dispose();

            GameObject.Destroy(_lineRenderer.gameObject);
            _lineRenderer = null;

            return true;
        }
    }
}
