using UnityEngine;

namespace WarGame
{
    public class DebugManager : Singeton<DebugManager>
    {
        public void Log(object message)
        {
            return;
            Debug.Log(message);
        }

        public void LogError(object message)
        {
            Debug.LogError(message);
        }

        public void LogWarning(object message)
        {
            Debug.LogWarning(message);
        }

        public void ClearLog()
        {
            Debug.ClearDeveloperConsole();
        }
    }
}
