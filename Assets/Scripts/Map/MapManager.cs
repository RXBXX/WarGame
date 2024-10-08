using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;
using FairyGUI;
using System.Linq;
using System.Collections;

//������Ϸ�е�ͼ����
namespace WarGame
{
    public class MapCellDictionary : Dictionary<int, Cell>
    {
        public void Recycle()
        {
            foreach (var v in this)
                v.Value.Recycle();
            Clear();
            MapManager.Instance.PushCellDicStack(this);
        }
    }

    public class MapCellList : List<Cell>
    {
        public void Recycle()
        {
            Clear();
            MapManager.Instance.PushCellListStack(this);
        }
    }

    public class MapIntList : List<int>
    {
        public void Recycle()
        {
            Clear();
            MapManager.Instance.PushIntListStack(this);
        }
    }


    /// <summary>
    /// Ѱ·ר��
    /// </summary>
    public class Cell
    {
        public int id;
        public float g; //�ƶ�����
        public float h; //�����پ���
        public WGVector3 coor;
        public Cell parent;
        public Enum.MarkType type;

        public float f
        {
            get { return g + h; }
        }

        public Cell(float g, float h, WGVector3 pos, Cell parent, int id)
        {
            this.g = g;
            this.h = h;
            this.coor = pos;
            this.id = id;
            this.parent = parent;
        }

        public void Recycle()
        {
            id = -1;
            g = 0;
            h = 0;
            coor = new WGVector3(0, 0, 0);
            parent = null;
            type = Enum.MarkType.None;

            MapManager.Instance.RecycleCell(this);
        }
    };

    public class MapManager : Singeton<MapManager>
    {
        private Dictionary<int, Hexagon> _map = new Dictionary<int, Hexagon>();
        private Dictionary<int, Bonfire> _bonfiresDic = new Dictionary<int, Bonfire>();
        private Dictionary<int, Ornament> _ornamentsDic = new Dictionary<int, Ornament>();
        private Stack<Cell> _cellPool = new Stack<Cell>();
        private Dictionary<int, int> _hexagonToRole = new Dictionary<int, int>();
        private Stack<MapCellDictionary> _mapCellDicStack = new Stack<MapCellDictionary>();
        private Stack<MapCellList> _mapCellLisStack = new Stack<MapCellList>();
        private Stack<MapIntList> _mapIntLisStack = new Stack<MapIntList>();

        private int _roleHeight = 5;

        //���ƶ�����
        private WGVector3[] _directions = new WGVector3[]
        {
            new WGVector3(1, 0, 0),
            new WGVector3(0, 0, 1),
            new WGVector3(-1, 0, 1),
            new WGVector3(-1, 0, 0),
            new WGVector3(0, 0, -1),
            new WGVector3(1, 0, -1),

            new WGVector3(1, -1, 0),
            new WGVector3(0, -1, 1),
            new WGVector3(-1, -1, 1),
            new WGVector3(-1, -1, 0),
            new WGVector3(0, -1, -1),
            new WGVector3(1, -1, -1),
            //new Vector3(0, -1, 0),

            new WGVector3(1, 1, 0),
            new WGVector3(0, 1, 1),
            new WGVector3(-1, 1, 1),
            new WGVector3(-1, 1, 0),
            new WGVector3(0, 1, -1),
            new WGVector3(1, 1, -1),
            //new Vector3(0, 1, 0),
        };


        public WGVector3[] Dicections
        {
            get { return _directions; }
        }

        public override bool Init()
        {
            return base.Init();
        }

        public override void Update(float deltaTime)
        {
            foreach (var v in _bonfiresDic)
            {
                v.Value.Update(deltaTime);
            }
            foreach (var v in _map)
                v.Value.Update(deltaTime);

            foreach (var v in _ornamentsDic)
                v.Value.Update(deltaTime);
        }

        public override bool Dispose()
        {
            ClearMap();

            foreach (var v in _mapCellDicStack)
                v.Clear();
            _mapCellDicStack.Clear();

            foreach (var v in _mapIntLisStack)
                v.Clear();
            _mapIntLisStack.Clear();

            return base.Dispose();
        }

        public void CreateMap(HexagonMapPlugin[] hexagons, BonfireMapPlugin[] bonfires, OrnamentMapPlugin[] ornaments, LightingPlugin lightingPlugin)
        {
            _map = MapTool.Instance.CreateMap(hexagons, GameObject.Find("Root"));
            _ornamentsDic = MapTool.Instance.CreateOrnament(ornaments, GameObject.Find("OrnamentRoot"));

            if (null != lightingPlugin)
            {
                GameObject.Find("Directional Light").GetComponent<Light>().color = lightingPlugin.LightColor;
                if (null != lightingPlugin.Sky)
                {
                    AssetsMgr.Instance.LoadAssetAsync<Material>("Assets/Materials/Skyboxs/" + lightingPlugin.Sky + ".mat", (mat) =>
                    {
                        GameObject.Find("Main Camera").GetComponent<Skybox>().material = mat;
                    });
                }
            }
        }

        //��ʼ������
        public List<BonfireData> InitBonfires(BonfireMapPlugin[] bonfirePlugins)
        {
            var bonfireDatas = new List<BonfireData>();
            foreach (var v in bonfirePlugins)
            {
                var bd = Factory.Instance.GetBonfireData(v);
                bonfireDatas.Add(bd);
                CreateBonfire(bd);
            }
            return bonfireDatas;
        }

        public Bonfire CreateBonfire(BonfireData bd)
        {
            //if (_bonfiresDic.ContainsKey(bd.UID))
            //    return null;

            var parent = GameObject.Find("BonfireRoot").transform;
            var bonfire = Factory.Instance.GetBonfire(bd);
            bonfire.SetParent(parent);
            _bonfiresDic.Add(bd.UID, bonfire);
            return bonfire;
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

            foreach (var pair in _ornamentsDic)
            {
                pair.Value.Dispose();
            }
            _ornamentsDic.Clear();
        }

        public bool ContainHexagon(int key)
        {
            return _map.ContainsKey(key);
        }

        public Hexagon GetHexagon(int key)
        {
            Hexagon hex = null;
            _map.TryGetValue(key, out hex);

            if (!Application.isPlaying && null == hex)
            {
                hex = Factory.Instance.GetHexagon(new HexagonMapPlugin(MapTool.Instance.GetHexagonKey(new WGVector3(0, 0, 0)), 1, true, new WGVector3(0, 0, 0)));
            }
            return hex;
        }

        public List<int> GetHexagonsByType(Enum.HexagonType type)
        {
            var hexagons = new List<int>();
            foreach (var v in _map)
            {
                if ((Enum.HexagonType)v.Value.ConfigID == type)
                {
                    hexagons.Add(v.Key);
                }
            }
            return hexagons;
        }

        public List<Bonfire> GetAllBonfires()
        {
            var bs = new List<Bonfire>();
            foreach (var v in _bonfiresDic)
                bs.Add(v.Value);
            return bs;
        }

        public Bonfire GetBonfire(int id)
        {
            return _bonfiresDic[id];
        }

        //public void UpdateHexagon(float intensity)
        //{
        //    Hexagon hexagon = null;
        //    foreach (var v in _map)
        //    {
        //        hexagon = v.Value;
        //        break;
        //    }
        //    //hexagon.GameObject.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("MainLightIntensity");
        //}

        /// <summary>
        /// ��ǿɵִ�ؿ�
        /// </summary>
        public void MarkingRegion(int hexagonID, float moveDis, float attackDis, Enum.RoleType roleType)
        {
            ClearMarkedRegion();

            var closeDic = PopCellDicStack();
            FindingMoveRegion(hexagonID, moveDis, roleType, closeDic);

            var moveRegion = new List<int>();
            foreach (var v in closeDic)
            {
                moveRegion.Add(v.Key);
            }

            foreach (var v in closeDic)
            {
                if (null != v.Value.parent && moveRegion.Contains(v.Value.parent.id))
                    moveRegion.Remove(v.Value.parent.id);
            }

            FindingAttackRegion(moveRegion, attackDis, closeDic);

            //foreach (var v in closeDic)
            //{ 
            //    _markedRegion.Add(v.Key);
            //    GetHexagon(v.Key)?.Marking(v.Value.type);
            //}

            foreach (var v in closeDic)
            {
                if (!ContainHexagon(v.Key))
                    continue;

                if (ContainHexagon(MapTool.Instance.GetHexagonKey(v.Value.coor + new WGVector3(0, 1, 0))))
                    continue;

                var hexagon = GetHexagon(v.Key);
                var blockParam = v.Value.type == Enum.MarkType.Walkable ? 1 : 0;
                RenderMgr.Instance.AddMeshInstanced("Assets/Prefabs/Mark.prefab", hexagon.GetPosition() + new Vector3(0, 0.4F, 0), Vector3.one / 5.0F, "_TexIndex", blockParam);
            }

            closeDic.Recycle();
        }

        public void ClearMarkedRegion()
        {
            RenderMgr.Instance.RemoveMeshInstanced("Assets/Prefabs/Mark.prefab");
            //foreach (var key1 in _markedRegion)
            //{
            //    var hexagon = GetHexagon(key1);
            //    if (null != hexagon)
            //        hexagon.Marking(Enum.MarkType.None);
            //}
            //_markedRegion.Clear();
        }

        /// <summary>
        /// ���·��
        /// </summary>
        public void MarkingPath(int initiatorID, int targetID, float moveDis)
        {
            ClearMarkedPath();
            var cells = FindingPath(initiatorID, targetID, Enum.RoleType.Hero);
            if (cells.Count <= 0)
                return;

            if (cells[cells.Count - 1].g > moveDis)
                return;

            //���·��
            var points = new List<Vector3>();
            for (int i = 0; i < cells.Count; i++)
            {
                points.Add(MapTool.Instance.GetPosFromCoor(cells[i].coor) + new Vector3(0, 0.23F, 0));
            }
            LineMgr.Instance.SetLine(points);

            cells.Recycle();
        }

        public void ClearMarkedPath()
        {
            LineMgr.Instance.ClearLine();
        }

        public Cell CreateCell(float g, float h, WGVector3 coor, Cell parent, int id)
        {
            if (_cellPool.Count <= 0)
            {
                return new Cell(g, h, coor, parent, id);
            }
            else
            {
                //DebugManager.Instance.Log("Recycle");
                var cell = _cellPool.Pop();
                cell.g = g;
                cell.h = h;
                cell.coor = coor;
                cell.parent = parent;
                cell.id = id;
                return cell;
            }
        }

        public void RecycleCell(Cell cell)
        {
            _cellPool.Push(cell);
        }

        private bool IsReachable(WGVector3 coor, Enum.RoleType roleType)
        {
            //������Ŀǰ������������������Ϊ�˽�ʡ���ܣ�������ʱע�͵������������Ҫ���ٴ�
            //�����ǰλ�ÿɿ�Խ�߶����Ϸ��еؿ飬֤���õؿ鲻�ɴ�
            for (int i = 1; i <= _roleHeight; i++)
            {
                var topKey = MapTool.Instance.GetHexagonKey(coor + new WGVector3(0, i, 0));
                if (ContainHexagon(topKey))
                    return false;
            }

            var key = MapTool.Instance.GetHexagonKey(coor);

            var hexagon = GetHexagon(key);
            if (null == hexagon)
                return false;

            if (!hexagon.IsReachable())
                return false;

            //�����ǰ�ؿ��е��ˣ��ǲ��ɴ�ģ����ǹ������벻��Ӱ��
            int roleId = 0;
            _hexagonToRole.TryGetValue(key, out roleId);
            if (roleId > 0)
            {
                var role = RoleManager.Instance.GetRole(roleId);
                if (roleType != role.Type)
                    return false;
            }

            return true;
        }

        private bool IsAttackable(WGVector3 coor)
        {
            //�����ǰλ�����Ϸ��еؿ飬֤���õؿ�Ϊ���ɹ���
            for (int i = 1; i <= _roleHeight; i++)
            {
                var topKey = MapTool.Instance.GetHexagonKey(coor + new WGVector3(0, i, 0));
                if (ContainHexagon(topKey))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Ѱ·ר��
        /// </summary>
        private Cell HandleMoveCell(WGVector3 cellPos, WGVector3 endPos, Cell parent, Dictionary<int, Cell> openDic, Dictionary<int, Cell> closeDic, Enum.RoleType roleType)
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
                //cell = new Cell(cost, Vector3.Distance((Vector3)cellPos, endPos), (Vector3)cellPos, parent, hexagon.ID);
                cell = CreateCell(cost, WGVector3.Distance(cellPos, endPos), cellPos, parent, hexagon.ID);
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
        public MapCellList FindingPath(int startHexagonID, int endHexagonID, Enum.RoleType roleType)
        {
            PrepareHexagonToRole();

            if (_hexagonToRole.ContainsKey(endHexagonID))
            {
                ClearHexagonToRole();
                return null;
            }

            var path = PopCellListStack();
            var openDic = PopCellDicStack();
            var closeDic = PopCellDicStack();

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
                if (openDic.ContainsKey(endCell.id))
                    openDic.Remove(endCell.id);
                if (closeDic.ContainsKey(endCell.id))
                    closeDic.Remove(endCell.id);

                path.Add(endCell);
                endCell = endCell.parent;
            }
            path.Reverse();

            openDic.Recycle();
            closeDic.Recycle();

            ClearHexagonToRole();

            return path;
        }

        /// <summary>
        /// ��ȡ·��ͨ���ؿ�id����ʽ����
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        public List<int> FindingPathForStr(int initiatorID, int targetID, float dis, Enum.RoleType roleType)
        {
            var path = FindingPath(initiatorID, targetID, roleType);
            if (null == path || path.Count <= 0)
                return null;
            if (path[path.Count - 1].g > dis)
                return null;
            var hexagons = new List<int>();
            for (int i = 0; i < path.Count; i++)
            {
                hexagons.Add(path[i].id);
            }

            path.Recycle();

            return hexagons;
        }

        /// <summary>
        /// Ѱ·ר��
        /// </summary>
        private Cell HandleAttackCell(WGVector3 cellPos, WGVector3 endPos, Cell parent, Dictionary<int, Cell> openDic, Dictionary<int, Cell> closeDic, float dis)
        {
            var key = MapTool.Instance.GetHexagonKey(cellPos);
            if (closeDic.ContainsKey(key))
                return null;

            var tempParent = parent;
            while (null != tempParent)
            {
                //�����ǰλ�ÿɿ�Խ�߶����Ϸ��еؿ飬֤���õؿ鲻�ɹ���
                for (int i = 1; i <= _roleHeight; i++)
                {
                    var topKey = MapTool.Instance.GetHexagonKey(tempParent.coor + new WGVector3(0, i, 0));
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

            if (cost > dis)
                return null;

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
                //cell = new Cell(cost, Vector3.Distance(cellPos, endPos), cellPos, parent, key);
                cell = CreateCell(cost, WGVector3.Distance(cellPos, endPos), cellPos, parent, key);
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
        /// Ѱ·
        /// �������
        /// </summary>
        /// <param name="startHexagonID"></param>
        /// <param name="endHexagonID"></param>
        /// <returns></returns>
        public List<Cell> FindingAttackPath(int startHexagonID, int endHexagonID, float dis)
        {
            List<Cell> path = new List<Cell>();
            var openDic = PopCellDicStack();
            var closeDic = PopCellDicStack();

            var startPos = GetHexagon(startHexagonID).coor;
            var endPos = GetHexagon(endHexagonID).coor;

            var cell = HandleAttackCell(startPos, endPos, null, openDic, closeDic, dis);
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
                    var cell2 = HandleAttackCell(pos2, endPos, c1, openDic, closeDic, dis);
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
                if (openDic.ContainsKey(endCell.id))
                    openDic.Remove(endCell.id);
                if (closeDic.ContainsKey(endCell.id))
                    closeDic.Remove(endCell.id);

                path.Add(endCell);
                endCell = endCell.parent;
            }
            path.Reverse();

            openDic.Recycle();
            closeDic.Recycle();

            return path;
        }

        /// <summary>
        /// ��ȡ·��ͨ���ؿ�id����ʽ����
        /// </summary>
        /// <param name="initiatorID"></param>
        /// <param name="targetID"></param>
        /// <returns></returns>
        public List<int> FindingAttackPathForStr(int initiatorID, int targetID, float dis)
        {
            var path = FindingAttackPath(initiatorID, targetID, dis);
            if (null == path || path.Count <= 0)
                return null;

            if (path[path.Count - 1].g > dis)
                return null;

            var hexagons = new List<int>();
            for (int i = 0; i < path.Count; i++)
            {
                hexagons.Add(path[i].id);
            }

            foreach (var v in path)
                v.Recycle();
            path.Clear();

            return hexagons;
        }

        /// <summary>
        /// ��ǿɴ�����ר��
        /// </summary>
        private Cell HandleMoveRegionCell(WGVector3 cellPos, Cell parent, float dis, Dictionary<int, Cell> openDic, Dictionary<int, Cell> closeDic, Dictionary<int, Cell> walkableDic, Enum.RoleType roleType)
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
                //cell = new Cell(cost, 0, cellPos, parent, hexagon.ID);
                cell = CreateCell(cost, 0, cellPos, parent, hexagon.ID);
                cell.type = Enum.MarkType.Walkable;
                openDic.Add(key, cell);
            }

            return cell;
        }

        /// <summary>
        /// ��ǿɴ�����ר��
        /// </summary>
        private Cell HandleAttackRegionCell(WGVector3 cellPos, Cell parent, float dis, Dictionary<int, Cell> openDic, Dictionary<int, Cell> closeDic)
        {
            var key = MapTool.Instance.GetHexagonKey(cellPos);
            //DebugManager.Instance.Log(key +"_"+ closeDic.ContainsKey(key));
            if (closeDic.ContainsKey(key))
                return null;

            //var tempParent = parent;
            //while (null != tempParent && tempParent.type == Enum.MarkType.Attackable)
            //{
            //    //�����ǰλ�ÿɿ�Խ�߶����Ϸ��еؿ飬֤���õؿ鲻�ɹ���
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
                //cell = new Cell(cost, 0, cellPos, parent, key);
                cell = CreateCell(cost, 0, cellPos, parent, key);
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
        /// ��ǿɴ�����
        /// �������
        /// </summary>
        /// <param name="hexagonID">���ؿ�</param>
        /// <param name="moveDis">���ƶ�����</param>
        /// /// <param name="moveDis">�ɹ�������</param>
        /// <returns></returns>
        public MapCellDictionary FindingMoveRegion(int hexagonID, float moveDis, Enum.RoleType roleType, MapCellDictionary closeDic = null)
        {
            PrepareHexagonToRole();

            var openDic = PopCellDicStack();

            if (null == closeDic)
                closeDic = PopCellDicStack();

            var startPos = GetHexagon(hexagonID).coor;
            var cell = HandleMoveRegionCell(startPos, null, moveDis, openDic, closeDic, null, roleType);
            if (null == cell)
                return closeDic;

            var allKeys = PopIntListStack();
            while (openDic.Count > 0)
            {
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
                allKeys.Clear();
            }
            allKeys.Recycle();

            ClearHexagonToRole();
            openDic.Recycle();
            return closeDic;
        }

        /// <summary>
        /// ��ǿɹ�������
        /// �������
        /// </summary>
        /// <returns></returns>
        public MapCellDictionary FindingAttackRegion(List<int> hexagonIDs, float attackDis, MapCellDictionary closeDic = null)
        {
            var openDic = PopCellDicStack();

            if (null == closeDic)
                closeDic = PopCellDicStack();

            foreach (var v in hexagonIDs)
            {
                for (int q = 0; q < _directions.Length; q++)
                {
                    HandleAttackRegionCell(GetHexagon(v).coor + _directions[q], null, attackDis, openDic, closeDic);
                }
            }

            var allKeys = PopIntListStack();
            while (openDic.Count > 0)
            {
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
                allKeys.Clear();
            }
            allKeys.Recycle();

            openDic.Recycle();
            return closeDic;
        }



        public MapCellDictionary FindingViewRegion(int hexagonID, float viewDis)
        {
            var openDic = PopCellDicStack();
            var closeDic = PopCellDicStack();

            for (int q = 0; q < _directions.Length; q++)
            {
                var pos2 = GetHexagon(hexagonID).coor + _directions[q];
                HandleAttackRegionCell(pos2, null, viewDis, openDic, closeDic);
            }

            //DebugManager.Instance.Log(openDic.Count);
            var allKeys = PopIntListStack();
            while (openDic.Count > 0)
            {
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
                allKeys.Clear();
            }
            allKeys.Recycle();
            openDic.Recycle();
            return closeDic;
        }

        private void PrepareHexagonToRole()
        {
            ClearHexagonToRole();
            var roles = RoleManager.Instance.GetAllRoles();
            foreach (var v in roles)
                _hexagonToRole.Add(v.Hexagon, v.ID);
        }

        private void ClearHexagonToRole()
        {
            _hexagonToRole.Clear();
        }

        public void PushCellDicStack(MapCellDictionary md)
        {
            _mapCellDicStack.Push(md);
        }

        public MapCellDictionary PopCellDicStack()
        {
            if (_mapCellDicStack.Count > 0)
                return _mapCellDicStack.Pop();
            else
                return new MapCellDictionary();
        }

        public void PushCellListStack(MapCellList ml)
        {
            _mapCellLisStack.Push(ml);
        }

        public MapCellList PopCellListStack()
        {
            if (_mapCellLisStack.Count > 0)
                return _mapCellLisStack.Pop();
            else
                return new MapCellList();
        }

        public void PushIntListStack(MapIntList ml)
        {
            _mapIntLisStack.Push(ml);
        }

        public MapIntList PopIntListStack()
        {
            if (_mapIntLisStack.Count > 0)
                return _mapIntLisStack.Pop();
            else
                return new MapIntList();
        }
    }
}
