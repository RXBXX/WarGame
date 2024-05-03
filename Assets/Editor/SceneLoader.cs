using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using UnityEngine;

public class SceneLoader : EditorWindow
{
    private string scenesPath = "Assets/Scenes"; // 指定场景文件夹路径

    [MenuItem("Tools/Scene Loader")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(SceneLoader));
    }

    private void OnGUI()
    {
        GUILayout.Label("Select Scene to Load", EditorStyles.boldLabel);

        string[] sceneFiles = Directory.GetFiles(scenesPath, "*.unity");

        foreach (string sceneFile in sceneFiles)
        {
            if (GUILayout.Button(Path.GetFileNameWithoutExtension(sceneFile)))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(sceneFile);
                }
            }
        }
    }
}
