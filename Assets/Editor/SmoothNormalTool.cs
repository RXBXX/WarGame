using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace WarGame
{
    public class SmoothNormalTool
    {
        [MenuItem("Tools/平滑法线、写入切线数据")]
        public static void WriteAverageNormalToTangentToos()
        {
            //if (null == Selection.activeGameObject)
            //    return;
            if (null == Selection.gameObjects || Selection.gameObjects.Length <= 0)
                return;

            foreach (var v in Selection.gameObjects)
                PreProcessingFotOutLine(v);
        }

        [MenuItem("Tools/平滑法线、应用切线数据")]
        public static void ApplyAverageNormalToTangentToos()
        {
            //if (null == Selection.activeGameObject)
            //    return;
            if (null == Selection.gameObjects || Selection.gameObjects.Length <= 0)
                return;
            foreach (var v in Selection.gameObjects)
                Tool.Instance.ApplyProcessingFotOutLine(v);
        }


        /// <summary>
        /// 平滑发现，将平滑后的法线写入切线位置，解决描边不连续的问题
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
                WriteAverageNormalToTangent(mesh);
                return;
            }

            MeshFilter meshFilter = null;
            if (go.TryGetComponent<MeshFilter>(out meshFilter))
            {
                var mesh = meshFilter.sharedMesh;
                WriteAverageNormalToTangent(mesh);
                return;
            }
        }

        public static void WriteAverageNormalToTangent(Mesh mesh)
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
            //Debug.Log("Write" + sqrt * sqrt);
            for (int i = 0; i < sqrt; i++)
            {
                for (int j = 0; j < sqrt; j++)
                {
                    var index = i * sqrt + j;
                    if (index >= tangents.Length)
                        break;
                    //Debug.Log(i+"_"+j +":"+ new Color((tangents[index].x + 1) / 2, (tangents[index].y + 1) / 2, (tangents[index].z + 1) / 2, (tangents[index].w + 1) / 2));
                    texture.SetPixel(i, j, new Color((tangents[index].x + 1) / 2, (tangents[index].y + 1) / 2, (tangents[index].z + 1) / 2, (tangents[index].w + 1) / 2));
                }
            }
            texture.Apply();
            var bytes = texture.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath + "/Textures/MeshTagentTex/" + mesh.name + ".png", bytes);
            GameObject.DestroyImmediate(texture);

            AssetDatabase.Refresh();

            string path = "Assets/Textures/MeshTagentTex/" + mesh.name + ".png";
            TextureImporter import = AssetImporter.GetAtPath(path) as TextureImporter;
            import.isReadable = true;
            import.npotScale = TextureImporterNPOTScale.None;
            
            AssetDatabase.ImportAsset(path);

            DebugManager.Instance.Log("成功写入切线数据：" + mesh.name);
        }
    }
}