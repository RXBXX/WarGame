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
        private string _start; //英雄移动的起点
        private Enum.InstructType _curInstruct = Enum.InstructType.None;
        private bool _locked = false;

        public override bool Init()
        {
            base.Init();

            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Idle_Event, Idle);
            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Attack_Event, Attack);
            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Cancel_Event, Cancel);
            EventDispatcher.Instance.AddListener(Enum.EventType.Hero_MoveEnd_Event, MoveEnd);

            return true;
        }

        public override bool Dispose()
        {
            base.Dispose();

            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Idle_Event, Idle);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Attack_Event, Attack);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Cancel_Event, Cancel);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Hero_MoveEnd_Event, MoveEnd);

            return true;
        }

        public void Touch(GameObject obj)
        {
            if (Enum.InstructType.None != _curInstruct)
                return;

            if (_locked)
                return;

            if (obj.tag == Enum.Tag.Hexagon.ToString())
            {
                if (_initiator > 0)
                {
                    var start = RoleManager.Instance.GetHexagonIDByHeroID(_initiator);
                    var end = obj.GetComponent<HexagonCellData>().ID;

                    var hero = RoleManager.Instance.GetHero(_initiator);
                    MapManager.Instance.MarkingPath(start, end, hero.GetMoveDis());
                }
            }
        }

        public void Click(GameObject obj)
        {
            if (_locked)
                return;

            var tag = obj.tag;
            if (tag == Enum.Tag.Hero.ToString())
            {
                var heroId = obj.GetComponent<RoleData>().ID;
                ClickHero(heroId);
            }
            else if (tag == Enum.Tag.Enemy.ToString())
            {

                var enemyId = obj.GetComponent<RoleData>().ID;
                ClickEnemy(enemyId);
            }
            else if (tag == Enum.Tag.Hexagon.ToString())
            {
                var hexagonID = obj.GetComponent<HexagonCellData>().ID;

                var heroID = RoleManager.Instance.GetHeroIDByHexagonID(hexagonID);
                if (heroID > 0)
                {
                    ClickHero(heroID);
                    return;
                }

                var enemyID = RoleManager.Instance.GetEnemyIDByHexagonID(hexagonID);
                if (enemyID > 0)
                {
                    ClickEnemy(enemyID);
                    return;
                }

                ClickHexagon(obj.GetComponent<HexagonCellData>().ID);
            }
        }

        public void ClickHero(int heroID)
        {
            if (_curInstruct == Enum.InstructType.Attack)
            {
                return;
            }
            if (heroID > 0)
            {
                string hexagonID = RoleManager.Instance.GetHexagonIDByHeroID(heroID);
                MapManager.Instance.ClearMarkedRegion();
                if (_initiator > 0 && heroID == _initiator)
                {
                    var hero = RoleManager.Instance.GetHero(_initiator);
                    MapManager.Instance.MarkingRegion(hexagonID, 0, hero.GetAttackDis(), true);
                    OpenInstruct(new Enum.InstructType[] { Enum.InstructType.Attack, Enum.InstructType.Idle, Enum.InstructType.Cancel });
                }
                else
                {
                    _initiator = heroID;
                    var hero = RoleManager.Instance.GetHero(_initiator);
                    MapManager.Instance.MarkingRegion(hexagonID, hero.GetMoveDis(), hero.GetAttackDis(), true);
                }
            }
        }

        public void ClickEnemy(int enemyId)
        {
            if (enemyId > 0)
            {
                if (_curInstruct == Enum.InstructType.Attack)
                {
                    Battle(enemyId);
                }
                else
                {
                    _initiator = 0;
                    string hexagonID = RoleManager.Instance.GetHexagonIDByEnemyID(enemyId);
                    MapManager.Instance.ClearMarkedRegion();
                    var enemy = RoleManager.Instance.GetEnemy(enemyId);
                    MapManager.Instance.MarkingRegion(hexagonID, enemy.GetMoveDis(), enemy.GetAttackDis(), false);
                }
            }
        }

        public void ClickHexagon(string hexagonID)
        {
            if (_curInstruct == Enum.InstructType.Attack)
            {
                return;
            }
            if (_initiator > 0)
            {
                var startID = RoleManager.Instance.GetHexagonIDByHeroID(_initiator);
                Move(startID, hexagonID);
            }
        }

        /// <summary>
        /// 显示操作指令
        /// </summary>
        private void OpenInstruct(Enum.InstructType[] orders = null)
        {
            _locked = true;

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
            _locked = false;
        }

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        private void Move(string start, string end)
        {
            List<string> hexagons = MapManager.Instance.FindingPathForStr(start, end);

            if (null == hexagons || hexagons.Count <= 0)
                return;

            var cost = 0.0f;
            for (int i = 1; i < hexagons.Count; i++)
            {
                cost += MapManager.Instance.GetHexagon(hexagons[i]).GetCost();
            }
            var hero = RoleManager.Instance.GetHero(_initiator);
            if (cost > hero.GetMoveDis())
                return;

            _locked = true;
            _start = start;
            MapManager.Instance.ClearMarkedPath();
            MapManager.Instance.ClearMarkedRegion();
            RoleManager.Instance.MoveHero(_initiator, hexagons);
        }

        /// <summary>
        /// 玩家取消移动后，复位
        /// </summary>
        private void ReverseMove()
        {
            if (null == _start)
                return;

            _locked = true;
            var start = RoleManager.Instance.GetHexagonIDByHeroID(_initiator);
            var path = MapManager.Instance.FindingPathForStr(start, _start);
            RoleManager.Instance.MoveHero(_initiator, path);
        }

        private void Battle(int enemyID)
        {
            var initiatorID = RoleManager.Instance.GetHexagonIDByHeroID(_initiator);
            var targetID = RoleManager.Instance.GetHexagonIDByEnemyID(enemyID);

            List<string> hexagons = MapManager.Instance.FindingPathForStr(initiatorID, targetID, true);
            if (null == hexagons || hexagons.Count <= 0)
                return;
            var cost = 0.0f;
            for (int i = 1; i < hexagons.Count; i++)
            {
                cost += MapManager.Instance.GetHexagon(hexagons[i]).GetCost();
            }

            var hero = RoleManager.Instance.GetHero(_initiator);
            if (cost > hero.GetAttackDis())
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
            if (_initiator <= 0)
                return;

            ReverseMove();
            CloseInstruct();
            MapManager.Instance.ClearMarkedRegion();

            _initiator = 0;
            _start = null;
        }

        private void Attack(params object[] args)
        {
            CloseInstruct();

            _curInstruct = Enum.InstructType.Attack;
            _start = null;
        }

        public void Check(params object[] args)
        {
            UIManager.Instance.OpenPanel("Hero", "HeroPanel");
        }

        public void MoveEnd(params object[] args)
        {
            _locked = false;
            if (_initiator <= 0)
                return;
            var hexagonID = RoleManager.Instance.GetHexagonIDByHeroID(_initiator);
            var hero = RoleManager.Instance.GetHero(_initiator);
            MapManager.Instance.MarkingRegion(hexagonID, 0, hero.GetAttackDis(), true);
            OpenInstruct(new Enum.InstructType[] { Enum.InstructType.Attack, Enum.InstructType.Idle, Enum.InstructType.Cancel });
        }

        public void CreateScene(string mapPath)
        {
            MapManager.Instance.CreateMap(mapPath);

            var attr = new RoleAttribute();
            attr.hp = 100;
            attr.attack = 30;
            attr.defense = 5F;
            var bornPoint = MapTool.Instance.GetHexagonKey(Vector3.zero);
            RoleManager.Instance.CreateHero(1, attr, "Assets/RPG Tiny Hero Duo/Prefab/MaleCharacterPolyart.prefab", bornPoint);

            bornPoint = MapTool.Instance.GetHexagonKey(Vector3.zero + new Vector3(1, 0, 0));
            RoleManager.Instance.CreateHero(2, attr, "Assets/RPG Tiny Hero Duo/Prefab/MaleCharacterPolyart.prefab", bornPoint);

            var bornPoint2 = MapTool.Instance.GetHexagonKey(new Vector3(3, 0, 3));
            RoleManager.Instance.CreateEnemy(11, attr, "Assets/RPG Tiny Hero Duo/Prefab/FemaleCharacterPolyart.prefab", bornPoint2);

            bornPoint2 = MapTool.Instance.GetHexagonKey(new Vector3(3, 0, 4));
            RoleManager.Instance.CreateEnemy(12, attr, "Assets/RPG Tiny Hero Duo/Prefab/FemaleCharacterPolyart.prefab", bornPoint2);

            UIManager.Instance.OpenPanel("Fight", "FightPanel");
        }

        public void DestroyScene()
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
