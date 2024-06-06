using FairyGUI;

namespace WarGame.UI
{
    public class StoryPanel : UIBase
    {
        private GTextField _context;
        private TypingEffect _te;
        private int _groupID;
        private int _index = 0;
        private WGArgsCallback _callback;
        private float _wordInterval = 0.1f;
        private float _paragraphInterval = 1.0f;
        private float _time = 0.0f;

        public StoryPanel(GComponent gCom, string name, object[] args) : base(gCom, name)
        {
            _context = GetGObjectChild<GTextField>("context");
            _te = new TypingEffect(_context);
            _groupID = (int)args[0];
            _callback = (WGArgsCallback)args[1];

            Next();
        }

        public override void Update(float deltaTime)
        {
            if (_time > _wordInterval)
            {
                if (!_te.Print())
                {
                    if (_time > _wordInterval + _paragraphInterval)
                    {
                        Next();
                    }
                }
                else
                {
                    _time = 0;
                }
            }
            _time += deltaTime;
        }

        private void Next()
        {
            _index++;
            if (_index > ConfigMgr.Instance.GetConfig<StoryGroupConfig>("StoryGroupConfig", _groupID).Count)
            {
                _callback();
                return;
            }

            _context.text = ConfigMgr.Instance.GetConfig<StoryConfig>("StoryConfig", _groupID * 1000 + _index).Context;
            _te.Start();
            _te.Print();
        }
    }
}
