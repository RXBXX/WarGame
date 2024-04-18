using FairyGUI;
using System.Collections.Generic;
using System;

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
        public void OpenPanel(string packageName, string panelName)
        {
            UIPackage.AddPackage("UI/" + packageName);

            Type classType = Type.GetType("WarGame.UI." + panelName);
            if (null == classType)
            {
                UnityEngine.Debug.LogError("û���ҵ��ű���" + classType);
                return;
            }

            var ui = (GComponent)UIPackage.CreateObject(packageName, panelName);
            if (null == ui)
                return;

            var panel = (UIBase)Activator.CreateInstance(classType, new[] { ui, (object)panelName });

            var layer = GetUILayer(panel.UILayer);
            if (null == ui)
                return;

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
            panel.RomoveParent();
            panel.Dispose();

            //UIPackage.
        }
    }
}
