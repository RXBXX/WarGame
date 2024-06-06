using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace WarGame
{
    public class SmoothNormalTool
    {
        [MenuItem("Tools/ƽ�����ߡ�д����������")]
        public static void WriteAverageNormalToTangentToos()
        {
            if (null == Selection.activeGameObject)
                return;

            PreProcessingFotOutLine(Selection.activeGameObject);
        }

        [MenuItem("Tools/ƽ�����ߡ�Ӧ����������")]
        public static void ApplyAverageNormalToTangentToos()
        {
            if (null == Selection.activeGameObject)
                return;

            Tool.Instance.ApplyProcessingFotOutLine(Selection.activeGameObject);
        }


        /// <summary>
        /// ƽ�����֣���ƽ����ķ���д������λ�ã������߲�����������
        /// </summary>
        /// <param name="go"></param>
        private static void PreProcessingFotOutLine(GameObject go, string name = null)
        {
            if (null == name)
                name = go.name;
            for (int i = 0; i < go.transform.childCount; i++)
            {
                var child = go.transform.GetChild(i).gameObject;
                PreProcessingFotOutLine(child, name + "_" + child.name);
            }

            SkinnedMeshRenderer meshRenderer = null;
            if (go.TryGetComponent<SkinnedMeshRenderer>(out meshRenderer))
            {
                var mesh = meshRenderer.sharedMesh;
                WriteAverageNormalToTangent(mesh, name);
                return;
            }

            MeshFilter meshFilter = null;
            if (go.TryGetComponent<MeshFilter>(out meshFilter))
            {
                var mesh = meshFilter.sharedMesh;
                WriteAverageNormalToTangent(mesh, name);
                return;
            }

            DebugManager.Instance.Log("�ɹ�д���������ݣ�"+ go.name);
        }

        public static void WriteAverageNormalToTangent(Mesh mesh, string name)
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

            //mesh.tangents = tangents;

            var sqrt = Mathf.CeilToInt(Mathf.Sqrt(tangents.Length));
            var texture = new Texture2D(sqrt, sqrt, TextureFormat.RGBA32, false);
            for (int i = 0; i < sqrt; i++)
            {
                for (int j = 0; j < sqrt; j++)
                {
                    var index = i * sqrt + j;
                    if (index >= tangents.Length)
                        break;
                    texture.SetPixel(i, j, new Color((tangents[index].x + 1)/2, (tangents[index].y+1)/2, (tangents[index].z+1)/2, (tangents[index].w+1)/2)); ;
                }
            }
            texture.Apply();

            var bytes = texture.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/Textures/MeshTagentTex/" + mesh.name + ".png", bytes);

            GameObject.DestroyImmediate(texture);
        }
}
}