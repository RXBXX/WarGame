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

        /// <summary>
        /// ��ȡui����ڵ�
        /// </summary>
        /// <param name="layerIndex"></param>
        /// <returns></returns>
        private GComponent GetUILayer(Enum.UILayer layerIndex)
        {
            GComponent layer = null;
            if (!_uiLayerDic.TryGetValue(layerIndex, out layer))
            {
                layer = new GComponent();
                layer.displayObject.gameObject.name = layerIndex.ToString();
                GRoot._inst.container.AddChild(layer.displayObject);
                _uiLayerDic[layerIndex] = layer;

                //��ʼ��ָ����Ĳ㼶����
                _panelDic[layerIndex] = new List<UIBase>();
            }

            return layer;
        }

        /// <summary>
        /// �򿪽���
        /// </summary>
        /// <param name=����></param>
        /// <param name=������></param>
        /// <param name=ָ���㼶></param>
        public UIBase OpenPanel(string packageName, string panelName)
        {
            UIPackage.AddPackage("UI/" + packageName);

            Type classType = Type.GetType("WarGame.UI." + panelName);
            if (null == classType)
            {
                UnityEngine.Debug.LogError("û���ҵ��ű���" + classType);
                return null;
            }

            var ui = (GComponent)UIPackage.CreateObject(packageName, panelName);
            if (null == ui)
            {
                UnityEngine.Debug.LogError("����ʧ�ܣ�" + classType);
                return null;
            }

            var panel = (UIBase)Activator.CreateInstance(classType, new[] { ui, (object)panelName });

            var layer = GetUILayer(panel.UILayer);
            if (null == ui)
            {
                UnityEngine.Debug.LogError("δ�ҵ���Ӧ�㼶��" + classType);
                return null;
            }

            //ui.position = position;
            //if (scale.x != 0 && scale.y != 0)
            //    ui.scale = scale;
            //ui.rotationX = rotation.x;
            //ui.rotationY = rotation.y;
            //ui.rotation = rotation.z;

            //if (this.container.hitArea != null)
            //{
            //    UpdateHitArea();
            //    ui.onSizeChanged.Add(UpdateHitArea);
            //    ui.onPositionChanged.Add(UpdateHitArea);
            //}
            panel.SetParent(layer);
            //layer.container.AddChild(ui.displayObject);

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
            panel.RemoveParent();
            panel.Dispose(true);

            //UIPackage.
        }

        //��UI���
        public UIBase OpenComponent(string packageName, string compName, string customName, Enum.UILayer layerIndex = Enum.UILayer.HUDLayer)
        {
            UIPackage.AddPackage("UI/" + packageName); //ж���߼���ûд

            Type classType = Type.GetType("WarGame.UI." + compName);
            if (null == classType)
            {
                UnityEngine.Debug.LogError("û���ҵ��ű���" + classType);
                return null;
            }

            var ui = (GComponent)UIPackage.CreateObject(packageName, compName);
            if (null == ui)
            {
                UnityEngine.Debug.LogError("����ʧ�ܣ�" + classType);
                return null;
            }

            var comp = (UIBase)Activator.CreateInstance(classType, new[] { ui, (object)compName });

            var layer = GetUILayer(layerIndex);
            if (null == ui)
            {
                UnityEngine.Debug.LogError("δ�ҵ���Ӧ�㼶��" + classType);
                return null;
            }

            //ui.position = position;
            //if (scale.x != 0 && scale.y != 0)
            //    ui.scale = scale;
            //ui.rotationX = rotation.x;
            //ui.rotationY = rotation.y;
            //ui.rotation = rotation.z;

            //if (this.container.hitArea != null)
            //{
            //    UpdateHitArea();
            //    ui.onSizeChanged.Add(UpdateHitArea);
            //    ui.onPositionChanged.Add(UpdateHitArea);
            //}
            comp.SetParent(layer);
            comp.name = customName;
            //layer.container.AddChild(ui.displayObject);

            _panelDic[comp.UILayer].Add(comp);

            //�����հ״��ر����
            EventCallback1 callback = null;
            callback = (EventContext context) => { TouchEmptyClose(comp.GCom, customName, ()=> { Debug.Log("Remove TouchEmptyClose"); Stage.inst.onTouchBegin.Remove(callback); }); };
            Stage.inst.onTouchBegin.Add(callback);

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
            panel.RemoveParent();
            panel.Dispose(true);

            //UIPackage.
        }

        //�����հ�����ر����
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
