using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;

namespace WarGame
{
    public class LogEditor
    {
        private class LogEditorConfig
        {
            public string logScriptPath = "";   //�Զ�����־�ű�·��
            public string logTypeName = "";     //�ű�type
            public int instanceID = 0;

            public LogEditorConfig(string logScriptPath, System.Type logType)
            {
                this.logScriptPath = logScriptPath;
                this.logTypeName = logType.FullName;
            }
        }

        //���õ���־
        private static LogEditorConfig[] _logEditorConfig = new LogEditorConfig[]
        {
        new LogEditorConfig("Assets/Scripts/Common/DebugManager.cs", typeof(DebugManager))
        };

        //�����ConsoleWindow˫����ת
        //[OnOpenAsset(0)]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            Debug.Log("OpenAsset");
            for (int i = _logEditorConfig.Length - 1; i >= 0; --i)
            {
                var configTmp = _logEditorConfig[i];
                UpdateLogInstanceID(configTmp);
                if (instanceID == configTmp.instanceID)
                {
                    var statckTrack = GetStackTrace();
                    if (!string.IsNullOrEmpty(statckTrack))
                    {
                        /*
                        ����˵�������������һ��ConsoleWindow����־��Ϣ
                        Awake
                        UnityEngine.Debug:Log(Object)
                        DDebug:Log(String) (at Assets/Scripts/DDebug/DDebug.cs:13)
                        Test:Awake() (at Assets/Scripts/Test.cs:13)

                        ˵����
                        1�����е�һ�е�"Awake":��ָ�����Զ����ӡ��־�����ĺ���������������Test�ű��е�Awake��������õ�
                        2���ڶ��е�"UnityEngine.Debug:Log(Object)":��ָ����־��ײ���ͨ��Debug.Log������ӡ������
                        3�������е�"DDebug:Log(String) (at Assets/Scripts/DDebug/DDebug.cs:13)":ָ�ڶ��еĺ���������DDebug.cs��13��
                        4�������е�"Test:Awake() (at Assets/Scripts/Test.cs:13)":ָTest.cs�ű���Awake���������˵ڶ��е�DDebug.cs��Log�������ڵ�13��
                         */

                        //ͨ��������Ϣ�����ѵó�˫������־Ӧ�ô�Test.cs�ļ�������λ����13��
                        //�Ի��зָ��ջ��Ϣ
                        var fileNames = statckTrack.Split('\n');
                        //��λ�������Զ�����־��������һ�У�"Test:Awake() (at Assets/Scripts/Test.cs:13)"
                        var fileName = GetCurrentFullFileName(fileNames);
                        //��λ��������������13
                        var fileLine = LogFileNameToFileLine(fileName);
                        //�õ������Զ�����־�����Ľű���"Assets/Scripts/Test.cs"
                        fileName = GetRealFileName(fileName);

                        //���ݽű������������򿪽ű�
                        //"Assets/Scripts/Test.cs"
                        //13
                        AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fileName), fileLine);
                        return true;
                    }
                    break;
                }
            }

            return false;
        }

        /// <summary>
        /// �������־��ջ
        /// </summary>
        /// <returns></returns>
        private static string GetStackTrace()
        {
            var consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            var fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            var consoleWindowInstance = fieldInfo.GetValue(null);

            if (null != consoleWindowInstance)
            {
                if ((object)EditorWindow.focusedWindow == consoleWindowInstance)
                {
                    fieldInfo = consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
                    string activeText = fieldInfo.GetValue(consoleWindowInstance).ToString();
                    return activeText;
                }
            }
            return "";
        }

        private static void UpdateLogInstanceID(LogEditorConfig config)
        {
            if (config.instanceID > 0)
            {
                return;
            }

            var assetLoadTmp = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(config.logScriptPath);
            if (null == assetLoadTmp)
            {
                throw new System.Exception("not find asset by path=" + config.logScriptPath);
            }
            config.instanceID = assetLoadTmp.GetInstanceID();
        }

        private static string GetCurrentFullFileName(string[] fileNames)
        {
            string retValue = "";
            int findIndex = -1;

            for (int i = fileNames.Length - 1; i >= 0; --i)
            {
                bool isCustomLog = false;
                for (int j = _logEditorConfig.Length - 1; j >= 0; --j)
                {
                    if (fileNames[i].Contains(_logEditorConfig[j].logTypeName))
                    {
                        isCustomLog = true;
                        break;
                    }
                }
                if (isCustomLog)
                {
                    findIndex = i;
                    break;
                }
            }

            if (findIndex >= 0 && findIndex < fileNames.Length - 1)
            {
                retValue = fileNames[findIndex + 1];
            }

            return retValue;
        }

        private static string GetRealFileName(string fileName)
        {
            int indexStart = fileName.IndexOf("(at ") + "(at ".Length;
            int indexEnd = ParseFileLineStartIndex(fileName) - 1;

            fileName = fileName.Substring(indexStart, indexEnd - indexStart);
            return fileName;
        }

        private static int LogFileNameToFileLine(string fileName)
        {
            int findIndex = ParseFileLineStartIndex(fileName);
            string stringParseLine = "";
            for (int i = findIndex; i < fileName.Length; ++i)
            {
                var charCheck = fileName[i];
                if (!IsNumber(charCheck))
                {
                    break;
                }
                else
                {
                    stringParseLine += charCheck;
                }
            }

            return int.Parse(stringParseLine);
        }

        private static int ParseFileLineStartIndex(string fileName)
        {
            int retValue = -1;
            for (int i = fileName.Length - 1; i >= 0; --i)
            {
                var charCheck = fileName[i];
                bool isNumber = IsNumber(charCheck);
                if (isNumber)
                {
                    retValue = i;
                }
                else
                {
                    if (retValue != -1)
                    {
                        break;
                    }
                }
            }
            return retValue;
        }

        private static bool IsNumber(char c)
        {
            return c >= '0' && c <= '9';
        }
    }
}
