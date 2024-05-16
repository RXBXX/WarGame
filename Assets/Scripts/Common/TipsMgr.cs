using WarGame.UI;
using FairyGUI;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class TipsMgr : Singeton<TipsMgr>
    {
        private List<TipsItem> _tipsList;
        private List<TipsItem> _tipsRecyclePool;
        private Vector2 _startPos;

        public override bool Init()
        {
            _tipsList = new List<TipsItem>();
            _tipsRecyclePool = new List<TipsItem>();
            _startPos = new Vector2(GRoot.inst.width * 0.5F, GRoot.inst.height * 0.2F);
            return true;
        }

        public void Add(string desc)
        {
            var item = GetItem();
            item.SetPosition(_startPos + new Vector2(0, _tipsList.Count * item.GetHeight()));
            _tipsList.Add(item);
            item.Show(desc, OnTipsOver);
        }

        private TipsItem GetItem()
        {
            if (_tipsRecyclePool.Count > 0)
            {
                var item = _tipsRecyclePool[0];
                _tipsRecyclePool.RemoveAt(0);
                return item;
            }

            return UIManager.Instance.CreateUI<TipsItem>("Tips", "TipsItem");
        }

        private void OnTipsOver(params object[] args)
        {
            var item = (TipsItem)args[0];
            _tipsList.Remove(item);
            _tipsRecyclePool.Add(item);

            for (int i = 0; i < _tipsList.Count; i++)
            {
                _tipsList[i].SetPosition(_startPos + new Vector2(0, i * _tipsList[i].GetHeight()));
            }
        }

        public override bool Dispose()
        {
            if (null != _tipsList)
            {
                for (int i = 0; i < _tipsList.Count; i++)
                {
                    _tipsList[i].Dispose(true);
                }
                _tipsList.Clear();
            }

            if (null != _tipsRecyclePool)
            {
                for (int i = 0; i < _tipsRecyclePool.Count; i++)
                {
                    _tipsRecyclePool[i].Dispose(true);
                }
                _tipsRecyclePool.Clear();
            }

            return base.Dispose();
        }
    }
}
