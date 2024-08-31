using FairyGUI;

namespace WarGame.UI
{
    public class RecordItem : UIBase
    {
        private string _id;
        private Controller _emptyC;
        private GTextField _title;
        private GTextField _time;
        private GTextField _duration;
        private GTextField _newGame;
        private GLoader _icon;

        public RecordItem(GComponent gCom, string name, object[] args) : base(gCom, name, args)
        {
            _emptyC = GetController("Empty");
            _title = GetGObjectChild<GTextField>("title");
            _time = GetGObjectChild<GTextField>("time");
            _duration = GetGObjectChild<GTextField>("duration");
            _newGame = GetGObjectChild<GTextField>("newGame");
            _icon = GetGObjectChild<GLoader>("icon");

            GetGObjectChild<GButton>("delBtn").onClick.Add(OnClickDelete);
        }

        public void Update(string id)
        {
            _id = id;
            _emptyC.SetSelectedIndex(null == id ? 1 : 0);
            if (null != id)
            {
                var gd = DatasMgr.Instance.GetRecord(id);
                _icon.url = gd.GetIcon();
                _title.text = ConfigMgr.Instance.GetTranslation("Record_Title") + "_" + gd.title;
                _time.text = ConfigMgr.Instance.GetTranslation("RecordItem_Time") + TimeMgr.Instance.GetFormatDateTime(gd.saveTime);
                _duration.text = ConfigMgr.Instance.GetTranslation("RecordItem_Duration") + TimeMgr.Instance.GetFormatLeftTime(gd.duration);
            }
            else
            {
                _newGame.text = "�´浵";
            }
        }

        private void OnClickDelete(EventContext context)
        {
            context.StopPropagation();

            WGCallback cb = () =>
            {
                DatasMgr.Instance.DeleteRecordC2S(_id);
            };
            UIManager.Instance.OpenPanel("Common", "CommonTipsPanel", new object[] { ConfigMgr.Instance.GetTranslation("RecordItem_Tips"), cb});
        }
    }
}
