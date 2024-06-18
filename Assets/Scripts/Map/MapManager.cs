using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;
using FairyGUI;

//管理游戏中地图数据
namespace WarGame
{
    public class MapManager : Singeton<MapManager>
    {
        private Dictionary<string, Hexagon> _map = new Dictionary<string, Hexagon>();
        private Dictionary<int, Bonfire> _bonfiresDic = new Dictionary<int, Bonfire>();

        private List<string> _markedRegion = new List<string>();

        private int _roleHeight = 5;

        //可移动方向
        private Vector3[] _directions = new Vector3[] {
            new Vector3(1, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(-1, 0, 1),
            new Vector3(-1, 0, 0),
            new Vector3(0, 0, -1),
            new Vector3(1, 0, -1),

            //new Vector3(1, -1, 0),
            //new Vector3(0, -1, 1),
            //new Vector3(-1, -1, 1),
            //new Vector3(-1, -1, 0),
            //new Vector3(0, -1, -1),
            //new Vector3(1, -1, -1),
            //new Vector3(0, -1, 0),

            //new Vector3(1, 1, 0),
            //new Vector3(0, 1, 1),
            //new Vector3(-1, 1, 1),
            //new Vector3(-1, 1, 0),
            //new Vector3(0, 1, -1),
            //new Vector3(1, 1, -1),
            //new Vector3(0, 1, 0),
            };


        public Vector3[] Dicections
        {
            get { return _directions; }
        }

        public override bool Init()
        {
            base.Init();

            return true;
        }

        public override void Update(float deltaTime)
        {
            foreach (var v in _bonfiresDic)
            {
                v.Value.Update(deltaTime);
            }
            foreach (var v in _map)
                v.Value.Update(deltaTime);
        }

        public override bool Dispose()
        {
            base.Dispose();

            ClearMap();

            return true;
        }

        public void CreateMap(HexagonMapPlugin[] hexagons, BonfireMapPlugin[] bonfires)
        {
            _map = MapTool.Instance.CreateMap(hexagons, GameObject.Find("Root"));
            _bonfiresDic = MapTool.Instance.CreateBonfire(bonfires, GameObject.Find("BonfireRoot"));
        }

        public void UpdateRound(int round)
        {
            foreach (var v in _bonfiresDic)
                v.Value.UpdateRound(round);
        }

        public float GetLoadingProgress()
        {
            float progress = 0;
            foreach (var v in _map)
                progress += v.Value.GetLoadingProgress();
            return progress / _map.Count;
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

            foreach (var pair in _bonfiresDic)
            {
                pair.Value.Dispose();
            }
            _bonfiresDic.Clear();
        }

        public bool ContainHexagon(string key)
        {
            return _map.ContainsKey(key);
        }

        public Hexagon GetHexagon(string key)
        {
            if (ContainHexagon(key))
                return _map[key];
            else
                return null;
        }

        public List<string> GetHexagonsByType(Enum.HexagonType type)
        {
            var hexagons = new List<string>();
            foreach (var v in _map)
            {
                if ((Enum.HexagonType)v.Value.ConfigID == type)
                {
                    hexagons.Add(v.Key);
                }
            }
            return hexagons;
        }

        public void UpdateHexagon(float intensity)
        {
            Hexagon hexagon = null;
            foreach (var v in _map)
            {
                hexagon = v.Value;
                break;
            }
            //hexagon.GameObject.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("MainLightIntensity");
        }

        /// <summary>
        /// 标记可抵达地块
        /// </summary>
        public void MarkingRegion(string hexagonID, float moveDis, float attackDis, Enum.RoleType roleType)
        {
            ClearMarkedRegion();

            var closeDic = new Dictionary<string, Cell>();
            FindingMoveRegion(hexagonID, moveDis, roleType, null, closeDic);

            var moveRegion = new List<string>();
            foreach (var v in closeDic)
            {
                moveRegion.Add(v.Key);
            }

            foreach (var v in closeDic)
            {
                if (null != v.Value.parent && moveRegion.Contains(v.Value.parent.id))
                    moveRegion.Remove(v.Value.parent.id);
            }

            FindingAttackRegion(moveRegion, attackDis, null, closeDic);

            foreach (var v in closeDic)
            { 
                _markedRegion.Add(v.Key);
                GetHexagon(v.Key)?.Marking(v.Value.type);
            }
        }

        public void ClearMarkedRegion()
        {
            foreach (var key1 in _markedRegion)
            {
                var hexagon = GetHexagon(key1);
                if (null != hexagon)
                    hexagon.Marking(Enum.MarkType.None);
            }
            _markedRegion.Clear();
        }

        /// <summary>
        /// 标记路径
        /// </summary>
        public void MarkingPath(string initiatorID, string targetID, float moveDis)
        {
            ClearMarkedPath();
            var cells = FindingPath(initiatorID, targetID, Enum.RoleType.Hero);
            if (cells.Count <= 0)
                return;

            if (cells[cells.Count - 1].g > moveDis)
                return;

            //标记路径
            var points = new List<Vector3>();
            for (int i = 0; i < cells.Count; i++)
            {
                points.Add(MapTool.Instance.GetPosFromCoor(cells[i].coor) + new Vector3(0, 0.23F, 0));
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
            public float h; //曼哈顿距离
            public Vector3 coor;
            public Cell parent;
            public Enum.MarkType type;

            public float f
            {
                get { return g + h; }
            }

            public Cell(float g, float h, Vector3 pos, Cell parent, string id)
            {
                this.g = g;
                this.h = h;
                this.coor = pos;
                this.id = id;
                this.parent = parent;
            }
        };

        private bool IsReachable(Vector3 coor, Enum.RoleType roleType)
        {
            //如果当前位置可跨越高度正上方有地块，证明该地块不可达
            for (int i = 1; i <= _roleHeight; i++)
            {
                var topKey = MapTool.Instance.GetHexagonKey(coor + new Vector3(0, i, 0));
                var upHexagon = GetHexagon(topKey);
                if (null != upHexagon)
                    return false;
            }

            var key = MapTool.Instance.GetHexagonKey(coor);

            if (!ContainHexagon(key))
                return false;

            var hexagon = GetHexagon(key);
            if (!hexagon.IsReachable())
                return false;

            //如果当前地块有敌人，是不可达的，但是攻击距离不受影响
            var roleId = RoleManager.Instance.GetRoleIDByHexagonID(key);
            if (roleId > 0)
            {
                var role = RoleManager.Instance.GetRole(roleId);
                if (roleType != role.Type)
                    return false;
            }

            return true;
        }

        private bool IsAttackable(Vector3 coor)
        {
            //如果当前位置正上方有地块，证明该地块为不可攻击
            for (int i = 1; i <= _roleHeight; i++)
            {
                var topKey = MapTool.Instance.GetHexagonKey(coor + new Vector3(0, i, 0));
                if (ContainHexagon(topKey))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 寻路专用
        /// </summary>
        private Cell HandleMoveCell(Vector3 cellPos, Vector3 endPos, Cell parent, Dictionary<string, Cell> openDic, Dictionary<string, Cell> closeDic, Enum.RoleType roleType)
        {
            if (!IsReachable(cellPos, roleType))
                return null;

            var key = MapTool.Instance.GetHexagonKey(cellPos);
            if (closeDic.ContainsKey(key))
                return null;

            var hexagon = GetHexagon(key);

            var cost = 0.0f;
            if (null != parent)
            {
                cost = parent.g + hexagon.GetCost();
                //if (cellPos.y != parent.coor.y)
                //    cost += hexagon.GetCost();
                //if (cellPos.x != parent.coor.x || cellPos.z != parent.coor.z)
                //    cost += hexagon.GetCost();
            }

            Cell cell = null;
            if (openDic.ContainsKey(key))
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
                cell = new Cell(cost, Vector3.Distance((Vector3)cellPos, endPos), (Vector3)cellPos, parent, hexagon.ID);
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
        public List<Cell> FindingPath(string startHexagonID, string endHexagonID, Enum.RoleType roleType)
        {
            List<Cell> path = new List<Cell>();
            Dictionary<string, Cell> openDic = new Dictionary<string, Cell>();
            Dictionary<string, Cell> closeDic = new Dictionary<string, Cell>();

            var startPos = GetHexagon(startHexagonID).coor;
            var endPos = GetHexagon(endHexagonID).coor;

            if (!IsReachable(endPos, roleType))
                return path;

            var cell = HandleMoveCell(startPos, endPos, null, openDic, closeDic, roleType);
            if (null == cell || cell.coor == endPos)
                return path;

            Cell endCell = null;
            while (openDic.Count > 0 && null == endCell)
            {
                Cell c1 = null;
                foreach (var pair in openDic)
                {
                    if (null == c1 || c1.f > pair.Value.f)
                    {
                        c1 = pair.Value;
                    }
                }

                for (int i = 0; i < _directions.Length; i++)
                {
                    var pos2 = c1.coor + _directions[i];
                    var cell2 = HandleMoveCell(pos2, endPos, c1, openDic, closeDic, roleType);
                    if (null != cell2 && cell2.coor == endPos)
                    {
                        endCell = cell2;
                        break;
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
        public List<string> FindingPathForStr(string initiatorID, string targetID, float dis, Enum.RoleType roleType)
        {
            var path = FindingPath(initiatorID, targetID, roleType);
            if (null == path || path.Count <= 0)
                return null;
            if (path[path.Count - 1].g > dis)
                return null;

            List<string> hexagons = new List<string>();
            for (int i = 0; i < path.Count; i++)
            {
                hexagons.Add(path[i].id);
            }
            return hexagons;
        }

        /// <summary>
        /// 寻路专用
        /// </summary>
        private Cell HandleAttackCell(Vector3 cellPos, Vector3 endPos, Cell parent, Dictionary<string, Cell> openDic, Dictionary<string, Cell> closeDic)
        {
            var key = MapTool.Instance.GetHexagonKey(cellPos);
            if (closeDic.ContainsKey(key))
                return null;

            var tempParent = parent;
            while (null != tempParent)
            {
                //如果当前位置可跨越高度正上方有地块，证明该地块不可攻击
                for (int i = 1; i <= _roleHeight; i++)
                {
                    var topKey = MapTool.Instance.GetHexagonKey(tempParent.coor + new Vector3(0, i, 0));
                    if (ContainHexagon(topKey))
                        return null;
                }

                tempParent = tempParent.parent;
            }

            var cost = 0f;
            if (null != parent)
            {
                cost = parent.g + 1;
                //if (cellPos.y != parent.coor.y)
                //    cost += 1;
                //if (cellPos.x != parent.coor.x || cellPos.z != parent.coor.z)
                //    cost += 1;
            }

            Cell cell = null;
            if (openDic.ContainsKey(key))
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
                cell = new Cell(cost, Vector3.Distance(cellPos, endPos), cellPos, parent, key);
                if (!IsAttackable(cellPos))
                {
                    closeDic.Add(key, cell);
                }
                else
                {
                    openDic.Add(key, cell);
                }
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
        public List<Cell> FindingAttackPath(string startHexagonID, string endHexagonID)
        {
            List<Cell> path = new List<Cell>();
            Dictionary<string, Cell> openDic = new Dictionary<string, Cell>();
            Dictionary<string, Cell> closeDic = new Dictionary<string, Cell>();

            var startPos = GetHexagon(startHexagonID).coor;
            var endPos = GetHexagon(endHexagonID).coor;

            var cell = HandleAttackCell(startPos, endPos, null, openDic, closeDic);
            if (null == cell || cell.coor == endPos)
                return path;

            Cell endCell = null;
            while (openDic.Count > 0 && null == endCell)
            {
                Cell c1 = null;
                foreach (var pair in openDic)
                {
                    if (null == c1 || c1.f > pair.Value.f)
                    {
                        c1 = pair.Value;
                    }
                }

                for (int i = 0; i < _directions.Length; i++)
                {
                    var pos2 = c1.coor + _directions[i];
                    var cell2 = HandleAttackCell(pos2, endPos, c1, openDic, closeDic);
                    if (null != cell2 && cell2.coor == endPos)
                    {
                        endCell = cell2;
                        break;
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
        public List<string> FindingAttackPathForStr(string initiatorID, string targetID, float dis)
        {
            var path = FindingAttackPath(initiatorID, targetID);
            if (null == path || path.Count <= 0)
                return null;

            if (path[path.Count - 1].g > dis)
                return null;

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
        private Cell HandleMoveRegionCell(Vector3 cellPos, Cell parent, float dis, Dictionary<string, Cell> openDic, Dictionary<string, Cell> closeDic, Dictionary<string, Cell> walkableDic, Enum.RoleType roleType)
        {
            if (!IsReachable(cellPos, roleType))
                return null;

            var key = MapTool.Instance.GetHexagonKey(cellPos);
            if (closeDic.ContainsKey(key))
                return null;

            if (null != walkableDic && walkableDic.ContainsKey(key))
                return null;

            var hexagon = GetHexagon(key);
            float cost = 0;
            if (null != parent)
            {
                cost = parent.g + hexagon.GetCost();
                //if (cellPos.y != parent.coor.y)
                //    cost += hexagon.GetCost();
                //if (cellPos.x != parent.coor.x || cellPos.z != parent.coor.z)
                //    cost += hexagon.GetCost();
            }

            if (cost > dis)
                return null;

            Cell cell = null;
            if (openDic.ContainsKey(key))
            {
                cell = openDic[key];
                if (cost < cell.g)
                {
                    cell.g = cost;
                    cell.parent = parent;
                }
            }
            else
            {
                cell = new Cell(cost, 0, cellPos, parent, hexagon.ID);
                cell.type = Enum.MarkType.Walkable;
                openDic.Add(key, cell);
            }

            return cell;
        }

        /// <summary>
        /// 标记可达区域专用
        /// </summary>
        private Cell HandleAttackRegionCell(Vector3 cellPos, Cell parent, float dis, Dictionary<string, Cell> openDic, Dictionary<string, Cell> closeDic)
        {
            var key = MapTool.Instance.GetHexagonKey(cellPos);
            //DebugManager.Instance.Log(key +"_"+ closeDic.ContainsKey(key));
            if (closeDic.ContainsKey(key))
                return null;

            //var tempParent = parent;
            //while (null != tempParent && tempParent.type == Enum.MarkType.Attackable)
            //{
            //    //如果当前位置可跨越高度正上方有地块，证明该地块不可攻击
            //    for (int i = 1; i <= _roleHeight; i++)
            //    {
            //        var topKey = MapTool.Instance.GetHexagonKey(tempParent.coor + new Vector3(0, i, 0));
            //        if (ContainHexagon(topKey))
            //            return null;
            //    }

            //    tempParent = tempParent.parent;
            //}

            float cost = 1;
            //if (cellPos.y != parent.coor.y)
            //    cost += 1;
            //if (cellPos.x != parent.coor.x || cellPos.z != parent.coor.z)
            //    cost += 1;

            if (null != parent && parent.type == Enum.MarkType.Attackable)
            {
                cost += parent.g;
            }

            //DebugManager.Instance.Log(cost > dis);
            if (cost > dis)
                return null;

            Cell cell = null;
            if (openDic.ContainsKey(key))
            {
                cell = openDic[key];
                if (cost < cell.g)
                {
                    cell.g = cost;
                    cell.parent = parent;
                }
            }
            else
            {
                cell = new Cell(cost, 0, cellPos, parent, key);
                cell.type = Enum.MarkType.Attackable;

                if (!IsAttackable(cellPos))
                {
                    closeDic.Add(key, cell);
                }
                else
                {
                    openDic.Add(key, cell);
                }
            }

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
        public Dictionary<string, Cell> FindingMoveRegion(string hexagonID, float moveDis, Enum.RoleType roleType, Dictionary<string, Cell> openDic = null, Dictionary<string, Cell> closeDic = null)
        {
            if (null == openDic)
                openDic = new Dictionary<string, Cell>();

            if (null == closeDic)
                closeDic = new Dictionary<string, Cell>();

            var startPos = GetHexagon(hexagonID).coor;
            var cell = HandleMoveRegionCell(startPos, null, moveDis, openDic, closeDic, null, roleType);
            if (null == cell)
                return closeDic;

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
                    for (int j = 0; j < _directions.Length; j++)
                    {
                        var pos2 = cell2.coor + _directions[j];
                        HandleMoveRegionCell(pos2, cell2, moveDis, openDic, closeDic, null, roleType);
                    }
                    openDic.Remove(key);
                    closeDic.Add(key, cell2);
                }
            }

            openDic.Clear();

            return closeDic;
        }

        /// <summary>
        /// 标记可攻击区域
        /// 广度优先
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, Cell> FindingAttackRegion(List<string> hexagonIDs, float attackDis, Dictionary<string, Cell> openDic = null, Dictionary<string, Cell> closeDic = null)
        {
            if (null == openDic)
                openDic = new Dictionary<string, Cell>();

            //DebugManager.Instance.Log(null == closeDic);
            if (null == closeDic)
                closeDic = new Dictionary<string, Cell>();

            foreach (var v in hexagonIDs)
            {
                for (int q = 0; q < _directions.Length; q++)
                {
                    HandleAttackRegionCell(GetHexagon(v).coor + _directions[q], null, attackDis, openDic, closeDic);
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
                    for (int j = 0; j < _directions.Length; j++)
                    {
                        var pos2 = cell2.coor + _directions[j];
                        HandleAttackRegionCell(pos2, cell2, attackDis, openDic, closeDic);
                    }
                }
                foreach (var v in allKeys)
                {
                    closeDic.Add(v, openDic[v]);
                    openDic.Remove(v);
                }
            }

            openDic.Clear();

            return closeDic;
        }

        public Dictionary<string, Cell> FindingViewRegion(string hexagonID, float viewDis)
        {
            var openDic = new Dictionary<string, Cell>();
            var closeDic = new Dictionary<string, Cell>();

            for (int q = 0; q < _directions.Length; q++)
            {
                var pos2 = GetHexagon(hexagonID).coor + _directions[q];
                HandleAttackRegionCell(pos2, null, viewDis, openDic, closeDic);
            }

            //DebugManager.Instance.Log(openDic.Count);

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
                    for (int j = 0; j < _directions.Length; j++)
                    {
                        var pos2 = cell2.coor + _directions[j];
                        HandleAttackRegionCell(pos2, cell2, viewDis, openDic, closeDic);
                    }
                    openDic.Remove(key);
                    closeDic.Add(key, cell2);
                }
            }

            openDic.Clear();

            return closeDic;
        }
    }
}
