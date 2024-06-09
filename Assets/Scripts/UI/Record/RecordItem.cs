using FairyGUI;

namespace WarGame.UI
{
    public class RecordItem : UIBase
    {
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
        }

        public void Update(int empty, string title, string duration = null, string time = null)
        {
            _emptyC.SetSelectedIndex(empty);
            if (0 == empty)
            {
                _title.text = title;
                _time.text = "最后保存时间：" + time;
                _duration.text = "游戏时常：" + duration;
            }
            else
            {
                _newGame.text = title;
            }
        }
    }
}
