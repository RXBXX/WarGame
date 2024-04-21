using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;
using UnityEngine.SceneManagement;

namespace WarGame
{
    public class SceneMgr : Singeton<SceneMgr>
    {
        private int _initiator;
        private string _hexagonID;
        private Enum.InstructType _curInstruct = Enum.InstructType.None;

        public override bool Init()
        {
            base.Init();

            //EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Move_Event, Move);
            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Attack_Event, Attack);
            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Cancel_Event, Cancel);
            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Check_Event, Check);
            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Select_Event, Select);
            EventDispatcher.Instance.AddListener(Enum.EventType.Hero_MoveEnd_Event, MoveEnd);

            return true;
        }

        public override bool Dispose()
        {
            base.Dispose();

            //EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Move_Event, Move);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Attack_Event, Attack);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Cancel_Event, Cancel);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Check_Event, Check);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Select_Event, Select);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Hero_MoveEnd_Event, MoveEnd);
            return true;
        }

        public void Touch(GameObject obj)
        {
            if (obj.tag == Enum.Tag.Hexagon.ToString())
            {
                if (_initiator > 0)
                {
                    var startID = RoleManager.Instance.GetHexagonIDByHeroID(_initiator);
                    var endID = obj.GetComponent<HexagonCellData>().ID;
                    MapManager.Instance.MarkingPath(startID, endID, 5);
                }
            }
        }

        public void Hit(GameObject obj)
        {
            var tag = obj.tag;
            string hexagonID = null;
            if (tag == Enum.Tag.Hero.ToString())
            {
                var heroId = obj.GetComponent<RoleData>().ID;
                if (heroId >= 0)
                {
                    _initiator = heroId;
                    hexagonID = RoleManager.Instance.GetHexagonIDByHeroID(heroId);
                    MapManager.Instance.MarkingRegion(hexagonID, 5, 2);
                }
            }
            else if (tag == Enum.Tag.Enemy.ToString())
            {
                var enemyId = obj.GetComponent<RoleData>().ID;
                if (enemyId >= 0)
                {
                    hexagonID = RoleManager.Instance.GetHexagonIDByEnemyID(enemyId);
                    MapManager.Instance.MarkingRegion(hexagonID, 5, 2);
                }
            }
            else if (tag == Enum.Tag.Hexagon.ToString())
            {
                if (_initiator > 0)
                {
                    var startID = RoleManager.Instance.GetHexagonIDByHeroID(_initiator);
                    var endID = obj.GetComponent<HexagonCellData>().ID;
                    Move(startID, endID);
                }
            }
        }

        /// <summary>
        /// 显示操作指令
        /// </summary>
        private void OpenInstruct(Enum.InstructType[] orders = null)
        {
            var hexagon = MapManager.Instance.GetHexagon(_hexagonID);
            HUDManager.Instance.AddHUD("HUD", "HUDInstruct", "HUDInstruct_Custom", hexagon.GameObject);
        }

        /// <summary>
        /// 显示操作指令
        /// </summary>
        public void CloseInstruct()
        {
            HUDManager.Instance.RemoveHUD("HUDInstruct_Custom");
        }

        public void Select(params object[] args)
        {
            switch ((Enum.InstructType)args[0])
            {
                case Enum.InstructType.Check:
                    Check();
                    break;
                case Enum.InstructType.Move:
                    _curInstruct = (Enum.InstructType)args[0];
                    break;
                case Enum.InstructType.Attack:
                    _curInstruct = (Enum.InstructType)args[0];
                    break;
                case Enum.InstructType.Cancel:
                    Cancel();
                    break;
            }
        }

        private void Move(string initiatorID, string targetID)
        {
            List<string> hexagons = MapManager.Instance.FindingPath2(initiatorID, targetID);

            var cost = 0.0f;
            for (int i = 0; i < hexagons.Count; i++)
            {
                cost += MapManager.Instance.GetHexagon(hexagons[i]).GetCost();
            }
            if (cost > 5)
                return;

            _hexagonID = targetID;
            RoleManager.Instance.MoveHero(1, hexagons);
        }

        private void Cancel(params object[] args)
        {
            DebugManager.Instance.Log("SceneMgr.Cancle");
            if (_initiator <= 0 || null == _hexagonID)
                return;

            CloseInstruct();
            MapManager.Instance.ClearMarkedRegion();

            DebugManager.Instance.Log(_hexagonID + "_" + RoleManager.Instance.GetHexagonIDByHeroID(_initiator));
            var path = MapManager.Instance.FindingPath2(_hexagonID, RoleManager.Instance.GetHexagonIDByHeroID(_initiator));
            RoleManager.Instance.MoveHero(_initiator, path);
            _initiator = 0;
            _hexagonID = null;
        }

        private void Attack(params object[] args)
        {
            DebugManager.Instance.Log("SceneMgr.Attack");
            CloseInstruct();
        }

        public void Check(params object[] args)
        {
            DebugManager.Instance.Log("SceneMgr.Check");
            UIManager.Instance.OpenPanel("Hero", "HeroPanel");
        }

        public void MoveEnd(params object[] args)
        {
            if (_initiator <= 0 || null == _hexagonID)
                return;
            MapManager.Instance.MarkingRegion(_hexagonID, 0, 2);
            OpenInstruct(new Enum.InstructType[] { Enum.InstructType.Attack, Enum.InstructType.Idle, Enum.InstructType.Cancel });
        }

        public void Create(string mapPath)
        {
            MapManager.Instance.CreateMap(mapPath);

            var attr = new RoleAttribute();
            attr.attack = 1;
            attr.defense = 0.2F;
            var bornPoint = MapTool.Instance.GetHexagonKey(Vector3.zero);
            RoleManager.Instance.CreateHero(1, attr, "Assets/RPG Tiny Hero Duo/Prefab/MaleCharacterPolyart.prefab", bornPoint);
            var bornPoint2 = MapTool.Instance.GetHexagonKey(new Vector3(3, 0, 3));
            RoleManager.Instance.CreateEnemy(2, attr, "Assets/RPG Tiny Hero Duo/Prefab/FemaleCharacterPolyart.prefab", bornPoint2);

            UIManager.Instance.OpenPanel("Fight", "FightPanel");
            LineMgr.Instance.Init();
        }

        public void Destroy()
        {
            UIManager.Instance.ClosePanel("FightPanel");
            RoleManager.Instance.Clear();
            MapManager.Instance.ClearMap();

            SceneManager.LoadScene("Main");
            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) =>
            {
                UIManager.Instance.OpenPanel("Map", "MapPanel");
            };
            LineMgr.Instance.Dispose();
        }
    }
}
