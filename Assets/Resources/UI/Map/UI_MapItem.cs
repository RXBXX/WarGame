/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Map
{
    public partial class UI_MapItem : GComponent
    {
        public GTextField m_title;
        public const string URL = "ui://p7jlxbp3rquy0";

        public static UI_MapItem CreateInstance()
        {
            return (UI_MapItem)UIPackage.CreateObject("Map", "MapItem");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_title = (GTextField)GetChildAt(1);
        }
    }
}