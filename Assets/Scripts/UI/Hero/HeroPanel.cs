using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using UnityEditor;

namespace WarGame.UI
{
    public class HeroPanel : UIBase
    {
        private Transform _heroRoot;
        private Vector2 _touchPos;

        public HeroPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            var heroPath = args[0].ToString();
            var prefab = AssetMgr.Instance.LoadAsset<GameObject>(heroPath);
            var hero = GameObject.Instantiate(prefab);
            _heroRoot = SceneMgr.Instance.GetHeroRoot();
            hero.transform.SetParent(_heroRoot);
            hero.transform.localPosition = Vector3.zero;

            _gCom.GetChild("closeBtn").onClick.Add((EventContext context) =>
            {
                SceneMgr.Instance.CloseHeroScene();
            });

            _gCom.onTouchBegin.Add((EventContext context) => {
                context.CaptureTouch();
                _touchPos = context.inputEvent.position;
            });
            _gCom.onTouchMove.Add((EventContext context) =>
            {
                _heroRoot.Rotate(Vector3.up,  (_touchPos.x - context.inputEvent.position.x));
                _touchPos = context.inputEvent.position;
            });
        }

        public override void Dispose(bool disposeGCom = false)
        {
            base.Dispose(disposeGCom);
        }
    }
}
