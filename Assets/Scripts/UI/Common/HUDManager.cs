using System.Collections.Generic;
using UnityEngine;

namespace WarGame.UI
{
    public class HUDManager : Singeton<HUDManager>
    {
        private List<HUD> _hudList = new List<HUD>();

        public override bool Init()
        {
            base.Init();
            return true;
        }

        public void Update()
        {
            for (int i = _hudList.Count - 1; i >= 0; i++)
            {
                if (null != _hudList[i])
                {
                    _hudList[i].Update();
                }
            }
        }

        public void AddHUD(string packageName, string compName, string customName, GameObject go)
        {
            var ui = (HUD)UIManager.Instance.CreateUI(packageName, compName);
            ui.name = customName;
            ui.SetOwner(go);
            _hudList.Add(ui);
        }

        public void RemoveHUD(string customName)
        {
            for (int i = _hudList.Count - 1; i >= 0; i++)
            {
                if (null != _hudList[i] && _hudList[i].name == customName)
                {
                    _hudList.RemoveAt(i);
                    break;
                }
            }
            _hudList.Clear();
        }

        public override bool Dispose()
        {
            base.Dispose();

            for (int i = _hudList.Count - 1; i >= 0; i++)
            {
                if (null != _hudList[i])
                {
                    _hudList[i].Dispose();
                }
            }
            _hudList.Clear();

            return true;
        }
    }
}