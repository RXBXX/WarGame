using System.Collections.Generic;
using FairyGUI;
using System;

namespace WarGame.UI
{
    public class UIBase
    {
        public Enum.UILayer UILayer = Enum.UILayer.PanelLayer;
        public string name;
        protected GComponent _gCom;
        private Dictionary<int, UIBase> childDic = new Dictionary<int, UIBase>();

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
                var type = Type.GetType(childGCom.name);
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

        public void RomoveParent()
        {
            _gCom.displayObject.RemoveFromParent();
        }

        public void SetVisible(bool visible)
        {
            _gCom.visible = visible;
        }

        public void Dispose()
        {
            foreach (var pair in childDic)
            {
                childDic[pair.Key].Dispose();
            }
            childDic.Clear();

            _gCom = null;
            name = null;
        }

        ~UIBase()
        {

        }
    }
}
