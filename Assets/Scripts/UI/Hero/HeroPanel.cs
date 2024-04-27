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
        private GGraph _touchArena;

        public HeroPanel(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            Debug.Log(args[0]);
            var heroConfing = ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", (int)args[0]);
            var prefab = AssetMgr.Instance.LoadAsset<GameObject>(heroConfing.Prefab);
            var hero = GameObject.Instantiate(prefab);
            _heroRoot = SceneMgr.Instance.GetHeroRoot();
            hero.transform.SetParent(_heroRoot);
            hero.transform.localPosition = Vector3.zero;
            _touchArena = (GGraph)_gCom.GetChild("touchArena");

            _gCom.GetChild("closeBtn").onClick.Add((EventContext context) =>
            {
                SceneMgr.Instance.CloseHeroScene();
            });

            _gCom.onTouchBegin.Add((EventContext context) => {
                if (Stage.inst.touchTarget != _touchArena.displayObject)
                    return;
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
