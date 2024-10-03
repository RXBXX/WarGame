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
        private float _wordInterval = 0.05f;
        private float _paragraphInterval = 1.0f;
        private float _time = 0.0f;
        private Transition _over;
        private bool _stop = true;
        private int _soundID = 0;
        private StoryItem _item;

        public StoryPanel(GComponent gCom, string name, object[] args) : base(gCom, name)
        {
            _context = GetGObjectChild<GTextField>("context");
            _over = GetTransition("over");
            _item = GetChild<StoryItem>("item");
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
                    OnStopString();
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
                _over.Play(() => { _callback(); });
                return;
            }

            _item.Play(_groupID * 1000 + _index, OnPlayString);
        }

        private void OnPlayString()
        {
            _soundID = AudioMgr.Instance.PlaySound("Assets/Audios/Print.mp3", true);
            var storyConfig = ConfigMgr.Instance.GetConfig<StoryConfig>("StoryConfig", _groupID * 1000 + _index);
            _context.text = storyConfig.GetTranslation("Context");
            _te.Start();
            _te.Print();
            _stop = false;
        }

        private void OnStopString()
        {
            if (0 != _soundID)
            {
                AudioMgr.Instance.StopSound(_soundID);
                _soundID = 0;
            }
        }
    }
}
