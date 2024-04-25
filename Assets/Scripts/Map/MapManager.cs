using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;
using FairyGUI;

//������Ϸ�е�ͼ����
namespace WarGame
{
    public class MapManager : Singeton<MapManager>
    {
        private Dictionary<string, HexagonCell> _map = new Dictionary<string, HexagonCell>();

        private List<string> _markedRegion = new List<string>();

        //�ƶ��п��Կ�Խ�߶�
        private int _stepHeight = 1;

        //���ƶ�����
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
        /// ��ǿɵִ�ؿ�
        /// </summary>
        public void MarkingRegion(string hexagonID, float moveDis, float attackDis, Enum.RoleType roleType)
        {
            var region = FindingRegion(hexagonID, moveDis, attackDis, roleType);
            for (int i = 0; i < region.Count; i++)
            {
                _markedRegion.Add(region[i].id);
                GetHexagon(region[i].id).Marking(region[i].type);
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
        /// ���·��
        /// </summary>
        public void MarkingPath(string initiatorID, string targetID, float moveDis)
        {
            ClearMarkedPath();

            var cells = FindingPath(initiatorID, targetID, Enum.RoleType.Hero, true);
            if (cells.Count <= 0)
                return;

            if (cells[cells.Count - 1].g > moveDis)
                return;

            //���·��
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
        /// Ѱ·ר��
        /// </summary>
        public class Cell
        {
            public string id;
            public float g; //�ƶ�����
            public float h;
            public Vector3 coor;
            public Cell parent;
            public Enum.MarkType type;

            public Cell(float g, float h, Vector3 pos, Cell parent, string id)
            {
                this.g = g;
                this.h = h;
                this.coor = pos;
                this.id = id;
                this.parent = parent;
            }
        };

        ///���ֵ�ǰ�����ڴ�ֱ�����Ͽɴ�ĵؿ�
        ///���������ҵ�ǰλ�õĿɵִ�ؿ�
        private Vector3 FindingVerticalCell(Vector3 coor)
        {
            for (int i = (int)coor.y + _stepHeight; i >= coor.y - _stepHeight; i--)
            {
                if (ContainHexagon(MapTool.Instance.GetHexagonKey(new Vector3(coor.x, i, coor.z))))
                {
                    coor.y = i;
                    break;
                }
            }
            return coor;
        }

        private bool IsReachable(Vector3 coor, Enum.RoleType roleType, bool isMovePath = true)
        {
            //�����ǰλ�ÿɿ�Խ�߶����Ϸ��еؿ飬֤���õؿ鲻�ɴ�
            var topKey = MapTool.Instance.GetHexagonKey(coor + new Vector3(0, _stepHeight + 1, 0));
            var upHexagon = GetHexagon(topKey);
            if (null != upHexagon)
                return false;

            var key = MapTool.Instance.GetHexagonKey(coor);
            if (!ContainHexagon(key))
                return false;

            var hexagon = GetHexagon(key);
            if (!hexagon.IsReachable())
                return false;

            //�����ǰ�ؿ��е��ˣ��ǲ��ɴ�ģ����ǹ������벻��Ӱ��
            if (isMovePath)
            {
                var roleId = RoleManager.Instance.GetRoleIDByHexagonID(key);
                if (roleId > 0)
                {
                    var role = RoleManager.Instance.GetRole(roleId);
                    if (roleType != role.Type)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Ѱ·ר��
        /// </summary>
        private Cell HandleCell(Vector3 cellPos, Vector3 endPos, Cell parent, Dictionary<string, Cell> openDic, Dictionary<string, Cell> closeDic, Enum.RoleType roleType, bool isMovePath = true)
        {
            cellPos = FindingVerticalCell(cellPos);

            if (!IsReachable(cellPos, roleType, isMovePath))
                return null;

            var key = MapTool.Instance.GetHexagonKey(cellPos);
            if (closeDic.ContainsKey(key))
                return null;

            var hexagon = GetHexagon(key);

            var cost = 0.0f;
            if (null != parent)
                cost = hexagon.GetCost() + parent.g;

            Cell cell = null;
            if (cellPos == endPos)
            {
                cell = new Cell(cost, Vector3.Distance(cellPos, endPos), cellPos, parent, hexagon.ID);
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
                openDic.Add(key, cell);
            }

            return cell;
        }


        /// <summary>
        /// Ѱ·
        /// �������
        /// </summary>
        /// <param name="startHexagonID"></param>
        /// <param name="endHexagonID"></param>
        /// <returns></returns>
        public List<Cell> FindingPath(string startHexagonID, string endHexagonID, Enum.RoleType roleType, bool isMovePath = true)
        {
            List<Cell> path = new List<Cell>();
            Dictionary<string, Cell> openDic = new Dictionary<string, Cell>();
            Dictionary<string, Cell> closeDic = new Dictionary<string, Cell>();

            var startPos = GetHexagon(startHexagonID).coordinate;
            var endPos = GetHexagon(endHexagonID).coordinate;

            if (!IsReachable(endPos, roleType, isMovePath))
                return path;

            var cell = HandleCell(startPos, endPos, null, openDic, closeDic, roleType, isMovePath);
            if (null == cell || cell.coor == endPos)
                return path;

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
                    var cell2 = HandleCell(pos2, endPos, c1, openDic, closeDic, roleType, isMovePath);
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
        /// ��ȡ·��ͨ���ؿ�id����ʽ����
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        public List<string> FindingPathForStr(string initiatorID, string targetID, Enum.RoleType roleType, bool isMovePath = true)
        {
            var path = FindingPath(initiatorID, targetID, roleType, isMovePath);
            List<string> hexagons = new List<string>();
            for (int i = 0; i < path.Count; i++)
            {
                hexagons.Add(path[i].id);
            }
            return hexagons;
        }


        /// <summary>
        /// ��ǿɴ�����ר��
        /// </summary>
        private Cell HandleRegionCell(Vector3 cellPos, Cell parent, float dis, Dictionary<string, Cell> openDic, Dictionary<string, Cell> closeDic, Dictionary<string, Cell> walkableDic, Enum.RoleType roleType, Enum.MarkType markType, bool isMovePath = true)
        {
            cellPos = FindingVerticalCell(cellPos);

            if (!IsReachable(cellPos, roleType, isMovePath))
                return null;

            var key = MapTool.Instance.GetHexagonKey(cellPos);
            if (closeDic.ContainsKey(key))
                return null;
            if (null != walkableDic && walkableDic.ContainsKey(key))
                return null;

            var hexagon = GetHexagon(key);
            //ÿ���ؿ��ͨ�����۲�һ�������������Ҫ����
            float cost = 0;
            if (!isMovePath)
            {
                cost = hexagon.GetCost();
            }

            //����п�ͨ��������ͣ���ĵؿ飨����ͬ��Ӫ��ɫ���ڵĵؿ飩��ͨ�����߸õؿ�������Ż�·���滮
            if (null != parent)
            {
                if(parent.type == markType)
                    cost = parent.g + hexagon.GetCost();

                var roleId = RoleManager.Instance.GetRoleIDByHexagonID(key);
                if (roleId > 0)
                {
                    var role = RoleManager.Instance.GetRole(roleId);
                    if (roleType == role.Type)
                    {
                        cost = cost + 1; //�����и����⣬���ֻ�м���Ӣ��ռ����Ψһ·�ڣ����۹��߻ᵼ��Ѱ·ʧ��
                    }
                }
            }

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
            else if (cost <= dis)
            {
                cell = new Cell(cost, 0, cellPos, parent, hexagon.ID);
                cell.type = markType;
                openDic.Add(key, cell);
            }

            return cell;
        }

        /// <summary>
        /// ��ǿɴ�����
        /// �������
        /// </summary>
        /// <param name="hexagonID">���ؿ�</param>
        /// <param name="moveDis">���ƶ�����</param>
        /// /// <param name="moveDis">�ɹ�������</param>
        /// <returns></returns>
        public List<Cell> FindingMoveRegion(string hexagonID, float moveDis, Enum.RoleType roleType)
        {
            List<Cell> region = new List<Cell>();

            Dictionary<string, Cell> openDic = new Dictionary<string, Cell>();
            Dictionary<string, Cell> closeDic = new Dictionary<string, Cell>();

            var startPos = GetHexagon(hexagonID).coordinate;
            var cell = HandleRegionCell(startPos, null, moveDis, openDic, closeDic, null, roleType, Enum.MarkType.Walkable, true);
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
                        HandleRegionCell(pos2, cell2, moveDis, openDic, closeDic, null, roleType, Enum.MarkType.Walkable, true);
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
        /// ��ǿɹ�������
        /// �������
        /// </summary>
        /// <returns></returns>
        public List<Cell> FindingAttackRegion(List<Cell> cells, float attackDis, Enum.RoleType roleType)
        {
            Dictionary<string, Cell> openDic = new Dictionary<string, Cell>();
            Dictionary<string, Cell> closeDic = new Dictionary<string, Cell>();
            Dictionary<string, Cell> walkableDic = new Dictionary<string, Cell>();

            for (int i = 0; i < cells.Count; i++)
            {
                walkableDic.Add(cells[i].id, cells[i]);
            }

            for (int i = 0; i < cells.Count; i++)
            {
                for (int j = 0; j < _directions.Length; j++)
                {
                    var pos2 = cells[i].coor + _directions[j];
                    var cell = HandleRegionCell(pos2, cells[i], attackDis, openDic, closeDic, walkableDic, roleType, Enum.MarkType.Attackable, false);
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
                        HandleRegionCell(pos2, cell2, attackDis, openDic, closeDic, walkableDic, roleType, Enum.MarkType.Attackable, false);
                    }
                    openDic.Remove(key);
                    closeDic.Add(key, cell2);
                }
            }

            List<Cell> region = new List<Cell>();
            foreach (var pair in closeDic)
            {
                region.Add(pair.Value);
            }

            openDic.Clear();
            closeDic.Clear();
            walkableDic.Clear();

            return region;
        }

        public List<Cell> FindingRegion(string hexagonID, float moveDis, float attackDis, Enum.RoleType roleType)
        {
            //�������еĸ��ڵ�
            var region = FindingMoveRegion(hexagonID, moveDis, roleType);
            var attackRegion = FindingAttackRegion(region, attackDis, roleType);
            for (int i = 0; i < attackRegion.Count; i++)
            {
                region.Add(attackRegion[i]);
            }
            return region;
        }

        public List<string> FindingAIPath(string initiator, string target, float moveDis, float attackDis)
        {
            var region = FindingRegion(initiator, moveDis, attackDis, Enum.RoleType.Enemy);
            Cell targetCell = null;
            for (int i = 0; i < region.Count; i++)
            {
                if (region[i].id == target)
                {
                    targetCell = region[i];
                }
            }

            if (null == targetCell)
                return null;

            List<string> path = new List<string>();
            targetCell = targetCell.parent;
            //var heroId = RoleManager.Instance.GetHeroIDByHexagonID(targetCell.id);
            //var enemyId = RoleManager.Instance.GetEnemyIDByHexagonID(targetCell.id)
            //if (heroId != _initiator)
            //return null;
            while (null != targetCell)
            {
                path.Add(targetCell.id);
                targetCell = targetCell.parent;
            }
            path.Reverse();
            return path;
        }
    }
}
