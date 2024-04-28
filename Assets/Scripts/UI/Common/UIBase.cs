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
                var type = Type.GetType("WarGame.UI." + childGCom.packageItem.name);
                if (null == type)
                    continue;
                UIBase child = (UIBase)Activator.CreateInstance(type, new[] { childGCom, (object)childGCom.name, null });
                childDic[childGCom.name] = child;
            }
        }

        public virtual void Update(float deltaTime)
        {

        }

        public void SetParent(GComponent parent, int childIndex = 0)
        {
            parent.container.AddChild(_gCom.displayObject);
        }

        public void RemoveParent()
        {
            _gCom.displayObject.RemoveFromParent();
        }

        public void SetVisible(bool visible)
        {
            _gCom.visible = visible;
        }

        public void SetPosition(Vector2 pos)
        {
            _gCom.position = pos;
        }

        public T GetChild<T>(string name) where T : UIBase
        {
            if (childDic.ContainsKey(name))
                return (T)childDic[name];
            else
                return default(T);
        }

        public virtual void Dispose(bool disposeGCom = false)
        {
            foreach (var pair in childDic)
            {
                pair.Value.Dispose();
            }
            childDic.Clear();

            if (disposeGCom)
                _gCom.Dispose();

            _gCom = null;
            name = null;
        }

        ~UIBase()
        {

        }
    }
}
