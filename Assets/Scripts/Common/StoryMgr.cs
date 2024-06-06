using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;

namespace WarGame
{
    public class StoryMgr : Singeton<StoryMgr>
    {
        private int _id;
        private WGArgsCallback _callback;
        private bool _autoStop;


        public void PlayStory(int id, bool autoStop, WGArgsCallback callback = null)
        {
            _id = id;
            _callback = callback;
            _autoStop = autoStop;

            WGArgsCallback cb = OnStoryEnd;
            UIManager.Instance.OpenPanel("Story", "StoryPanel", new object[] { id, cb });
        }

        public void StopStory()
        {
            if (_id <= 0)
                return;

            _id = 0;
            UIManager.Instance.ClosePanel("StoryPanel");
        }

        private void OnStoryEnd(params object[] args)
        {
            if (null != _callback)
            {
                _callback();
                _callback = null;
            }
            if (_autoStop)
                StopStory();
        }
    }
}
