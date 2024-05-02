/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Map
{
    public partial class UI_MapScroll : GComponent
    {
        public GLoader m_map;
        public GButton m_level_0;
        public GButton m_level_1;
        public GButton m_level_2;
        public GButton m_level_3;
        public GButton m_level_4;
        public GButton m_level_5;
        public GButton m_level_6;
        public GButton m_level_7;
        public GButton m_level_8;
        public const string URL = "ui://p7jlxbp3nw174";

        public static UI_MapScroll CreateInstance()
        {
            return (UI_MapScroll)UIPackage.CreateObject("Map", "MapScroll");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_map = (GLoader)GetChildAt(0);
            m_level_0 = (GButton)GetChildAt(1);
            m_level_1 = (GButton)GetChildAt(2);
            m_level_2 = (GButton)GetChildAt(3);
            m_level_3 = (GButton)GetChildAt(4);
            m_level_4 = (GButton)GetChildAt(5);
            m_level_5 = (GButton)GetChildAt(6);
            m_level_6 = (GButton)GetChildAt(7);
            m_level_7 = (GButton)GetChildAt(8);
            m_level_8 = (GButton)GetChildAt(9);
        }
    }
}