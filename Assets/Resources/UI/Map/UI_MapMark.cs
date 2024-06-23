/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Map
{
    public partial class UI_MapMark : GButton
    {
        public Controller m_showBtn;
        public GImage m_lock;
        public GTextField m_desc;
        public GButton m_goOnBtn;
        public GButton m_restartBtn;
        public const string URL = "ui://p7jlxbp3hctw6";

        public static UI_MapMark CreateInstance()
        {
            return (UI_MapMark)UIPackage.CreateObject("Map", "MapMark");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_showBtn = GetControllerAt(0);
            m_lock = (GImage)GetChildAt(1);
            m_desc = (GTextField)GetChildAt(4);
            m_goOnBtn = (GButton)GetChildAt(5);
            m_restartBtn = (GButton)GetChildAt(6);
        }
    }
}