using FairyGUI;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace WarGame.UI
{
    /// <summary>
    /// UI
    /// </summary>
    public class UIManager : Singeton<UIManager>
    {
        private Dictionary<Enum.UILayer, GComponent> _uiLayerDic = new Dictionary<Enum.UILayer, GComponent>();
        private Dictionary<Enum.UILayer, List<UIBase>> _panelDic = new Dictionary<Enum.UILayer, List<UIBase>>();

        public override bool Init()
        {
            base.Init();

            var layerList = new List<Enum.UILayer>();
            foreach (var v in typeof(Enum.UILayer).GetEnumValues())
            {
                layerList.Add((Enum.UILayer)v);
            }

            //选择排序把ui层级顺序排好
            int minIndex;
            Enum.UILayer temp;
            for (int i = 0; i < layerList.Count; i++)
            {
                minIndex = i;
                for (int j = i + 1; j < layerList.Count; j++)
                {
                    if (layerList[j] < layerList[minIndex])
                        minIndex = j;
                }
                if (minIndex != i)
                {
                    temp = layerList[minIndex];
                    layerList[minIndex] = layerList[i];
                    layerList[i] = temp;
                }
            }

            for (int i = 0; i < layerList.Count; i++)
            {
                GComponent layerGCom = new GComponent();
                layerGCom.displayObject.gameObject.name = layerList[i].ToString();
                GRoot._inst.container.AddChild(layerGCom.displayObject);
                _uiLayerDic[layerList[i]] = layerGCom;

                //初始化指定层的层级管理
                _panelDic[layerList[i]] = new List<UIBase>();
            }

            return true;
        }

        public override bool Dispose()
        {
            base.Dispose();

            foreach (var pair in _uiLayerDic)
            {
                pair.Value.Dispose();
            }
            _uiLayerDic.Clear();

            foreach (var pair in _panelDic)
            {
                foreach (var pair1 in pair.Value)
                {
                    pair1.Dispose();
                }
                pair.Value.Clear();
            }
            _panelDic.Clear();

            return true;
    }

        /// <summary>
        /// 获取ui层根节点
        /// </summary>
        /// <param name="layerIndex"></param>
        /// <returns></returns>
        private GComponent GetUILayer(Enum.UILayer layerIndex)
        {
            return _uiLayerDic[layerIndex];
        }

        public UIBase CreateUI(string packageName, string compName)
        {
            UIPackage.AddPackage("UI/" + packageName);

            Type classType = Type.GetType("WarGame.UI." + compName);
            if (null == classType)
            {
                UnityEngine.Debug.LogError("没有找到脚本：" + classType);
                return null;
            }

            var comp = (GComponent)UIPackage.CreateObject(packageName, compName);
            if (null == comp)
            {
                UnityEngine.Debug.LogError("创建失败：" + classType);
                return null;
            }

            var ui = (UIBase)Activator.CreateInstance(classType, new[] { comp, (object)compName });

            var layer = GetUILayer(ui.UILayer);
            if (null == comp)
            {
                UnityEngine.Debug.LogError("未找到对应层级：" + classType);
                return null;
            }

            ui.SetParent(layer);
            return ui;
        }

        public void DestroyUI(UIBase ui, bool isPanel)
        {
            ui.RemoveParent();
            ui.Dispose(isPanel);
        }

        /// <summary>
        /// 打开界面
        /// </summary>
        /// <param name=包名></param>
        /// <param name=界面名></param>
        /// <param name=指定层级></param>
        public UIBase OpenPanel(string packageName, string panelName)
        {
            //UIPackage.AddPackage("UI/" + packageName);

            //Type classType = Type.GetType("WarGame.UI." + panelName);
            //if (null == classType)
            //{
            //    UnityEngine.Debug.LogError("没有找到脚本：" + classType);
            //    return null;
            //}

            //var ui = (GComponent)UIPackage.CreateObject(packageName, panelName);
            //if (null == ui)
            //{
            //    UnityEngine.Debug.LogError("创建失败：" + classType);
            //    return null;
            //}

            //var panel = (UIBase)Activator.CreateInstance(classType, new[] { ui, (object)panelName });

            //var layer = GetUILayer(panel.UILayer);
            //if (null == ui)
            //{
            //    UnityEngine.Debug.LogError("未找到对应层级：" + classType);
            //    return null;
            //}

            //panel.SetParent(layer);
            var panel = CreateUI(packageName, panelName);

            if (panel.UILayer == Enum.UILayer.PanelLayer)
            {
                if (_panelDic[panel.UILayer].Count > 0)
                    _panelDic[panel.UILayer][_panelDic[panel.UILayer].Count - 1].SetVisible(false);
            }

            _panelDic[panel.UILayer].Add(panel);

            return panel;
            //GRoot._inst.container.AddChild(ui.displayObject);
        }

        public void ClosePanel(string panelName)
        {
            UIBase panel = null;
            foreach (var pair_1 in _panelDic)
            {
                foreach (var pair_2 in pair_1.Value)
                {
                    if (pair_2.name == panelName)
                    {
                        panel = pair_2;
                        break;
                    }
                }
            }

            if (null == panel)
                return;

            _panelDic[panel.UILayer].Remove(panel);

            //如果是PanelLayer需要回复上次入栈的界面
            if (panel.UILayer == Enum.UILayer.PanelLayer)
            {
                var lastIndex = _panelDic[panel.UILayer].Count - 1;
                if (lastIndex >= 0)
                    _panelDic[panel.UILayer][lastIndex].SetVisible(true);
            }

            //panel.RemoveParent();
            //panel.Dispose(true);
            DestroyUI(panel, true);
        }

        //打开UI组件
        public UIBase OpenComponent(string packageName, string compName, string customName, bool touchEmptyClose = false, Enum.UILayer layerIndex = Enum.UILayer.HUDLayer)
        {
            //UIPackage.AddPackage("UI/" + packageName); //卸载逻辑还没写

            //Type classType = Type.GetType("WarGame.UI." + compName);
            //if (null == classType)
            //{
            //    UnityEngine.Debug.LogError("没有找到脚本：" + classType);
            //    return null;
            //}

            //var ui = (GComponent)UIPackage.CreateObject(packageName, compName);
            //if (null == ui)
            //{
            //    UnityEngine.Debug.LogError("创建失败：" + classType);
            //    return null;
            //}

            //var comp = (UIBase)Activator.CreateInstance(classType, new[] { ui, (object)compName });

            //var layer = GetUILayer(layerIndex);
            //if (null == ui)
            //{
            //    UnityEngine.Debug.LogError("未找到对应层级：" + classType);
            //    return null;
            //}

            //comp.SetParent(layer);
            var comp = CreateUI(packageName, compName);
            comp.name = customName;

            _panelDic[comp.UILayer].Add(comp);

            //触摸空白处关闭组件

            if (touchEmptyClose)
            {
                EventCallback1 callback = null;
                callback = (EventContext context) => { TouchEmptyClose(comp.GCom, customName, () => { Debug.Log("Remove TouchEmptyClose"); Stage.inst.onTouchBegin.Remove(callback); }); };
                Stage.inst.onTouchBegin.Add(callback);
            }

            return comp;
        }

        public void CloseComponent(string customName)
        {
            UIBase panel = null;
            foreach (var pair_1 in _panelDic)
            {
                foreach (var pair_2 in pair_1.Value)
                {
                    if (pair_2.name == customName)
                    {
                        panel = pair_2;
                        break;
                    }
                }
            }

            if (null == panel)
                return;

            _panelDic[panel.UILayer].Remove(panel);

            DestroyUI(panel, true);
        }

        //触摸空白区域关闭组件
        private void TouchEmptyClose(GComponent gCom, string customName, EventCallback0 callback)
        {
            Debug.Log("Execute TouchEmptyClose");
            var hitTarget = Stage.inst.touchTarget;
            while (null != hitTarget && hitTarget != gCom.displayObject)
            {
                hitTarget = hitTarget.parent;
            }
            if (null == hitTarget)
            {
                callback();

                UIManager.Instance.CloseComponent(customName);
            }
        }
    }
}
