using UnityEngine;
using WarGame.UI;

namespace WarGame
{
    public class Entrance:MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            UIManager.Instance.OpenPanel("Main", "MainPanel");
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
