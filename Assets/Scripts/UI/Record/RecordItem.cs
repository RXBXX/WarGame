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

        public RecordItem(GComponent gCom, string name, object[] args) : base(gCom, name, args)
        {
            _emptyC = GetController("Empty");
            _title = GetGObjectChild<GTextField>("title");
            _time = GetGObjectChild<GTextField>("time");
            _duration = GetGObjectChild<GTextField>("duration");
            _newGame = GetGObjectChild<GTextField>("newGame");
            GetGObjectChild<GButton>("delBtn").onClick.Add(OnClickDelete);
        }

        public void Update(string id)
        {
            _id = id;
            _emptyC.SetSelectedIndex(null == id ? 1 : 0);
            if (null != id)
            {
                var gd = DatasMgr.Instance.GetRecord(id);
                _title.text = gd.title;
                _time.text = "最后保存时间：" + TimeMgr.Instance.GetFormatDateTime(gd.saveTime);
                _duration.text = "游戏时常：" + TimeMgr.Instance.GetFormatLeftTime(gd.duration);
            }
            else
            {
                _newGame.text = "新存档";
            }
        }

        private void OnClickDelete(EventContext context)
        {
            context.StopPropagation();

            WGCallback cb = () =>
            {
                DatasMgr.Instance.DeleteRecordC2S(_id);
            };
            UIManager.Instance.OpenPanel("Common", "CommonTipsPanel", new object[] {"是否确认要删除该游戏记录？", cb});
        }
    }
}
