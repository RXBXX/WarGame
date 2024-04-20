using UnityEngine;
using WarGame.UI;

namespace WarGame
{
    public class Entrance:MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log("Entrance Awake");
            DontDestroyOnLoad(this);
            Game.Instance.Init();
        }

        private void Start()
        {
            Debug.Log("Entrance Start");
            Game.Instance.Start();
        }

        private void Update()
        {
            Game.Instance.Update();
        }

        private void LateUpdate()
        {
            Game.Instance.LateUpdate();
        }

        private void OnDestroy()
        {
            Game.Instance.Dispose();
        }
    }
}
