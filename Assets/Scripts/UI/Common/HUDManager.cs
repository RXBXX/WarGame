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

        public override void Update(float deltaTime)
        {
            for (int i = _hudList.Count - 1; i >= 0; i--)
            {
                if (i >= _hudList.Count)
                    continue;
                _hudList[i].Update(deltaTime);
            }
        }

        public HUD AddHUD(string packageName, string compName, string customName, GameObject go, object[] args = null)
        {
            var ui = (HUD)UIManager.Instance.CreateUI(packageName, compName, args);
            ui.name = customName;
            ui.SetOwner(go);
            _hudList.Add(ui);

            return ui;
        }

        public void RemoveHUD(string customName)
        {
            for (int i = _hudList.Count - 1; i >= 0; i--)
            {
                if (i >= _hudList.Count)
                    continue;
                if (_hudList[i].name == customName)
                {
                    _hudList[i].Dispose(true);
                    _hudList.RemoveAt(i);
                    break;
                }
            }
        }

        public T GetHUD<T>(string hudKey) where T:HUD
        {
            for (int i = _hudList.Count - 1; i >= 0; i--)
            {
                if (i >= _hudList.Count)
                    continue;
                if (_hudList[i].name == hudKey)
                {
                    return (T)_hudList[i];
                }
            }
            return default(T);
        }

        public void SetVisible(bool visible)
        {
            for (int i = _hudList.Count - 1; i >= 0; i--)
            {
                if (i >= _hudList.Count)
                    continue;
                _hudList[i].SetVisible(visible);
            }
        }

        public override bool Dispose()
        {
            base.Dispose();

            for (int i = _hudList.Count - 1; i >= 0; i--)
            {
                if (i > _hudList.Count)
                    continue;
                _hudList[i].Dispose();
            }
            _hudList.Clear();

            return true;
        }
    }
}