using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarGame.UI;
using UnityEngine.SceneManagement;
using UnityEditor;

namespace WarGame
{
    public class SceneMgr : Singeton<SceneMgr>
    {
        private int _initiator; //当前选中的英雄
        private string _origin; //英雄移动的起点
        private Enum.InstructType _curInstruct = Enum.InstructType.None;
        private bool _locked = false;
        private int _roundIndex = 0;
        private string _curMap = null;
        private bool _isStarted = false;
        private GameObject _heroScene;

        public override bool Init()
        {
            base.Init();

            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Idle_Event, Idle);
            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Attack_Event, Attack);
            EventDispatcher.Instance.AddListener(Enum.EventType.HUDInstruct_Cancel_Event, Cancel);
            EventDispatcher.Instance.AddListener(Enum.EventType.Role_MoveEnd_Event, MoveEnd);
            EventDispatcher.Instance.AddListener(Enum.EventType.Fight_RoundOver_Event, RoundOver);

            return true;
        }

        public override bool Dispose()
        {
            base.Dispose();

            HUDManager.Instance.RemoveHUD("HUDInstruct_Custom");
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Idle_Event, Idle);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Attack_Event, Attack);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.HUDInstruct_Cancel_Event, Cancel);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Role_MoveEnd_Event, MoveEnd);
            EventDispatcher.Instance.RemoveListener(Enum.EventType.Fight_RoundOver_Event, RoundOver);

            return true;
        }

        public override void Update()
        {
            if (!_isStarted)
                return;
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
                    var start = RoleManager.Instance.GetHexagonIDByRoleID(_initiator);
                    var end = obj.GetComponent<HexagonCellData>().ID;

                    var hero = RoleManager.Instance.GetRole(_initiator);
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

                var heroID = RoleManager.Instance.GetRoleIDByHexagonID(hexagonID);
                if (heroID > 0)
                {
                    ClickHero(heroID);
                    return;
                }

                var enemyID = RoleManager.Instance.GetRoleIDByHexagonID(hexagonID);
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
                var hero = RoleManager.Instance.GetRole(heroID);
                if (hero.State == Enum.RoleState.AttackOver)
                    return;

                string hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(heroID);
                MapManager.Instance.ClearMarkedRegion();
                if (_initiator > 0 && heroID == _initiator)
                {
                    MapManager.Instance.MarkingRegion(hexagonID, 0, hero.GetAttackDis(), Enum.RoleType.Hero);
                    OpenInstruct(new Enum.InstructType[] { Enum.InstructType.Attack, Enum.InstructType.Idle, Enum.InstructType.Cancel });
                }
                else
                {
                    MapManager.Instance.MarkingRegion(hexagonID, hero.GetMoveDis(), hero.GetAttackDis(), Enum.RoleType.Hero);
                    if (hero.State != Enum.RoleState.Attacking)
                        return;
                    _initiator = heroID;
                }
            }
        }

        public void ClickEnemy(int enemyId)
        {
            if (enemyId > 0)
            {
                var enemy = RoleManager.Instance.GetRole(enemyId);
                if (enemy.State == Enum.RoleState.AttackOver)
                    return;

                if (_curInstruct == Enum.InstructType.Attack)
                {
                    Battle(enemyId);
                }
                else
                {
                    _initiator = 0;
                    string hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(enemyId);
                    MapManager.Instance.ClearMarkedRegion();
                    MapManager.Instance.MarkingRegion(hexagonID, enemy.GetMoveDis(), enemy.GetAttackDis(), Enum.RoleType.Enemy);
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
                var startID = RoleManager.Instance.GetHexagonIDByRoleID(_initiator);
                Move(startID, hexagonID);
            }
        }

        /// <summary>
        /// 显示操作指令
        /// </summary>
        private void OpenInstruct(Enum.InstructType[] orders = null)
        {
            _locked = true;

            var role = RoleManager.Instance.GetRole(_initiator);
            HUDManager.Instance.AddHUD("HUD", "HUDInstruct", "HUDInstruct_Custom", role.HUDPoint);
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
            List<string> hexagons = MapManager.Instance.FindingPathForStr(start, end, Enum.RoleType.Hero, true);

            if (null == hexagons || hexagons.Count <= 0)
                return;

            var cost = 0.0f;
            for (int i = 1; i < hexagons.Count; i++)
            {
                cost += MapManager.Instance.GetHexagon(hexagons[i]).GetCost();
            }
            var hero = RoleManager.Instance.GetRole(_initiator);
            if (cost > hero.GetMoveDis())
                return;

            _locked = true;
            _origin = start;
            MapManager.Instance.ClearMarkedPath();
            MapManager.Instance.ClearMarkedRegion();
            RoleManager.Instance.MoveRole(_initiator, hexagons);
        }

        /// <summary>
        /// 玩家取消移动后，复位
        /// </summary>
        private void ReverseMove()
        {
            if (null == _origin)
                return;

            _locked = true;
            var start = RoleManager.Instance.GetHexagonIDByRoleID(_initiator);
            var path = MapManager.Instance.FindingPathForStr(start, _origin, Enum.RoleType.Hero, true);
            RoleManager.Instance.MoveRole(_initiator, path);
        }

        private void Battle(int enemyID)
        {
            var initiatorID = RoleManager.Instance.GetHexagonIDByRoleID(_initiator);
            var targetID = RoleManager.Instance.GetHexagonIDByRoleID(enemyID);

            List<string> hexagons = MapManager.Instance.FindingPathForStr(initiatorID, targetID, Enum.RoleType.Hero, false);
            if (null == hexagons || hexagons.Count <= 0)
                return;
            var cost = 0.0f;
            for (int i = 1; i < hexagons.Count; i++)
            {
                cost += MapManager.Instance.GetHexagon(hexagons[i]).GetCost();
            }

            var hero = RoleManager.Instance.GetRole(_initiator);
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

            RoleManager.Instance.NextState(_initiator);
            //var hero = RoleManager.Instance.GetHero(_initiator);
            //hero.NextState();

            _initiator = 0;
            _origin = null;
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
            _origin = null;
        }

        private void Attack(params object[] args)
        {
            CloseInstruct();

            _curInstruct = Enum.InstructType.Attack;
            _origin = null;
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
            var hexagonID = RoleManager.Instance.GetHexagonIDByRoleID(_initiator);
            var hero = RoleManager.Instance.GetRole(_initiator);
            MapManager.Instance.MarkingRegion(hexagonID, 0, hero.GetAttackDis(), Enum.RoleType.Hero);
            OpenInstruct(new Enum.InstructType[] { Enum.InstructType.Attack, Enum.InstructType.Idle, Enum.InstructType.Cancel });
        }

        public void CreateScene(string mapPath)
        {
            MapManager.Instance.CreateMap(mapPath);

            var attr = new RoleAttribute();
            attr.hp = 100;
            attr.attack = 60;
            attr.defense = 5F;
            attr.moveDis = 6;
            attr.attackDis = 3;
            var bornPoint = MapTool.Instance.GetHexagonKey(Vector3.zero);
            RoleManager.Instance.CreateHero(1, attr, "Assets/RPG Tiny Hero Duo/Prefab/MaleCharacterPolyart.prefab", bornPoint);

            bornPoint = MapTool.Instance.GetHexagonKey(Vector3.zero + new Vector3(1, 0, 0));
            RoleManager.Instance.CreateHero(2, attr, "Assets/RPG Tiny Hero Duo/Prefab/MaleCharacterPolyart.prefab", bornPoint);

            attr = new RoleAttribute();
            attr.hp = 100;
            attr.attack = 10;
            attr.defense = 5F;
            attr.moveDis = 5;
            attr.attackDis = 1.0f;
            var bornPoint2 = MapTool.Instance.GetHexagonKey(new Vector3(3, 0, 3));
            RoleManager.Instance.CreateEnemy(11, attr, "Assets/RPG Tiny Hero Duo/Prefab/FemaleCharacterPolyart.prefab", bornPoint2);

            bornPoint2 = MapTool.Instance.GetHexagonKey(new Vector3(3, 0, 4));
            RoleManager.Instance.CreateEnemy(12, attr, "Assets/RPG Tiny Hero Duo/Prefab/FemaleCharacterPolyart.prefab", bornPoint2);

            UIManager.Instance.OpenPanel("Fight", "FightPanel");
            _isStarted = true;

            RoleManager.Instance.NextAllHeroState();
        }

        public void RoundOver(params object[] args)
        {
            _roundIndex += 1;
            EventDispatcher.Instance.Dispatch(Enum.EventType.Fight_Round_Event, new object[] { _roundIndex });
            RoleManager.Instance.NextAllState();
            RoleManager.Instance.NextAllHeroState();
        }

        public void OpenScene(string mapDir)
        {
            _curMap = mapDir;
            SceneManager.LoadSceneAsync("MapScene");
            SceneManager.sceneLoaded += SceneLoaded;
        }

        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            CreateScene(_curMap);
        }

        public void DestroyScene()
        {
            UIManager.Instance.ClosePanel("FightPanel");
            RoleManager.Instance.Clear();
            MapManager.Instance.ClearMap();

            SceneManager.LoadScene("TransitionScene");
            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) =>
            {
                UIManager.Instance.OpenPanel("Map", "MapPanel");
            };
        }

        public void OpenHeroScene(params object[] args)
        {
            var prefab = AssetMgr.Instance.LoadAsset<GameObject>("Assets/Prefabs/HeroScene.prefab");
            _heroScene = GameObject.Instantiate<GameObject>(prefab);
            _heroScene.transform.position = Vector3.one * 10000;
            CameraMgr.Instance.SetMainCamera(_heroScene.transform.Find("Camera").GetComponent<Camera>());
            UIManager.Instance.OpenPanel("Hero", "HeroPanel", args);
            HUDManager.Instance.SetVisible(false);
        }

        public void CloseHeroScene()
        {
            GameObject.Destroy(_heroScene);
            UIManager.Instance.ClosePanel("HeroPanel");
            HUDManager.Instance.SetVisible(true);
        }

        public Transform GetHeroRoot()
        {
            return _heroScene.transform.Find("HeroRoot");
        }
    }
}
