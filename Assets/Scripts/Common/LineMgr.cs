using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WarGame
{
    public class LineMgr : Singeton<LineMgr>
    {
        private GameObject _go;
        private Mesh _mesh;
        private float _width = 0.22F;
        private float _arrowWidth = 0.38F;
        private float _arrowLength = 0.48F;

        public override bool Init()
        {
            base.Init();

            var prefab = AssetMgr.Instance.LoadAsset<GameObject>("Assets/Prefabs/Line.prefab");
            _go = GameObject.Instantiate(prefab);
            GameObject.DontDestroyOnLoad(_go);
            _mesh = new Mesh();
            GameObject.Instantiate(prefab).GetComponent<MeshFilter>().mesh = _mesh;

            return true;
        }

        public void SetLine(List<Vector3> points)
        {
            _go.transform.position = points[0];
            var poss = new Vector3[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                poss[i] = points[i] - points[0];
            }

            int verticesCount = 0;
            for (int i = 0; i < poss.Length; i++)
            {
                verticesCount += 2;
                if (i > 0 && poss[i].y != poss[i - 1].y)
                    verticesCount += 4;
            }
            verticesCount += 3;
            var vertices = new Vector3[verticesCount];
            verticesCount = 0;
            for (int i = 0; i < poss.Length; i++)
            {
                if (0 == i)
                {
                    Vector3 dir = new Vector3(poss[i + 1].x - poss[i].x, 0, poss[i + 1].z - poss[i].z).normalized;
                    var nor = new Vector3(-dir.z, poss[i].y, dir.x);
                    var leftPoint = poss[i] + nor * (_width / 2);
                    var rightPoint = poss[i] - nor * (_width / 2);
                    vertices[verticesCount] = leftPoint;
                    vertices[verticesCount + 1] = rightPoint;
                    verticesCount += 2;
                }
                else
                {
                    if (poss[i].y != poss[i - 1].y)
                    {
                        Vector3 downDir = new Vector3(poss[i].x - poss[i - 1].x, 0, poss[i].z - poss[i - 1].z).normalized;
                        var downCenter = new Vector3((poss[i].x + poss[i - 1].x) / 2, poss[i - 1].y, (poss[i].z + poss[i - 1].z) / 2);
                        var downNormal = new Vector3(-downDir.z, 0, downDir.x);
                        var leftDownPoint = downCenter + downNormal * (_width / 2);
                        var rightDownPoint = downCenter - downNormal * (_width / 2);

                        var upCenter = new Vector3((poss[i].x + poss[i - 1].x) / 2, poss[i].y, (poss[i].z + poss[i - 1].z) / 2);
                        var upNormal = new Vector3(-downDir.z, 0, downDir.x);
                        var leftUpPoint = upCenter + upNormal * (_width / 2);
                        var rightUpPoint = upCenter - upNormal * (_width / 2);

                        vertices[verticesCount] = leftDownPoint;
                        vertices[verticesCount + 1] = rightDownPoint;
                        vertices[verticesCount + 2] = leftUpPoint;
                        vertices[verticesCount + 3] = rightUpPoint;
                        verticesCount += 4;
                    }
                    if (i == poss.Length - 1)
                    {
                        Vector3 dir = new Vector3(poss[i].x - poss[i - 1].x, 0, poss[i].z - poss[i - 1].z).normalized;
                        var nor = new Vector3(- dir.z, 0, dir.x).normalized;
                        var pos = poss[i] - dir * _arrowLength;
                        var leftPoint = pos + nor * (_width / 2);
                        var rightPoint = pos - nor * (_width / 2);
                        vertices[verticesCount] = leftPoint;
                        vertices[verticesCount + 1] = rightPoint;
                        vertices[verticesCount + 2] = pos + nor * _arrowWidth / 2;
                        vertices[verticesCount + 3] = poss[i];
                        vertices[verticesCount + 4] = pos - nor * _arrowWidth / 2;
                        verticesCount += 5;
                    }
                    else
                    {
                        Vector3 dir1 = new Vector3(poss[i].x - poss[i - 1].x, poss[i].y - poss[i - 1].y, poss[i].z - poss[i - 1].z).normalized;
                        var nor1 = new Vector3(-dir1.z, 0, dir1.x).normalized;

                        var leftPoint1 = new Vector3(poss[i - 1].x, poss[i - 1].y, poss[i - 1].z) + nor1 * (_width / 2.0f);
                        var rightPoint1 = new Vector3(poss[i - 1].x, poss[i - 1].y, poss[i - 1].z) - nor1 * (_width / 2.0f);
                        //Debug.Log(dir1 + "_" + nor1 + "_" + leftPoint1 + "_" + rightPoint1);

                        Vector3 dir2 = new Vector3(poss[i].x - poss[i + 1].x, poss[i].y - poss[i + 1].y, poss[i].z - poss[i + 1].z).normalized;
                        var nor2 = new Vector3(-dir2.z, 0, dir2.x).normalized;

                        var leftPoint2 = new Vector3(poss[i + 1].x, poss[i + 1].y, poss[i + 1].z) + nor2 * (_width / 2);
                        var rightPoint2 = new Vector3(poss[i + 1].x, poss[i + 1].y, poss[i + 1].z) - nor2 * (_width / 2);
                        //Debug.Log(dir2 + "_" + nor2 + "_" + leftPoint2 + "_" + rightPoint2);

                        vertices[verticesCount] = CalculateIntersectionPoint(leftPoint1, dir1, rightPoint2, dir2);
                        //Debug.Log(vertices[verticesCount]);
                        vertices[verticesCount + 1] = CalculateIntersectionPoint(rightPoint1, dir1, leftPoint2, dir2);
                        //Debug.Log(vertices[verticesCount + 1]);
                        verticesCount += 2;
                    }
                }
            }

            var triangles = new int[(verticesCount - 3 - 2) * 3 + 3];
            for (int i = 0; i < vertices.Length - 5; i += 2)
            {
                var index = (int)(i / 2);
                triangles[index * 6] = i;
                triangles[index * 6 + 1] = i + 2;
                triangles[index * 6 + 2] = i + 3;

                triangles[index * 6 + 3] = i;
                triangles[index * 6 + 4] = i + 3;
                triangles[index * 6 + 5] = i + 1;
            }

            triangles[triangles.Length - 3] = vertices.Length - 3;
            triangles[triangles.Length - 2] = vertices.Length - 2;
            triangles[triangles.Length - 1] = vertices.Length - 1;

            Vector2[] uvs = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                var index = (int)(i % 4);
                if (index == 0)
                    uvs[i] = Vector2.zero;
                else if (index == 1)
                    uvs[i] = new Vector2(1, 0);
                else if (index == 2)
                    uvs[i] = new Vector2(0, 1);
                else if (index == 3)
                    uvs[i] = new Vector2(1, 1);
            }

            _mesh.vertices = vertices;
            _mesh.triangles = triangles;
            _mesh.uv = uvs;
            _mesh.RecalculateNormals();

            _go.GetComponent<MeshFilter>().mesh = _mesh;
        }

        public void ClearLine()
        {
            _mesh.Clear();
        }

        private Vector3 CalculateIntersectionPoint(Vector3 p1, Vector3 v1, Vector3 p2, Vector3 v2)
        {
            Vector3 v3 = p2 - p1;
            Vector3 cross = Vector3.Cross(v1, v2);
            float determinant = Vector3.Dot(v3, cross);

            // 计算参数 t1
            float t1 = Vector3.Dot(Vector3.Cross(v3, v2), cross) / cross.sqrMagnitude;
            // 计算参数 t2
            float t2 = Vector3.Dot(Vector3.Cross(v3, v1), cross) / cross.sqrMagnitude;

            //// 如果行列式为 0，则表示两条射线平行或共线
            if (Mathf.Approximately(determinant, 0))
            {
                // 检查两条射线是否平行
                if (Vector3.Cross(v1, v3).sqrMagnitude > 0)
                {
                    Debug.Log("Lines are parallel but not collinear, may intersect.");

                    // 如果 t1 和 t2 都是非负数，则表示两条射线相交
                    if (t1 >= 0 && t2 >= 0)
                    {
                        // 计算交点坐标
                        Vector3 intersectionPoint = p1 + t1 * v1;
                        return intersectionPoint;
                    }
                }
                else
                {
                    return (p1 + p2) / 2.0f;
                }
            }

            // 如果 t1 和 t2 都是非负数，则表示两条射线相交
            if (t1 >= 0 && t2 >= 0)
            {
                // 计算交点坐标
                Vector3 intersectionPoint = p1 + t1 * v1;
                return intersectionPoint;
            }
            else
            {
                Debug.Log("Lines do not intersect.");
                return (p1 + p2) / 2.0f;
            }
        }


        public override bool Dispose()
        {
            base.Dispose();
            return true;
        }
    }
}
