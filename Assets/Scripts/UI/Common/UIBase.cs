using System.Collections.Generic;
using FairyGUI;
using System;
using UnityEngine;

namespace WarGame.UI
{
    public class UIBase
    {
        public Enum.UILayer UILayer = Enum.UILayer.PanelLayer;
        public string name;
        protected GComponent _gCom;
        protected Dictionary<string, UIBase> childDic = new Dictionary<string, UIBase>();

        public GComponent GCom
        {
            get { return _gCom; }
        }
        public UIBase(GComponent gCom, string name, object[] args = null)
        {
            this._gCom = gCom;
            this.name = name;

            var childCount = gCom.numChildren;
            for (int i = 0; i < childCount; i++)
            {
                var childGCom = gCom.GetChildAt(i);
                if (null == childGCom.packageItem)
                    continue;

                if (childGCom.packageItem.type != PackageItemType.Component)
                    continue;

                var childUI = UIManager.Instance.CreateUI<UIBase>(childGCom.packageItem.name, childGCom);
                if (null != childUI)
                    childDic[childGCom.name] = childUI;
            }
        }

        public virtual void OnEnable()
        {

        }

        public virtual void Update(float deltaTime)
        {

        }

        public void SetParent(GComponent parent, int childIndex = 0)
        {
            //parent.container.AddChild(_gCom.displayObject);
            parent.AddChild(_gCom);
        }

        public void RemoveParent()
        {
            _gCom.displayObject.RemoveFromParent();
        }

        public void SetVisible(bool visible)
        {
            if (_gCom.visible = visible)
                return;
            _gCom.visible = visible;
        }

        public void SetPosition(Vector2 pos)
        {
            _gCom.position = pos;
        }

        protected T GetChild<T>(string name) where T : UIBase
        {
            if (childDic.ContainsKey(name))
                return (T)childDic[name];
            else
                return default(T);
        }

        protected T GetGObjectChild<T>(string name) where T: GObject
        {
            return (T)_gCom.GetChild(name);
        }

        protected Controller GetController(string name)
        {
            return _gCom.GetController(name);
        }

        protected Transition GetTransition(string name)
        {
            return _gCom.GetTransition(name);
        }

        public void SetScale(Vector2 scale)
        {
            _gCom.scale = scale;
        }

        public float GetHeight()
        {
            return _gCom.height;
        }

        /// <summary>
        /// 销毁组件
        /// </summary>
        /// <param name="disposeGCom">对于被加载的组件的子组件，disposeGCom为false，跟随父组件的销毁会自动被销毁
        /// 对于被主动加载的组件，disposeGCom为true，需要手动销毁</param>
        public virtual void Dispose(bool disposeGCom = false)
        {
            foreach (var pair in childDic)
            {
                pair.Value.Dispose();
            }
            childDic.Clear();

            if (disposeGCom)
            {
                UIManager.Instance.RemovePackage(GCom.packageItem.owner.name);
                _gCom.Dispose();
            }

            _gCom = null;
            name = null;
        }

        ~UIBase()
        {

        }
    }
}
