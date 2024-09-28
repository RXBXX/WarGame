using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using UnityEditor.Build.Reporting;
using System.IO;

namespace WarGame
{
    public class BuildProject
    {
        [MenuItem("Tools/打包")]
        private static void Build()
        {
            // 设置打包选项
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Main.unity" }, // 指定要打包的场景
                locationPathName = "APP/WarGame.exe",             // 指定输出路径
                target = BuildTarget.StandaloneWindows,             // 指定打包平台
                options = BuildOptions.Development                 // 指定打包选项
                
            };

            // 执行打包
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            // 检查打包结果
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

            //加密配置表
            var dirs = Directory.GetFiles("E://WarGame/APP/WarGame_Data/StreamingAssets/Configs/");
            foreach (var v in dirs)
            {
                //DebugManager.Instance.Log(v);
                var str = File.ReadAllText(v);
                str = Tool.AESEncrypt(str);
                //DebugManager.Instance.Log(Tool.AESDecrypt(str)); 
                File.WriteAllText(v, str);
            }

            //加密地图文件
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
