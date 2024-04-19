using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;
using FairyGUI;

//管理游戏中地图数据
namespace WarGame
{
    public class MapManager : Singeton<MapManager>
    {
        private Dictionary<string, HexagonCell> _map = new Dictionary<string, HexagonCell>();

        private HexagonCell _curHexagon = null, _lastHexagon = null;

        private UIBase _curInstruct = null;

        //移动中可以跨越高度
        private int _stepHeight = 1;

        public void Dispose()
        {
            foreach (var pair in _map)
            {
                pair.Value.Dispose();
            }
            _map.Clear();

            if (null != _curHexagon)
            {
                _curHexagon.Dispose();
                _curHexagon = null;
            }

            if (null != _lastHexagon)
            {
                _lastHexagon.Dispose();
                _lastHexagon = null;
            }

            if (null != _curInstruct)
            {
                _curInstruct.Dispose();
            }
        }

        public void CreateMap(string mapPath)
        {
            _map = MapTool.Instance.CreateMap(mapPath, GameObject.Find("Root"));
        }

        public bool ContainHexagon(string key)
        {
            return _map.ContainsKey(key);
        }
        public HexagonCell GetHexagon(string key)
        {
            if (ContainHexagon(key))
                return _map[key];
            else
                return null;
        }

        public void SelectHexagon(string key)
        {
            var hexagon = GetHexagon(key);

            if (hexagon == _lastHexagon)
                return;
            if ((hexagon == _curHexagon))
                return;

            hexagon.OnClick();
            hexagon.SetSelected(true);

            if (null == _curHexagon)
            {
                _curHexagon = hexagon;
                MarkReachableHexagon(key);
            }
            else if (null == _lastHexagon)
            {
                _lastHexagon = _curHexagon;
                _curHexagon = hexagon;
                ShowHUD(hexagon, _lastHexagon.position, _curHexagon.position);
            }
            else
            {
                _lastHexagon.SetSelected(false);
                _curHexagon.SetSelected(false);
                CloseHUD();

                _lastHexagon = null;
                _curHexagon = hexagon;
                MarkReachableHexagon(key);
            }
        }

        /// <summary>
        /// 标记可抵达地块
        /// </summary>
        public void MarkReachableHexagon(string key)
        { }

        /// <summary>
        /// 显示操作指令
        /// </summary>
        public void ShowHUD(HexagonCell hexagon, Vector3 startPos, Vector3 endPos)
        {
            var worldPos = MapTool.Instance.FromCellPosToWorldPos(hexagon.position);
            var screenPos = Camera.main.WorldToScreenPoint(worldPos);
            screenPos.y = GRoot._inst.height - screenPos.y;

            _curInstruct = UIManager.Instance.OpenComponent("Map", "MapInstruct", "MapInstruct_Custom");
            _curInstruct.SetPosition(screenPos);

            //Debug.Log("AddEvent");
            //需要写一个事件管理
            ((MapInstruct)_curInstruct).SetDelegate(() =>
            {
                //Debug.Log("FindPath");
                var paths = FindPath(startPos, endPos);
                if (paths.Count > 0)
                {
                    for (int i = 0; i < paths.Count; i++)
                    {
                        //Debug.Log(path[i]);
                        var hexagon = GetHexagon(paths[i]);
                        hexagon.SetSelected(true);
                    }
                    _lastHexagon = null;
                    _curHexagon = null;

                    HeroManager.Instance.SetHero(GameObject.Find("Hero"));
                    HeroManager.Instance.Move(paths);
                }
                else
                {
                    if (null != _lastHexagon)
                    {
                        _lastHexagon.SetSelected(false);
                        _lastHexagon = null;
                    }
                    if (null != _curHexagon)
                    {
                        _curHexagon.SetSelected(false);
                        _curHexagon = null;
                    }
                }
                CloseHUD();
            },
            () =>
            {
                if (null != _lastHexagon)
                {
                    _lastHexagon.SetSelected(false);
                    _lastHexagon = null;
                }
                if (null != _curHexagon)
                {
                    _curHexagon.SetSelected(false);
                    _curHexagon = null;
                }
            });
        }

        /// <summary>
        /// 显示操作指令
        /// </summary>
        public void CloseHUD()
        {
            UIManager.Instance.CloseComponent("MapInstruct_Custom");
        }


        /// <summary>
        /// 寻路专用
        /// </summary>
        private class Cell
        {
            public float g;
            public float h;
            public Vector3 pos;
            public Cell parent;

            public Cell(float g, float h, Vector3 pos)
            {
                this.g = g;
                this.h = h;
                this.pos = pos;
            }
        };

        /// <summary>
        /// 寻路专用
        /// </summary>
        private Cell HandleCell(Vector3 cellPos, Vector3 endPos, Cell parent, Dictionary<string, Cell> openDic, Dictionary<string, Cell> closeDic)
        {
            //如果当前位置可跨越高度正上方有地块，证明该地块不可达
            var topKey = MapTool.Instance.GetHexagonKey(cellPos + new Vector3(0, _stepHeight + 1, 0));
            var upHexagon = GetHexagon(topKey);
            if (null != upHexagon)
                return null;

            //从上往下找当前位置的可抵达地块
            for (int i = (int)cellPos.y + _stepHeight; i >= cellPos.y - _stepHeight; i--)
            {
                if (ContainHexagon(MapTool.Instance.GetHexagonKey(new Vector3(cellPos.x, i, cellPos.z))))
                {
                    cellPos.y = i;
                    break;
                }
            }

            var key = MapTool.Instance.GetHexagonKey(cellPos);
            if (!ContainHexagon(key))
                return null;
            var hexagon = GetHexagon(key);
            if (!hexagon.IsReachable() || closeDic.ContainsKey(key))
                return null;

            Cell cell = null;
            if (cellPos == endPos)
            {
                cell = new Cell(hexagon.GetCost() + parent.g, Vector3.Distance(cellPos, endPos), cellPos);
                cell.parent = parent;
            }
            else if (openDic.TryGetValue(key, out cell))
            {
                float parentG = 0;
                if (null != parent)
                    parentG = parent.g;

                if (cell.g > hexagon.GetCost() + parentG)
                {
                    cell.g = hexagon.GetCost() + parentG;
                    cell.parent = parent;
                }
            }
            else
            {
                float parentG = 0;
                if (null != parent)
                    parentG = parent.g;
                cell = new Cell(hexagon.GetCost() + parentG, Vector3.Distance(cellPos, endPos), cellPos);
                cell.parent = parent;
                openDic.Add(key, cell);
            }
            return cell;
        }

        public List<string> FindPath(Vector3 startPos, Vector3 endPos)
        {
            List<string> pathList = new List<string>();
            Dictionary<string, Cell> openDic = new Dictionary<string, Cell>();
            Dictionary<string, Cell> closeDic = new Dictionary<string, Cell>();

            var cell = HandleCell(startPos, endPos, null, openDic, closeDic);
            if (null == cell || cell.pos == endPos)
                return pathList;

            var hexagon = GetHexagon(MapTool.Instance.GetHexagonKey(endPos));
            if (null == hexagon || !hexagon.IsReachable())
                return pathList;


            //可移动方向
            Vector3[] directions = new Vector3[] {
            new Vector3(1, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(-1, 0, 1),
            new Vector3(-1, 0, 0),
            new Vector3(0, 0, -1),
            new Vector3(1, 0, -1),
            };



            //F = G + H
            Cell endCell = null;
            while (openDic.Count > 0 && null == endCell)
            {
                Cell c1 = null;
                foreach (var pair in openDic)
                {
                    if (null == c1 || c1.h > pair.Value.h)
                    {
                        c1 = pair.Value;
                    }
                }
                //Debug.Log(c1.pos + " _ " + GetHexagon(MapTool.Instance.GetHexagonKey(c1.pos)).IsReachable());

                for (int i = 0; i < directions.Length; i++)
                {
                    var pos2 = c1.pos + directions[i];
                    var cell2 = HandleCell(pos2, endPos, c1, openDic, closeDic);
                    if (null != cell2 && cell2.pos == endPos)
                    {
                        endCell = cell2;
                    }

                    //var key2 = MapTool.Instance.GetHexagonKey(pos2);
                    //var hexagon2 = GetHexagon(key2);
                    //if (null == hexagon2 || !hexagon2.IsReachable() || closeDic.ContainsKey(key2))
                    //    continue;

                    //Cell c2 = null;
                    //if (pos2 == endPos)
                    //{
                    //    c2 = new Cell(hexagon2.GetCost() + c1.g, Vector3.Distance(pos2, endPos), pos2);
                    //    c2.parent = c1;
                    //    endCell = c2;
                    //    break;
                    //}
                    //else if (openDic.TryGetValue(key2, out c2))
                    //{
                    //    if (c2.g > hexagon2.GetCost() + c1.g)
                    //    {
                    //        c2.g = hexagon2.GetCost() + c1.g;
                    //        c2.parent = c1;
                    //    }
                    //}
                    //else
                    //{
                    //    c2 = new Cell(hexagon2.GetCost() + c1.g, Vector3.Distance(pos2, endPos), pos2);
                    //    c2.parent = c1;
                    //    openDic.Add(key2, c2);
                    //}

                }

                var key1 = MapTool.Instance.GetHexagonKey(c1.pos);
                openDic.Remove(key1);
                closeDic.Add(key1, c1);
            }

            while (null != endCell)
            {
                pathList.Add(MapTool.Instance.GetHexagonKey(endCell.pos));
                endCell = endCell.parent;
            }

            return pathList;
        }
    }
}
