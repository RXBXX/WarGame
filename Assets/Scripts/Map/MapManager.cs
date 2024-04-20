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

        private HexagonCell _selectedHexagon = null, _targetHexagon = null;

        private List<string> _markedHexagons = new List<string>();

        private UIBase _curInstruct = null;

        //�ƶ��п��Կ�Խ�߶�
        private int _stepHeight = 1;

        private List<Cell> _path = new List<Cell>();

        //���ƶ�����
        private Vector3[] directions = new Vector3[] {
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

            EventDispatcher.Instance.AddListener("MapInstruct_Move_Event", Move);
            EventDispatcher.Instance.AddListener("MapInstruct_Attack_Event", Attack);
            EventDispatcher.Instance.AddListener("MapInstruct_Cancel_Event", Cancel);
            EventDispatcher.Instance.AddListener("MapInstruct_Check_Event", Check);

            return true;
        }

        public override bool Dispose()
        {
            base.Dispose();

            foreach (var pair in _map)
            {
                pair.Value.Dispose();
            }
            _map.Clear();

            if (null != _selectedHexagon)
            {
                _selectedHexagon.Dispose();
                _selectedHexagon = null;
            }

            if (null != _targetHexagon)
            {
                _targetHexagon.Dispose();
                _targetHexagon = null;
            }

            if (null != _curInstruct)
            {
                _curInstruct.Dispose();
            }
            return true;
        }

        public void CreateMap(string mapPath)
        {
            _map = MapTool.Instance.CreateMap(mapPath, GameObject.Find("Root"));
        }

        public void ClearMap()
        {
            foreach (var pair in _map)
            {
                pair.Value.Dispose();
            }
            _map.Clear();

            _selectedHexagon = null;
            _targetHexagon = null;

            if (null != _curInstruct)
            {
                _curInstruct.Dispose();
                _curInstruct = null;
            }
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

        public void SelectHexagon(Vector3 pos, bool haveHero = false)
        {
            var key = MapTool.Instance.GetHexagonKey(pos);
            if (!ContainHexagon(key))
                return;

            var hexagon = GetHexagon(key);
            if (hexagon == _targetHexagon)
                return;
            if ((hexagon == _selectedHexagon))
                return;

            hexagon.OnClick();

            if (null == _selectedHexagon && null == _targetHexagon)
                haveHero = true;

            //�������ؿ���Ӣ��
            if (haveHero)
            {
                if (null == _selectedHexagon)  //û����㲢��û���յ�
                {
                    hexagon.Marking(Enum.MarkType.Selected);
                    _selectedHexagon = GetHexagon(key);
                    MarkReachableHexagon(pos);
                }
            }
            else
            {
                if (null != _selectedHexagon && null == _targetHexagon) //�����û���յ�
                {
                    var path = FindingPath(_selectedHexagon.position, hexagon.position);
                    if (path.Count > 0 && 5 >= path[0].g)
                    {
                        ClearMarked();

                        hexagon.Marking(Enum.MarkType.Selected);
                        _targetHexagon = GetHexagon(key);
                        //���·��
                        for (int i = 1; i < path.Count - 1; i++)
                        {
                            var key1 = MapTool.Instance.GetHexagonKey(path[i].pos);
                            _markedHexagons.Add(key1);
                            GetHexagon(key1).Marking(path[i].type);
                        }
                        _path = path;
                        //��ָ����
                        ShowHUD(hexagon);
                    }
                }
            }
        }

        /// <summary>
        /// ��ǿɵִ�ؿ�
        /// </summary>
        public void MarkReachableHexagon(Vector3 pos)
        {
            var region = FindingRegion(pos, 5, 2); //��ͬӢ�۵�������һ��
            for (int i = 1; i < region.Count; i++)
            {
                var cell = region[i];
                var key = MapTool.Instance.GetHexagonKey(cell.pos);

                _markedHexagons.Add(key);
                if (ContainHexagon(key))
                {
                    GetHexagon(key).Marking(cell.type);
                }
            }
        }

        public void ClearMarked()
        {
            foreach (var key1 in _markedHexagons)
            {
                GetHexagon(key1).Marking(Enum.MarkType.None);
            }
            _markedHexagons.Clear();
        }

        /// <summary>
        /// ��ʾ����ָ��
        /// </summary>
        public void ShowHUD(HexagonCell hexagon)
        {
            DebugManager.Instance.Log("ShowHUD");
            HUDManager.Instance.AddHUD("Map", "MapInstruct", "MapInstruct_Custom", hexagon.gameObject);
        }

        /// <summary>
        /// ��ʾ����ָ��
        /// </summary>
        public void CloseHUD()
        {
            HUDManager.Instance.RemoveHUD("MapInstruct_Custom");
            //UIManager.Instance.CloseComponent("MapInstruct_Custom");
        }

        private void Move(params object[] args)
        {
            Debug.Log("Move");
            CloseHUD();
            ClearMarked();

            _targetHexagon.Marking(Enum.MarkType.None);
            _targetHexagon = null;
            _selectedHexagon.Marking(Enum.MarkType.None);
            _selectedHexagon = null;

            HeroManager.Instance.SetHero(GameObject.Find("Hero"));
            List<Vector3> points = new List<Vector3> { };
            for (int i = 0; i < _path.Count; i++)
            {
                points.Add(MapTool.Instance.FromCellPosToWorldPos(_path[i].pos) + new Vector3(0, 0.53F, 0));
            }
            HeroManager.Instance.Move(points);
        }

        private void Cancel(params object[] args)
        {
            Debug.Log("Cancel");
            CloseHUD();
            ClearMarked();

            _targetHexagon.Marking(Enum.MarkType.None);
            _targetHexagon = null;
            MarkReachableHexagon(_selectedHexagon.position);
        }

        private void Attack(params object[] args)
        {
            Debug.Log("Attack");
            CloseHUD();
            ClearMarked();
        }

        public void Check(params object[] args)
        {
            Debug.Log("Check");
        }


        /// <summary>
        /// Ѱ·ר��
        /// </summary>
        public class Cell
        {
            public float g;
            public float h;
            public Vector3 pos;
            public Cell parent;
            public Enum.MarkType type;

            public Cell(float g, float h, Vector3 pos, Cell parent)
            {
                this.g = g;
                this.h = h;
                this.pos = pos;
                this.parent = parent;
            }
        };

        /// <summary>
        /// Ѱ·ר��
        /// </summary>
        private Cell HandleCell(Vector3 cellPos, Vector3 endPos, Cell parent, Dictionary<string, Cell> openDic, Dictionary<string, Cell> closeDic)
        {
            //�����ǰλ�ÿɿ�Խ�߶����Ϸ��еؿ飬֤���õؿ鲻�ɴ�
            var topKey = MapTool.Instance.GetHexagonKey(cellPos + new Vector3(0, _stepHeight + 1, 0));
            var upHexagon = GetHexagon(topKey);
            if (null != upHexagon)
                return null;

            //���������ҵ�ǰλ�õĿɵִ�ؿ�
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
                cell = new Cell(hexagon.GetCost() + parent.g, Vector3.Distance(cellPos, endPos), cellPos, parent);
                cell.type = Enum.MarkType.Selected;
                openDic.Add(key, cell);
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
                cell = new Cell(hexagon.GetCost() + parentG, Vector3.Distance(cellPos, endPos), cellPos, parent);
                cell.type = Enum.MarkType.Selected;
                openDic.Add(key, cell);
            }
            return cell;
        }

        /// <summary>
        /// Ѱ·
        /// �������
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        public List<Cell> FindingPath(Vector3 startPos, Vector3 endPos)
        {
            List<Cell> path = new List<Cell>();
            Dictionary<string, Cell> openDic = new Dictionary<string, Cell>();
            Dictionary<string, Cell> closeDic = new Dictionary<string, Cell>();

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

                for (int i = 0; i < directions.Length; i++)
                {
                    var pos2 = c1.pos + directions[i];
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

            return path;
        }


        /// <summary>
        /// ��ǿɴ�����ר��
        /// </summary>
        private Cell HandleCell2(Vector3 cellPos, Cell parent, float moveDis, float attackDis, Dictionary<string, Cell> openDic, Dictionary<string, Cell> closeDic)
        {
            //�����ǰλ�ÿɿ�Խ�߶����Ϸ��еؿ飬֤���õؿ鲻�ɴ�
            var topKey = MapTool.Instance.GetHexagonKey(cellPos + new Vector3(0, _stepHeight + 1, 0));
            var upHexagon = GetHexagon(topKey);
            if (null != upHexagon)
                return null;

            //���������ҵ�ǰλ�õĿɵִ�ؿ�
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

            //ÿ���ؿ��ͨ�����۲�һ�������������Ҫ����

            float parentG = 0;
            if (null != parent)
                parentG = parent.g;
            Cell cell = new Cell(hexagon.GetCost() + parentG, 0, cellPos, parent);

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
        /// ��ǿɴ�����
        /// �������
        /// </summary>
        /// <param name="startPos">���λ��</param>
        /// <param name="moveDis">���ƶ�����</param>
        /// /// <param name="moveDis">�ɹ�������</param>
        /// <returns></returns>
        public List<Cell> FindingRegion(Vector3 startPos, float moveDis, float attackDis)
        {
            List<Cell> region = new List<Cell>();

            Dictionary<string, Cell> openDic = new Dictionary<string, Cell>();
            Dictionary<string, Cell> closeDic = new Dictionary<string, Cell>();

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
                    for (int i = 0; i < directions.Length; i++)
                    {
                        var pos2 = cell2.pos + directions[i];
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
