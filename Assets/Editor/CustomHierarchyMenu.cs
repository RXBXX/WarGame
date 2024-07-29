using UnityEditor;
using UnityEngine;

namespace WarGame
{
    public static class CustomHierarchyMenu
    {
        [MenuItem("GameObject/Ìí¼ÓÉãÏñ»úµã", false, 0)]
        private static void CustomAction1(MenuCommand menuCommand)
        {
            GameObject go = menuCommand.context as GameObject;
            var point = WGVector3.ToWGVector3(go.transform.position);
            GameObject.Find("FloatPoint").GetComponent<MapFloatPoint>().Points.Add(point);
        }
    }
}
