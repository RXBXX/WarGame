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
        private Dictionary<int, UIBase> childDic = new Dictionary<int, UIBase>();

        public GComponent GCom {
            get { return _gCom; }
        }
        public UIBase(GComponent gCom, string name)
        {
            this._gCom = gCom;
            this.name = name;

            var childCount = gCom.numChildren;
            for (int i = 0; i < childCount; i++)
            {
                var childGCom = gCom.GetChildAt(i);
                if (null == childGCom.packageItem)
                    continue;
                var type = Type.GetType(childGCom.name);  //需要验证这是组件packageItem的名称还是当前名称
                if (null == type)
                    continue;
                UIBase child = (UIBase)Activator.CreateInstance(type, new[] { childGCom, (object)childGCom.name });
                childDic[i] = child;
            }
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

        public virtual void Dispose(bool disposeGCom = false)
        {
            Debug.Log("UIBase.Dispose");
            foreach (var pair in childDic)
            {
                childDic[pair.Key].Dispose();
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
