/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Map
{
    public partial class UI_MapPanel : GComponent
    {
        public GLoader m_bg;
        public UI_MapScroll m_mapScroll;
        public GButton m_closeBtn;
        public GButton m_heroBtn;
        public const string URL = "ui://p7jlxbp3rquy2";

        public static UI_MapPanel CreateInstance()
        {
            return (UI_MapPanel)UIPackage.CreateObject("Map", "MapPanel");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GLoader)GetChildAt(0);
            m_mapScroll = (UI_MapScroll)GetChildAt(1);
            m_closeBtn = (GButton)GetChildAt(2);
            m_heroBtn = (GButton)GetChildAt(3);
        }
    }
}