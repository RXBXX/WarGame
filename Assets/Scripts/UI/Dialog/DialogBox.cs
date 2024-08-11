using FairyGUI;

namespace WarGame.UI
{
    public class DialogBox : UIBase
    {
        private GTextField _context;
        private GButton _role;
        private Controller _type;
        private TypingEffect _te;
        private bool _start = false;
        private float _interval = 0.1f;
        private float _time = 0.0f;
        private WGArgsCallback _callback;
        private int _lastRole = 0;
        private int _soundID = 0;
        private GTextField _name;

        public DialogBox(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _context = GetGObjectChild<GTextField>("context");
            _role = GetGObjectChild<GButton>("role");
            _type = GetController("type");
            _te = new TypingEffect(_context);
            _name = GetGObjectChild<GTextField>("name");
        }

        public override void Update(float deltaTime)
        {
            if (!_start)
                return;

            if (_time > _interval)
            {
                if (!_te.Print())
                {
                    OnComplete();
                }
                _time = 0;
            }

            _time += deltaTime;
        }

        public void Play(string context, int roleID, WGArgsCallback callback)
        {
            _callback = callback;

            if (0 != _lastRole && _lastRole != roleID)
            {
                var type = _type.selectedIndex == 0 ? 1 : 0;
                _type.SetSelectedIndex(type);
            }
            _lastRole = roleID;
            var roleConfig = ConfigMgr.Instance.GetConfig<RoleConfig>("RoleConfig", roleID);
            _role.icon = roleConfig.FullLengthIcon;
            _name.text = roleConfig.GetTranslation("Name");
            _context.text = context;
            _te.Start();
            _te.Print();
            _start = true;
            _time = 0;
            _soundID = AudioMgr.Instance.PlaySound("Assets/Audios/Print.mp3", true);
        }

        public bool IsPlaying()
        {
            return _start;
        }

        public void Complete()
        {
            _te.Cancel();
        }

        private void OnComplete()
        {
            if (0 != _soundID)
            {
                AudioMgr.Instance.StopSound(_soundID);
                _soundID = 0;
            }
            _start = false;
            _te.Cancel();
            _callback();
        }

        public override void Dispose(bool disposeGCom = false)
        {
            if (0 != _soundID)
            {
                AudioMgr.Instance.StopSound(_soundID);
                _soundID = 0;
            }
            base.Dispose(disposeGCom);
        }
    }
}
