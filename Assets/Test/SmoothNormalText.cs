using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothNormalText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        WriteAverageNormalToTangent(GetComponent<MeshFilter>().mesh);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void WriteAverageNormalToTangent(Mesh mesh)
    {
        var averageNormalHash = new Dictionary<Vector3, Vector3>();
        for (var j = 0; j < mesh.vertices.Length; j++)
        {
            if (!averageNormalHash.ContainsKey(mesh.vertices[j]))
            {
                averageNormalHash.Add(mesh.vertices[j], mesh.normals[j]);
            }
            else
            {
                averageNormalHash[mesh.vertices[j]] = (averageNormalHash[mesh.vertices[j]] + mesh.normals[j]).normalized;
            }
        }
        var averageNormals = new Vector3[mesh.vertexCount];
        for (var j = 0; j < mesh.vertices.Length; j++)
        {
            averageNormals[j] = averageNormalHash[mesh.vertices[j]];
        }

        var tangents = new Vector4[mesh.vertexCount];
        for (var j = 0; j < mesh.vertices.Length; j++)
        {
            tangents[j] = new Vector4(averageNormals[j].x, averageNormals[j].y, averageNormals[j].z, 0);
        }

        mesh.tangents = tangents;
    }
}
