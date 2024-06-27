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
        private GLoader _pic;
        private Transition _fadeIn;
        private Transition _fadeOut;
        private Transition _over;
        private bool _stop = true;

        public StoryPanel(GComponent gCom, string name, object[] args) : base(gCom, name)
        {
            _pic = GetGObjectChild<GLoader>("pic");
            _context = GetGObjectChild<GTextField>("context");
            _fadeIn = GetTransition("fadeIn");
            _fadeOut = GetTransition("fadeOut");
            _over = GetTransition("over");
            _te = new TypingEffect(_context);
            _groupID = (int)args[0];
            _callback = (WGArgsCallback)args[1];

            Next();
        }

        public override void Update(float deltaTime)
        {
            if (_stop)
                return;

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
            _stop = true;

            _index++;
            if (_index > ConfigMgr.Instance.GetConfig<StoryGroupConfig>("StoryGroupConfig", _groupID).Count)
            {
                _over.Play(()=> { _callback(); });
                return;
            }

            var storyConfig = ConfigMgr.Instance.GetConfig<StoryConfig>("StoryConfig", _groupID * 1000 + _index);

            if (_index <= 1)
            {
                _pic.url = storyConfig.Pic;
                _fadeIn.Play(() =>
                {
                    _context.text = storyConfig.Context;
                    _te.Start();
                    _te.Print();
                    _stop = false;
                });
            }
            else
            {
                if (null != storyConfig.Pic)
                {
                    _fadeOut.Play(() =>
                    {
                        _pic.url = storyConfig.Pic;
                        _fadeIn.Play(() =>
                        {
                            _context.text = storyConfig.Context;
                            _te.Start();
                            _te.Print();
                            _stop = false;
                        });
                    });
                }
                else
                {
                    _context.text = storyConfig.Context;
                    _te.Start();
                    _te.Print();
                    _stop = false;
                }
            }
        }
    }
}
