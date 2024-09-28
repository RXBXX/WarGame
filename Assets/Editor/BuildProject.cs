using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using UnityEditor.Build.Reporting;
using System.IO;

namespace WarGame
{
    public class BuildProject
    {
        [MenuItem("Tools/���")]
        private static void Build()
        {
            // ���ô��ѡ��
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Main.unity" }, // ָ��Ҫ����ĳ���
                locationPathName = "APP/WarGame.exe",             // ָ�����·��
                target = BuildTarget.StandaloneWindows,             // ָ�����ƽ̨
                options = BuildOptions.Development                 // ָ�����ѡ��
                
            };

            // ִ�д��
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            // ��������
            OnBuildCompleted(summary.result);
        }

        private static void OnBuildCompleted(BuildResult result)
        {
            if (result != BuildResult.Succeeded)
            {
                return;
            }

            //DebugManager.Instance.Log("AESEncrypt");

            //File.Delete("E://WarGame/APP/WarGame_Data/StreamingAssets/Datas/GameData.json");

            //�������ñ�
            var dirs = Directory.GetFiles("E://WarGame/APP/WarGame_Data/StreamingAssets/Configs/");
            foreach (var v in dirs)
            {
                //DebugManager.Instance.Log(v);
                var str = File.ReadAllText(v);
                str = Tool.AESEncrypt(str);
                //DebugManager.Instance.Log(Tool.AESDecrypt(str)); 
                File.WriteAllText(v, str);
            }

            //���ܵ�ͼ�ļ�
            dirs = Directory.GetFiles("E://WarGame/APP/WarGame_Data/StreamingAssets/Maps/");
            foreach (var v in dirs)
            {
                //DebugManager.Instance.Log(v);
                var str = File.ReadAllText(v);
                str = Tool.AESEncrypt(str);
                //DebugManager.Instance.Log(Tool.AESDecrypt(str)); 
                File.WriteAllText(v, str);
            }
        }
    }
}
