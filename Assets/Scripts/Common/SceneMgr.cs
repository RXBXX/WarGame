using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;
using UnityEngine.SceneManagement;

namespace WarGame
{
    public class SceneMgr : Singeton<SceneMgr>
    {
        private int _initiator; //当前选中的英雄
        private string _start; //英雄移动的起点地块
        private Enum.InstructType _curInstruct = Enum.InstructType.None;
        private bool _locked = false;

        public override bool Init()
        {
            base.Init();

            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Idle_Event, Idle);
            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Attack_Event, Attack);
            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Cancel_Event, Cancel);
            //EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Check_Event, Check);
            //EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Select_Event, Select);

            EventDispatcher.Instance.AddListener(Enum.EventType.Hero_MoveEnd_Event, MoveEnd);

            return true;
        }

        public override bool Dispose()
        {
            base.Dispose();

            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Idle_Event, Idle);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Attack_Event, Attack);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Cancel_Event, Cancel);
            //EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Check_Event, Check);
            //EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Select_Event, Select);

            EventDispatcher.Instance.RemoveListener(Enum.EventType.Hero_MoveEnd_Event, MoveEnd);
            return true;
        }

        public void Touch(GameObject obj)
        {
            if (Enum.InstructType.None != _curInstruct)
                return;

            if (obj.tag == Enum.Tag.Hexagon.ToString())
            {
                if (_initiator > 0)
                {
                    var start = RoleManager.Instance.GetHexagonIDByHeroID(_initiator);
                    var end = obj.GetComponent<HexagonCellData>().ID;
                    MapManager.Instance.MarkingPath(start, end, 5);
                }
            }
        }

        public void Click(GameObject obj)
        {
            if (_locked)
                return;

            var tag = obj.tag;
            string hexagonID = null;
            if (tag == Enum.Tag.Hero.ToString())
            {
                var heroId = obj.GetComponent<RoleData>().ID;
                if (heroId >= 0)
                {
                    _initiator = heroId;
                    hexagonID = RoleManager.Instance.GetHexagonIDByHeroID(heroId);
                    MapManager.Instance.ClearMarkedRegion();
                    MapManager.Instance.MarkingRegion(hexagonID, 5, 2);
                }
            }
            else if (tag == Enum.Tag.Enemy.ToString())
            {

                var enemyId = obj.GetComponent<RoleData>().ID;
                if (enemyId >= 0)
                {
                    if (_curInstruct == Enum.InstructType.Attack)
                    {
                        Battle(enemyId);
                    }
                    else
                    {
                        hexagonID = RoleManager.Instance.GetHexagonIDByEnemyID(enemyId);
                        MapManager.Instance.ClearMarkedRegion();
                        MapManager.Instance.MarkingRegion(hexagonID, 5, 2);
                    }
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
            if (_initiator <= 0)
                return;
            var hexagonID = RoleManager.Instance.GetHexagonIDByHeroID(_initiator);
            var hexagon = MapManager.Instance.GetHexagon(hexagonID);
            HUDManager.Instance.AddHUD("HUD", "HUDInstruct", "HUDInstruct_Custom", hexagon.GameObject);
        }

        /// <summary>
        /// 显示操作指令
        /// </summary>
        public void CloseInstruct()
        {
            HUDManager.Instance.RemoveHUD("HUDInstruct_Custom");
        }

        private void Move(string start, string end)
        {
            List<string> hexagons = MapManager.Instance.FindingPath2(start, end);

            if (null == hexagons || hexagons.Count <= 0)
                return;

            var cost = 0.0f;
            for (int i = 1; i < hexagons.Count; i++)
            {
                cost += MapManager.Instance.GetHexagon(hexagons[i]).GetCost();
            }
            if (cost > 5)
                return;

            _locked = true;
            _start = start;
            MapManager.Instance.ClearMarkedRegion();
            RoleManager.Instance.MoveHero(1, hexagons);
        }

        private void Battle(int enemyID)
        {
            var initiatorID = RoleManager.Instance.GetHexagonIDByHeroID(_initiator);
            var targetID = RoleManager.Instance.GetHexagonIDByEnemyID(enemyID);

            List<string> hexagons = MapManager.Instance.FindingPath2(initiatorID, targetID);
            if (null == hexagons || hexagons.Count <= 0)
                return;
            var cost = 0.0f;
            for (int i = 1; i < hexagons.Count; i++)
            {
                cost += MapManager.Instance.GetHexagon(hexagons[i]).GetCost();
            }

            if (cost > 2)
                return;
            MapManager.Instance.ClearMarkedRegion();
            RoleManager.Instance.Attack(_initiator, enemyID);
            _initiator = 0;
            _curInstruct = Enum.InstructType.None;
        }

        private void Idle(params object[] args)
        {
            if (_initiator <= 0)
                return;
            CloseInstruct();
            MapManager.Instance.ClearMarkedRegion();

            _initiator = 0;
            _start = null;
            _locked = false;
        }

        private void Cancel(params object[] args)
        {
            DebugManager.Instance.Log("ScenMgr.Cancel");
            if (_initiator <= 0)
                return;

            CloseInstruct();
            MapManager.Instance.ClearMarkedRegion();

            var start = RoleManager.Instance.GetHexagonIDByHeroID(_initiator);
            DebugManager.Instance.Log("ScenMgr.Cancel:" + start +"/" + _start);
            var path = MapManager.Instance.FindingPath2(start, _start);
            RoleManager.Instance.MoveHero(_initiator, path);
            _initiator = 0;
            _start = null;
            _locked = false;
        }

        private void Attack(params object[] args)
        {
            DebugManager.Instance.Log("SceneMgr.Attack");
            CloseInstruct();

            _curInstruct = Enum.InstructType.Attack;
            _start = null;
            _locked = false;
        }

        public void Check(params object[] args)
        {
            DebugManager.Instance.Log("SceneMgr.Check");
            UIManager.Instance.OpenPanel("Hero", "HeroPanel");
        }

        public void MoveEnd(params object[] args)
        {
            if (_initiator <= 0)
                return;
            var hexagonID = RoleManager.Instance.GetHexagonIDByHeroID(_initiator);
            MapManager.Instance.MarkingRegion(hexagonID, 0, 2);
            OpenInstruct(new Enum.InstructType[] { Enum.InstructType.Attack, Enum.InstructType.Idle, Enum.InstructType.Cancel });
        }

        public void Create(string mapPath)
        {
            MapManager.Instance.CreateMap(mapPath);

            var attr = new RoleAttribute();
            attr.hp = 100;
            attr.attack = 10;
            attr.defense = 5F;
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
