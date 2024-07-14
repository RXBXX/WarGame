using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class MapSky : UIBase
    {
        private List<GLoader> _clouds = new List<GLoader>();
        private Stack<GLoader> _cloudStack = new Stack<GLoader>();
        private List<string> _cloudResPool = new List<string>() { "ui://Map/Cloud" };
        private float _interval = 10.0f;
        private float _speed = 30.0f;

        public MapSky(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            Init();
        }

        private void Init()
        {
            for (int i = 0; i < 11; i++)
            {
                var posX = Random.Range(-512, GCom.width);
                var posY = Random.Range(-128, GCom.height);
                var cloud = CreateCloud();
                cloud.url = _cloudResPool[Random.Range(0, _cloudResPool.Count)];
                GCom.AddChild(cloud);
                cloud.xy = new Vector2(posX, posY);

                _clouds.Add(cloud);
            }
        }

        public override void Update(float deltaTime)
        {
            for (int i = _clouds.Count - 1; i >= 0; i--)
            {
                if (_clouds[i].x < -512)
                {
                    _cloudStack.Push(_clouds[i]);
                    _clouds.RemoveAt(i);
                    continue;
                }
                _clouds[i].x = _clouds[i].x - deltaTime * _speed;
            }

            _interval -= deltaTime;
            if (_interval <= 0)
            {
                var posY = Random.Range(-128, GCom.height);
                var cloud = CreateCloud();
                cloud.url = _cloudResPool[Random.Range(0, _cloudResPool.Count)];
                GCom.AddChild(cloud);
                cloud.xy = new Vector2(GCom.width, posY);
                _clouds.Add(cloud);
                _interval = 10.0f;
            }
        }

        private GLoader CreateCloud()
        {
            if (_cloudStack.Count > 0)
                return _cloudStack.Pop();
            else
                return new GLoader();
        }

        public override void Dispose(bool disposeGCom = false)
        {
            for (int i = _clouds.Count - 1; i >= 0; i--)
                _clouds[i].Dispose();
            _clouds.Clear();

            foreach (var v in _cloudStack)
                v.Dispose();
            _cloudStack.Clear();

            base.Dispose(disposeGCom);
        }
    }
}
