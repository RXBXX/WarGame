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

        private List<string> _markedRegion = new List<string>();
        private List<string> _markedPath = new List<string>();

        //移动中可以跨越高度
        private int _stepHeight = 1;

        //可移动方向
        private Vector3[] _directions = new Vector3[] {
            new Vector3(1, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(-1, 0, 1),
            new Vector3(-1, 0, 0),
            new Vector3(0, 0, -1),
            new Vector3(1, 0, -1),
            };

        public override bool Init()
        {
            base.Init();

            return true;
        }

        public override bool Dispose()
        {
            base.Dispose();

            ClearMap();

            return true;
        }

        public void CreateMap(string mapPath)
        {
            _map = MapTool.Instance.CreateMap(mapPath, GameObject.Find("Root"));
        }

        public void ClearMap()
        {
            ClearMarkedPath();
            ClearMarkedRegion();

            foreach (var pair in _map)
            {
                pair.Value.Dispose();
            }
            _map.Clear();
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

        /// <summary>
        /// 标记可抵达地块
        /// </summary>
        public void MarkingRegion(string hexagonID, float moveDis, float attackDis)
        {
            var cells = FindingRegion(hexagonID, moveDis, attackDis);
            for (int i = 1; i < cells.Count; i++)
            {
                var cell = cells[i];
                _markedRegion.Add(cell.ID);
                GetHexagon(cell.ID).Marking(cell.type);
            }
        }
        public void ClearMarkedRegion()
        {
            foreach (var key1 in _markedRegion)
            {
                GetHexagon(key1).Marking(Enum.MarkType.None);
            }
            _markedRegion.Clear();
        }

        /// <summary>
        /// 标记路径
        /// </summary>
        public void MarkingPath(string initiatorID, string targetID, float moveDis)
        {
            ClearMarkedPath();

            var cells = FindingPath(initiatorID, targetID);
            if (cells.Count <= 0)
                return;

            if (cells[cells.Count - 1].g > moveDis)
                return;

            //标记路径
            var points = new List<Vector3>();
            for (int i = 0; i < cells.Count; i++)
            {
                points.Add(MapTool.Instance.GetPosFromCoor(cells[i].pos) + new Vector3(0, 0.23F, 0));
            }
            LineMgr.Instance.SetLine(points);

        }

        public void ClearMarkedPath()
        {
            foreach (var key1 in _markedPath)
            {
                GetHexagon(key1).Marking(Enum.MarkType.None);
            }
            _markedPath.Clear();
        }


        /// <summary>
        /// 寻路专用
        /// </summary>
        public class Cell
        {
            public string ID;
            public float g;
            public float h;
            public Vector3 pos;
            public Cell parent;
            public Enum.MarkType type;

            public Cell(float g, float h, Vector3 pos, Cell parent, string id)
            {
                this.g = g;
                this.h = h;
                this.pos = pos;
                this.parent = parent;
                this.ID = id;
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

            var cost = 0.0f;
            if (null != parent)
                cost = hexagon.GetCost() + parent.g;

            Cell cell = null;
            if (cellPos == endPos)
            {
                cell = new Cell(cost, Vector3.Distance(cellPos, endPos), cellPos, parent, hexagon.ID);
                cell.type = Enum.MarkType.Selected;
                openDic.Add(key, cell);
            }
            else if (openDic.TryGetValue(key, out cell))
            {
                if (cell.g > cost)
                {
                    cell.g = cost;
                    cell.parent = parent;
                }
            }
            else
            {
                cell = new Cell(cost, Vector3.Distance(cellPos, endPos), cellPos, parent, hexagon.ID);
                cell.type = Enum.MarkType.Selected;
                openDic.Add(key, cell);
            }
            return cell;
        }

        /// <summary>
        /// 寻路
        /// 深度优先
        /// </summary>
        /// <param name="startHexagonID"></param>
        /// <param name="endHexagonID"></param>
        /// <returns></returns>
        public List<Cell> FindingPath(string startHexagonID, string endHexagonID)
        {
            List<Cell> path = new List<Cell>();
            Dictionary<string, Cell> openDic = new Dictionary<string, Cell>();
            Dictionary<string, Cell> closeDic = new Dictionary<string, Cell>();

            var startPos = GetHexagon(startHexagonID).coordinate;
            var endPos = GetHexagon(endHexagonID).coordinate;
            var cell = HandleCell(startPos, endPos, null, openDic, closeDic);
            if (null == cell || cell.pos == endPos)
                return path;

            var hexagon = GetHexagon(MapTool.Instance.GetHexagonKey(endPos));
            if (null == hexagon || !hexagon.IsReachable())
                return path;

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

                for (int i = 0; i < _directions.Length; i++)
                {
                    var pos2 = c1.pos + _directions[i];
                    var cell2 = HandleCell(pos2, endPos, c1, openDic, closeDic);
                    if (null != cell2 && cell2.pos == endPos)
                    {
                        endCell = cell2;
                    }
                }

                var key1 = MapTool.Instance.GetHexagonKey(c1.pos);
                openDic.Remove(key1);
                closeDic.Add(key1, c1);
            }

            while (null != endCell)
            {
                path.Add(endCell);
                endCell = endCell.parent;
            }
            path.Reverse();

            return path;
        }

        public List<string> FindingPath2(string initiatorID, string targetID)
        {
            var path = FindingPath(initiatorID, targetID);
            List<string> hexagons = new List<string>();
            for (int i = 0; i < path.Count; i++)
            {
                hexagons.Add(path[i].ID);
            }
            return hexagons;
        }

        /// <summary>
        /// 标记可达区域专用
        /// </summary>
        private Cell HandleCell2(Vector3 cellPos, Cell parent, float moveDis, float attackDis, Dictionary<string, Cell> openDic, Dictionary<string, Cell> closeDic)
        {
            DebugManager.Instance.Log("MapManager.HandleCell2 start");
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
            if (!hexagon.IsReachable() || closeDic.ContainsKey(key) || openDic.ContainsKey(key))
                return null;

            //每个地块的通过代价不一样，这个问题需要考虑
            float cost = 0;
            if (null != parent)
            {
                cost = parent.g + hexagon.GetCost();
            }
            Cell cell = new Cell(cost, 0, cellPos, parent, hexagon.ID);

            if (cell.g > (moveDis + attackDis))
                return null;
            else if (cell.g > moveDis)
                cell.type = Enum.MarkType.Attachable;
            else
                cell.type = Enum.MarkType.Walkable;

            openDic.Add(key, cell);
            return cell;
        }


        /// <summary>
        /// 标记可达区域
        /// 广度优先
        /// </summary>
        /// <param name="hexagonID">起点地块</param>
        /// <param name="moveDis">可移动距离</param>
        /// /// <param name="moveDis">可攻击距离</param>
        /// <returns></returns>
        public List<Cell> FindingRegion(string hexagonID, float moveDis, float attackDis)
        {
            List<Cell> region = new List<Cell>();

            Dictionary<string, Cell> openDic = new Dictionary<string, Cell>();
            Dictionary<string, Cell> closeDic = new Dictionary<string, Cell>();

            var startPos = GetHexagon(hexagonID).coordinate;
            var cell = HandleCell2(startPos, null, moveDis, attackDis, openDic, closeDic);
            if (null == cell)
                return region;

            while (openDic.Count > 0)
            {
                var allKeys = new List<string>();
                foreach (var pair in openDic)
                {
                    allKeys.Add(pair.Key);
                }
                foreach (var key in allKeys)
                {
                    var cell2 = openDic[key];
                    for (int i = 0; i < _directions.Length; i++)
                    {
                        var pos2 = cell2.pos + _directions[i];
                        HandleCell2(pos2, cell2, moveDis, attackDis, openDic, closeDic);
                    }
                    openDic.Remove(key);
                    closeDic.Add(key, cell2);
                }
            }

            foreach (var pair in closeDic)
            {
                region.Add(pair.Value);
            }

            return region;
        }
    }
}
