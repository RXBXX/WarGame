/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Map
{
    public partial class UI_MapScroll : GComponent
    {
        public GLoader m_map;
        public UI_MapMark m_level_0;
        public const string URL = "ui://p7jlxbp3nw174";

        public static UI_MapScroll CreateInstance()
        {
            return (UI_MapScroll)UIPackage.CreateObject("Map", "MapScroll");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_map = (GLoader)GetChildAt(0);
            m_level_0 = (UI_MapMark)GetChildAt(1);
        }
    }
}