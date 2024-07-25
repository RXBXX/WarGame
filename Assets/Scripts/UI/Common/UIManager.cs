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

            //ѡ�������ui�㼶˳���ź�
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

                //��ʼ��ָ����Ĳ㼶����
                _panelDic[layerList[i]] = new List<UIBase>();
            }

            //DebugManager.Instance.Log("Screen:" + Screen.width + "_" + Screen.height);
            //DebugManager.Instance.Log("Stage:" + Stage.inst.width + "_" + Stage.inst.height);
            //DebugManager.Instance.Log("GRoot:" + GRoot.inst.width + "_" + GRoot.inst.height);
            //�������Ų���
            GRoot.inst.SetContentScaleFactor(1134, 750, UIContentScaler.ScreenMatchMode.MatchWidthOrHeight);
            //DebugManager.Instance.Log("Screen:" + Screen.width + "_" + Screen.height);
            //DebugManager.Instance.Log("Stage:" + Stage.inst.width + "_" + Stage.inst.height);
            //DebugManager.Instance.Log("GRoot:" + GRoot.inst.width + "_" + GRoot.inst.height);

            //���ع���ui��
            AddPackage("Common");

            //����Ĭ������
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
        /// ʹ��foreach����һ��Ҫע�⣬��ֹ�ڱ����ڣ�����������List��Dictionary�����仯
        /// ����ʹ��Forѭ���ķ�ʽ��������Ϊ�˱������������������𱨴�
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

            //�ڱ༭ģʽ�����У�StageEngine����OnApplicationQuit�����а�ж��
            if (!Application.isEditor)
            {
                UIPackage.RemoveAllPackages();
                _uiPackagesDic.Clear();
            }
            //UIPackage.RemovePackage("Common");

            return true;
        }

        /// <summary>
        /// ��ȡui����ڵ�
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
                DebugManager.Instance.LogError("û���ҵ��ű���" + classType);
                return null;
            }

            var comp = CreateObject<GComponent>(packageName, compName);
            if (null == comp)
            {
                DebugManager.Instance.LogError("����ʧ�ܣ�" + compName);
                return null;
            }

            var ui = (T)Activator.CreateInstance(classType, new[] { comp, (object)compName, args });

            var layer = GetUILayer(ui.UILayer);
            if (null == layer)
            {
                DebugManager.Instance.LogError("δ�ҵ���Ӧ�㼶��" + compName);
                return null;
            }

            ui.SetParent(layer);
            return ui;
        }

        //֮����ûͨ��Tֱ��������ʵ��������Ϊ��Щ�ط�����ķ���������UIBase������ʵ�ʴ�������Ҫ����������
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
        /// �򿪽���
        /// </summary>
        /// <param name=����></param>
        /// <param name=������></param>
        /// <param name=ָ���㼶></param>
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

            //�����PanelLayer��Ҫ�ظ��ϴ���ջ�Ľ���
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

        //��UI���
        public UIBase OpenComponent(string packageName, string compName, string customName, bool touchEmptyClose = false, Enum.UILayer layerIndex = Enum.UILayer.HUDLayer)
        {
            var comp = CreateUI<UIBase>(packageName, compName);
            comp.name = customName;

            _panelDic[comp.UILayer].Add(comp);

            //�����հ״��ر����
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

        //�����հ�����ر����
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
        /// ���ذ��壬������������
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
        /// ������壬������ü����������������������
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
