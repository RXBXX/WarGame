using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using FairyGUI;

namespace WarGame.UI
{
    public class FightPanel : UIBase
    {
        public FightPanel(GComponent gCom, string name) : base(gCom, name)
        {
            _gCom.GetChild("closeBtn").onClick.Add(()=> {
                UIManager.Instance.ClosePanel("FightPanel");
                MapManager.Instance.ClearMap();

                SceneManager.LoadScene("Main");
                SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) =>
                {
                    UIManager.Instance.OpenPanel("Map", "MapPanel");
                };
            });
        }
    }
}
