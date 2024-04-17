/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace Map
{
    public partial class UI_MapPanel : GComponent
    {
        public GList m_mapList;
        public const string URL = "ui://p7jlxbp3rquy2";

        public static UI_MapPanel CreateInstance()
        {
            return (UI_MapPanel)UIPackage.CreateObject("Map", "MapPanel");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_mapList = (GList)GetChildAt(0);
        }
    }
}