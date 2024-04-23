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
        public void MarkingRegion(string hexagonID, float moveDis, float attackDis, bool isHero = true)
        {
            //缓存所有的父节点
            var cells = FindingMoveRegion(hexagonID, moveDis, isHero);
            for (int i = 0; i < cells.Count; i++)
            {
                _markedRegion.Add(cells[i].id);
                GetHexagon(cells[i].id).Marking(cells[i].type);

            }
            DebugManager.Instance.Log("MarkingRegion:" + cells.Count);
            cells = FindingAttackRegion(cells, attackDis, isHero);
            for (int i = 0; i < cells.Count; i++)
            {
                _markedRegion.Add(cells[i].id);
                GetHexagon(cells[i].id).Marking(cells[i].type);
            }
            DebugManager.Instance.Log("MarkingRegion:" + cells.Count);
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
                if (i > 0 && cells[i - 1].coor.y != cells[i].coor.y)
                {
                    var start = MapTool.Instance.GetPosFromCoor(cells[i - 1].coor) + new Vector3(0, 0.23F, 0);
                    var end = MapTool.Instance.GetPosFromCoor(cells[i].coor) + new Vector3(0, 0.23F, 0);
                    points.Add(new Vector3((start.x + end.x) / 2, start.y, (start.z + end.z) / 2));
                    points.Add(new Vector3((start.x + end.x) / 2, end.y, (start.z + end.z) / 2));
                    points.Add(end);
                }
                else
                {
                    points.Add(MapTool.Instance.GetPosFromCoor(cells[i].coor) + new Vector3(0, 0.23F, 0));
                }
            }
            LineMgr.Instance.SetLine(points);

        }

        public void ClearMarkedPath()
        {
            LineMgr.Instance.ClearLine();
        }


        /// <summary>
        /// 寻路专用
        /// </summary>
        public class Cell
        {
            public string id;
            public float g; //移动代价
            public float h;
            public Vector3 coor;
            public Cell parent;
            public Enum.MarkType type;

            public Cell(float g, float h, Vector3 pos, Cell parent, string id)
            {
                this.g = g;
                this.h = h;
                this.coor = pos;
                this.parent = parent;
                this.id = id;
            }
        };

        /// <summary>
        /// 寻路专用
        /// </summary>
        private Cell HandleMoveCell(Vector3 cellPos, Vector3 endPos, Cell parent, Dictionary<string, Cell> openDic, Dictionary<string, Cell> closeDic, bool isAttackPath = false, bool isHero = true)
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

            //如果当前地块有敌人，是不可达的，但是攻击距离不受影响
            if (!isAttackPath)
            {
                if (isHero && RoleManager.Instance.GetEnemyIDByHexagonID(key) > 0)
                    return null;
                else if (!isHero && RoleManager.Instance.GetHeroIDByHexagonID(key) > 0)
                    return null;
            }

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
            else if (openDic.ContainsKey(key))
            {
                cell = openDic[key];
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
        public List<Cell> FindingPath(string startHexagonID, string endHexagonID, bool isAttackPath = false, bool isHero = true)
        {
            List<Cell> path = new List<Cell>();
            Dictionary<string, Cell> openDic = new Dictionary<string, Cell>();
            Dictionary<string, Cell> closeDic = new Dictionary<string, Cell>();

            var startPos = GetHexagon(startHexagonID).coordinate;
            var endPos = GetHexagon(endHexagonID).coordinate;
            var cell = HandleMoveCell(startPos, endPos, null, openDic, closeDic, isAttackPath, isHero);
            if (null == cell || cell.coor == endPos)
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
                    var pos2 = c1.coor + _directions[i];
                    var cell2 = HandleMoveCell(pos2, endPos, c1, openDic, closeDic, isAttackPath, isHero);
                    if (null != cell2 && cell2.coor == endPos)
                    {
                        endCell = cell2;
                    }
                }

                var key1 = MapTool.Instance.GetHexagonKey(c1.coor);
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

        /// <summary>
        /// 获取路径通过地块id的形式返回
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        public List<string> FindingPathForStr(string initiatorID, string targetID, bool isAttackPath = false, bool isHero = true)
        {
            var path = FindingPath(initiatorID, targetID, isAttackPath, isHero);
            List<string> hexagons = new List<string>();
            for (int i = 0; i < path.Count; i++)
            {
                hexagons.Add(path[i].id);
            }
            return hexagons;
        }

        /// <summary>
        /// 标记可达区域专用
        /// </summary>
        private Cell HandleMoveRegionCell(Vector3 cellPos, Cell parent, float dis, Dictionary<string, Cell> openDic, Dictionary<string, Cell> closeDic, bool isHero = true)
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
            if (!hexagon.IsReachable())
                return null;

            if (closeDic.ContainsKey(key) || openDic.ContainsKey(key))
                return null;

            if (isHero && RoleManager.Instance.GetEnemyIDByHexagonID(key) > 0)
                return null;

            if (!isHero && RoleManager.Instance.GetHeroIDByHexagonID(key) > 0)
                return null;

            //每个地块的通过代价不一样，这个问题需要考虑
            float cost = 0;
            if (null != parent)
            {
                cost = parent.g + hexagon.GetCost();
            }
            Cell cell = null;
            if (cost <= dis)
            {
                cell = new Cell(cost, 0, cellPos, parent, hexagon.ID);
                cell.type = Enum.MarkType.Walkable;
            }

            if (null != cell)
                openDic.Add(key, cell);

            return cell;
        }

        /// <summary>
        /// 标记可达区域专用
        /// </summary>
        private Cell HandleAttackRegionCell(Vector3 cellPos, Cell parent, float dis, Dictionary<string, Cell> openDic, Dictionary<string, Cell> closeDic, Dictionary<string, Cell> walkableDic, bool isHero = true)
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

            if (closeDic.ContainsKey(key) || openDic.ContainsKey(key) || walkableDic.ContainsKey(key))
                return null;

            //每个地块的通过代价不一样，这个问题需要考虑
            float cost = hexagon.GetCost();
            if (null != parent)
            {
                cost += parent.g;
            }
            Cell cell = null;
            if (cost <= dis)
            {
                cell = new Cell(cost, 0, cellPos, parent, hexagon.ID);
                cell.type = Enum.MarkType.Attachable;
            }

            if (null != cell)
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
        public List<Cell> FindingMoveRegion(string hexagonID, float moveDis, bool isHero = true)
        {
            List<Cell> region = new List<Cell>();

            Dictionary<string, Cell> openDic = new Dictionary<string, Cell>();
            Dictionary<string, Cell> closeDic = new Dictionary<string, Cell>();

            var startPos = GetHexagon(hexagonID).coordinate;
            var cell = HandleMoveRegionCell(startPos, null, moveDis, openDic, closeDic, isHero);
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
                        var pos2 = cell2.coor + _directions[i];
                        HandleMoveRegionCell(pos2, cell2, moveDis, openDic, closeDic, isHero);
                    }
                    openDic.Remove(key);
                    closeDic.Add(key, cell2);
                }
            }

            foreach (var pair in closeDic)
            {
                region.Add(pair.Value);
            }

            openDic.Clear();
            closeDic.Clear();

            return region;
        }

        /// <summary>
        /// 标记可攻击区域
        /// 广度优先
        /// </summary>
        /// <returns></returns>
        public List<Cell> FindingAttackRegion(List<Cell> cells, float attackDis, bool isHero = true)
        {
            Dictionary<string, Cell> openDic = new Dictionary<string, Cell>();
            Dictionary<string, Cell> closeDic = new Dictionary<string, Cell>();
            Dictionary<string, Cell> walkableDic = new Dictionary<string, Cell>();

            var parentCell = new Dictionary<string, bool>();
            for (int i = 0; i < cells.Count; i++)
            {
                if (null != cells[i].parent)
                {
                    parentCell[cells[i].parent.id] = true;
                }
                walkableDic[cells[i].id] = cells[i];
            }

            for (int i = 0; i < cells.Count; i++)
            {
                if (!parentCell.ContainsKey(cells[i].id))
                {
                    for (int j = 0; j < _directions.Length; j++)
                    {
                        var pos2 = cells[i].coor + _directions[j];
                        HandleAttackRegionCell(pos2, null, attackDis, openDic, closeDic, walkableDic, isHero);
                    }
                }
            }

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
                        var pos2 = cell2.coor + _directions[i];
                        HandleAttackRegionCell(pos2, cell2, attackDis, openDic, closeDic, walkableDic, isHero);
                    }
                    openDic.Remove(key);
                    closeDic.Add(key, cell2);
                }
            }

            List<Cell> region = new List<Cell>();
            foreach (var pair in closeDic)
            {
                DebugManager.Instance.Log(pair.Value.g);
                region.Add(pair.Value);
            }

            openDic.Clear();
            closeDic.Clear();
            walkableDic.Clear();

            return region;
        }
    }
}
