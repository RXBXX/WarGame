using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;
using DG.Tweening;

namespace WarGame.UI
{
    public class DialogBox : UIBase
    {
        private GTextField _context;
        private GLoader _role;
        private Controller _type;
        private TypingEffect _te;
        private bool _start = false;
        private float _interval = 0.1f;
        private float _time = 0.0f;
        private WGEventCallback _callback;
        
        public DialogBox(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            _context = GetUIChild<GTextField>("context");
            _role = GetUIChild<GLoader>("role");
            _type = GetController("type");
            _te = new TypingEffect(_context);
        }

        public override void Update(float deltaTime)
        {
            if (_start)
            {
                if (_time > _interval)
                {
                    if (!_te.Print())
                    {
                        _start = false;
                        _te.Cancel();
                        _callback();
                    }
                    _time = 0;
                }
                _time += deltaTime;
            }
        }

        public void Play(string context, string roleURL, WGEventCallback callback)
        {
            _callback = callback;
            var type = _type.selectedIndex;
            if (0 == type)
                type = 1;
            else
                type = 0;
            _type.SetSelectedIndex(type);
            _role.url = roleURL;
            _context.text = context;
            _te.Start();
            _te.Print();
            _start = true;
        }

        public bool IsPlaying()
        {
            return _start;
        }

        public void Complete()
        {
            _te.Cancel();
        }
    }
}
