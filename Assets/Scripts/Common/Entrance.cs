using UnityEngine;
using WarGame.UI;

namespace WarGame
{
    public class Entrance:MonoBehaviour
    {
        private void Awake()
        {
            //Debug.Log("Entrance Awake");
            DontDestroyOnLoad(this);
            DontDestroyOnLoad(GameObject.Find("Main Camera"));
            Game.Instance.Init();
        }

        private void Start()
        {
            //Debug.Log("Entrance Start");
            Game.Instance.Start();
        }

        private void Update()
        {
            Game.Instance.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            Game.Instance.FixedUpdate(Time.deltaTime);
        }

        private void LateUpdate()
        {
            Game.Instance.LateUpdate();
        }

        private void OnApplicationQuit()
        {
            //Game.Instance.Dispose();
        }
    }
}
