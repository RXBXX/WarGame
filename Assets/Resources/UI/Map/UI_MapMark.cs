/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Map
{
    public partial class UI_MapMark : GButton
    {
        public Controller m_type;
        public GImage m_lock;
        public GTextField m_desc;
        public const string URL = "ui://p7jlxbp3hctw6";

        public static UI_MapMark CreateInstance()
        {
            return (UI_MapMark)UIPackage.CreateObject("Map", "MapMark");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_type = GetControllerAt(0);
            m_lock = (GImage)GetChildAt(1);
            m_desc = (GTextField)GetChildAt(4);
        }
    }
}