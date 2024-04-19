/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Map
{
    public partial class UI_MapInstruct : GComponent
    {
        public GButton m_navigationBtn;
        public const string URL = "ui://p7jlxbp3nr693";

        public static UI_MapInstruct CreateInstance()
        {
            return (UI_MapInstruct)UIPackage.CreateObject("Map", "MapInstruct");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_navigationBtn = (GButton)GetChildAt(1);
        }
    }
}