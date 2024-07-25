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
        private Dictionary<string, int> _uiPackagesDic = new Dictionary<string, int>();

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

            //DebugManager.Instance.Log("Screen:" + Screen.width + "_" + Screen.height);
            //DebugManager.Instance.Log("Stage:" + Stage.inst.width + "_" + Stage.inst.height);
            //DebugManager.Instance.Log("GRoot:" + GRoot.inst.width + "_" + GRoot.inst.height);
            //设置缩放参数
            GRoot.inst.SetContentScaleFactor(1134, 750, UIContentScaler.ScreenMatchMode.MatchWidthOrHeight);
            //DebugManager.Instance.Log("Screen:" + Screen.width + "_" + Screen.height);
            //DebugManager.Instance.Log("Stage:" + Stage.inst.width + "_" + Stage.inst.height);
            //DebugManager.Instance.Log("GRoot:" + GRoot.inst.width + "_" + GRoot.inst.height);

            //加载公用ui包
            AddPackage("Common");

            //设置默认字体
            UIConfig.defaultFont = "Bangers";

            return true;
        }

        public int GetGRootWidth()
        {
            return 1334;
        }

        public int GetGRootHeight()
        {
            return 750;
        }

        /// <summary>
        /// 使用foreach遍历一定要注意，禁止在遍历内，让所遍历的List或Dictionary发生变化
        /// 这里使用For循环的方式遍历就是为了避免引起上述问题引起报错
        /// InvalidOperationException: Collection was modified; enumeration operation may not execute.
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(float deltaTime)
        {
            foreach (var pair in _panelDic)
            {
                for (int i = pair.Value.Count - 1; i >= 0; i--)
                {
                    if (i <= pair.Value.Count)
                    {
                        pair.Value[i].Update(deltaTime);
                    }
                }
                //foreach (var pair1 in pair.Value)
                //{
                //    pair1.Update(deltaTime);
                //}
            }
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

            //在编辑模式下运行，StageEngine会在OnApplicationQuit把所有包卸载
            if (!Application.isEditor)
            {
                UIPackage.RemoveAllPackages();
                _uiPackagesDic.Clear();
            }
            //UIPackage.RemovePackage("Common");

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

        public T CreateUI<T>(string packageName, string compName, object[] args = null) where T : UIBase
        {
            Type classType = Type.GetType("WarGame.UI." + compName);
            if (null == classType)
            {
                DebugManager.Instance.LogError("没有找到脚本：" + classType);
                return null;
            }

            var comp = CreateObject<GComponent>(packageName, compName);
            if (null == comp)
            {
                DebugManager.Instance.LogError("创建失败：" + compName);
                return null;
            }

            var ui = (T)Activator.CreateInstance(classType, new[] { comp, (object)compName, args });

            var layer = GetUILayer(ui.UILayer);
            if (null == layer)
            {
                DebugManager.Instance.LogError("未找到对应层级：" + compName);
                return null;
            }

            ui.SetParent(layer);
            return ui;
        }

        //之所以没通过T直接来创建实例，是因为有些地方传入的泛型类型是UIBase，但是实际创建的需要是其派生类
        public T CreateUI<T>(string compName, GObject gObj, object[] args = null) where T : UIBase
        {
            Type classType = Type.GetType("WarGame.UI." + compName);
            if (null == classType)
            {
                return null;
            }

            return (T)Activator.CreateInstance(classType, new[] { gObj, (object)compName, args });
        }

        public T CreateObject<T>(string packageName, string compName) where T:GObject
        {
            AddPackage(packageName);
            return (T)UIPackage.CreateObject(packageName, compName);
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
        public UIBase OpenPanel(string packageName, string panelName, params object[] args)
        {
            UIBase panel = null;
            foreach (var panles in _panelDic)
            {
                foreach (var p in panles.Value)
                {
                    if (p.name == panelName)
                    {
                        panel = p;
                        break;
                    }
                }
                if (null != panel)
                    break;
            }

            if (null == panel)
                panel = CreateUI<UIBase>(packageName, panelName, args);
            else
                _panelDic[panel.UILayer].Remove(panel);

            if (panel.UILayer == Enum.UILayer.PanelLayer)
            {
                if (_panelDic[panel.UILayer].Count > 0)
                    _panelDic[panel.UILayer][_panelDic[panel.UILayer].Count - 1].SetVisible(false);
            }

            _panelDic[panel.UILayer].Add(panel);

            panel.OnEnable();

            return panel;
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
                if (null != panel)
                    break;
            }

            if (null == panel)
                return;

            _panelDic[panel.UILayer].Remove(panel);

            //如果是PanelLayer需要回复上次入栈的界面
            if (panel.UILayer == Enum.UILayer.PanelLayer)
            {
                var lastIndex = _panelDic[panel.UILayer].Count - 1;
                if (lastIndex >= 0)
                {
                    _panelDic[panel.UILayer][lastIndex].SetVisible(true);
                    _panelDic[panel.UILayer][lastIndex].OnEnable();
                }
            }

            DestroyUI(panel, true);
        }

        //打开UI组件
        public UIBase OpenComponent(string packageName, string compName, string customName, bool touchEmptyClose = false, Enum.UILayer layerIndex = Enum.UILayer.HUDLayer)
        {
            var comp = CreateUI<UIBase>(packageName, compName);
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

        /// <summary>
        /// 加载包体，并记载引用数
        /// </summary>
        /// <param name="packageName"></param>
        public void AddPackage(string packageName)
        {
            var package = UIPackage.AddPackage("UI/" + packageName);
            if (!_uiPackagesDic.ContainsKey(packageName))
                _uiPackagesDic[packageName] = 0;
            _uiPackagesDic[package.name] ++;
        }

        /// <summary>
        /// 清楚包体，检查引用计数，符合条件就清除包体
        /// </summary>
        /// <param name="packageName"></param>
        public void RemovePackage(string packageName)
        {
            if (!_uiPackagesDic.ContainsKey(packageName))
                return;

            var quoteCount = _uiPackagesDic[packageName];
            quoteCount -= 1;
            if (quoteCount <= 0)
            {
                if (null != UIPackage.GetByName(packageName))
                {
                    UIPackage.RemovePackage(packageName);
                }
                _uiPackagesDic.Remove(packageName);
            }
            else
            {
                _uiPackagesDic[packageName] = quoteCount;
            }
        }
    }
}
