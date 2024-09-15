using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FairyGUI;

namespace WarGame.UI
{
    public class FightHeroGroup : UIBase
    {
        private Vector2[] _directions = new Vector2[6] {
        new Vector2(0, 1),
        new Vector2(1, 0),
        new Vector2(1, -1),
        new Vector2(0, -1),
        new Vector2(-1, 0),
        new Vector2(-1, 1)
    };

        private float _outsideDiameter = 84.0f;
        private float _radian = 30.0f / 360.0f * 2.0f * Mathf.PI;

        private Dictionary<string, int> _heroDic = new Dictionary<string, int>();
        private Dictionary<string, GButton> _heroUIDic = new Dictionary<string, GButton>();

        public FightHeroGroup(GComponent gCom, string customName, object[] args) : base(gCom, customName, args)
        {
            GCom.onTouchEnd.Add(() =>
            {
                foreach (var v in _heroUIDic)
                {
                    if (v.Value.displayObject == Stage.inst.touchTarget)
                    {
                        EventDispatcher.Instance.PostEvent(Enum.Event.Fight_Change_Hero, new object[] {_heroDic[v.Key]});
                        break;
                    }
                }
            });
        }

        public void Show(Vector2 centerPos, List<int> heros)
        {
            if (GCom.visible)
                return;

            AudioMgr.Instance.PlaySound("Assets/Audios/Show.mp3");
            SetPosition(centerPos);

            _heroDic.Clear();

            var openList = new List<Vector2> { Vector2.zero};
            var startIndex = 0;
            var endIndex = 6;
            var circle = 0;
            while(true)
            {
                for (int j = startIndex; j < endIndex; j++)
                {
                    foreach (var v in _directions)
                    {
                        var coor = openList[j] + v;
                        if (openList.Contains(coor))
                            continue;
                        openList.Add(coor);

                        GButton ui = null;
                        if (_heroUIDic.ContainsKey(coor.x + "_" + coor.y))
                        {
                            ui = _heroUIDic[coor.x + "_" + coor.y];
                        }
                        else
                        {
                            ui = UIManager.Instance.CreateObject<GButton>("Common", "CommonHero");
                            GCom.AddChild(ui);
                            ui.asButton.title = coor.x + "_" + coor.y;

                            _heroUIDic.Add(coor.x + "_" + coor.y, ui);
                        }
                        var uiPosX = coor.x * (_outsideDiameter / 2 + Mathf.Sin(_radian) * _outsideDiameter / 2) - _outsideDiameter / 2;
                        var uiPosY = Mathf.Cos(_radian) * _outsideDiameter * coor.y + coor.x * Mathf.Cos(_radian) * _outsideDiameter * Mathf.Sin(_radian) - _outsideDiameter / 2;
                        ui.xy = - new Vector2(_outsideDiameter / 2, _outsideDiameter / 2);
                        ui.TweenMove(new Vector2(uiPosX, uiPosY), 0.1f).SetDelay(circle * 0.1F);

                        var roleData = DatasMgr.Instance.GetRoleData(heros[openList.Count - 2]);
                        var config = roleData.GetConfig();
                        ui.icon = config.Icon;
                        _heroDic.Add(coor.x + "_" + coor.y, roleData.UID);
                        if (openList.Count > heros.Count)
                            break;
                    }

                    if (openList.Count > heros.Count)
                        break;
                }

                if (openList.Count > heros.Count)
                    break;

                startIndex = endIndex;
                endIndex = openList.Count;
                circle += 1;
            }

            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }
    }
}
