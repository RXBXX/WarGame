using DG.Tweening;
using WarGame.UI;
using UnityEngine;
using System.Collections.Generic;

namespace WarGame
{
    public class Game : Singeton<Game>
    {
        public override bool Init()
        {
            //DebugManager.Instance.Log("Game.Init");
            base.Init();

            AssetsMgr.Instance.Init();
            TimeMgr.Instance.Init();
            DebugManager.Instance.Init();
            EventDispatcher.Instance.Init();
            DatasMgr.Instance.Init();
            AudioMgr.Instance.Init();
            ConfigMgr.Instance.Init();
            CoroutineMgr.Instance.Init();
            UIManager.Instance.Init();
            TipsMgr.Instance.Init();
            DialogMgr.Instance.Init();
            SceneMgr.Instance.Init();
            HUDManager.Instance.Init();
            CameraMgr.Instance.Init();
            InputManager.Instance.Init();
            MapTool.Instance.Init();
            MapManager.Instance.Init();
            LineMgr.Instance.Init();
            RoleManager.Instance.Init();
            BattleMgr.Instance.Init();
            EventMgr.Instance.Init();

            DOTween.Init(true, false);
            DOTween.debugMode = true;
            
            return true;
        }

        public bool Start()
        {
            //Time.timeScale = 10;
            UIManager.Instance.OpenPanel("Login", "LoginPanel");
            AudioMgr.Instance.PlayMusic("Assets/Audios/BG_Music.mp3");

            //DebugManager.Instance.Log(Application.temporaryCachePath);
            //DebugManager.Instance.Log("WarGame Start!");
            //DebugManager.Instance.LogError("WarGame Start Error!");
            return true;
        }

        public override void Update(float deltaTime)
        {
            //DebugManager.Instance.Log("Update");
            DatasMgr.Instance.Update(deltaTime);
            DebugManager.Instance.Update(deltaTime);
            EventDispatcher.Instance.Update(deltaTime);
            UIManager.Instance.Update(deltaTime);
            SceneMgr.Instance.Update(deltaTime);
            HUDManager.Instance.Update(deltaTime);
            CameraMgr.Instance.Update(deltaTime);
            InputManager.Instance.Update(deltaTime);
            MapTool.Instance.Update(deltaTime);
            MapManager.Instance.Update(deltaTime);
            LineMgr.Instance.Update(deltaTime);
            RoleManager.Instance.Update(deltaTime);
            RenderMgr.Instance.Update(deltaTime);
            AudioMgr.Instance.Update(deltaTime);

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.A))
            {
                //var itemsDic = new List<TwoIntPair>();
                //for (int i = 1; i < 4; i++)
                //{
                //    itemsDic.Add(new TwoIntPair(i, 100));
                //}

                //WGCallback cb = () => { DebugManager.Instance.Log("OnSuccess"); };
                //UIManager.Instance.OpenPanel("Reward", "RewardItemsPanel", new object[] { itemsDic, cb });
                var _sequences = new List<Sequence>();
                var itemConfig = ConfigMgr.Instance.GetConfig<ItemConfig>("ItemConfig", 1);
                int assetID = 0;
                var pos = Vector3.zero;
                assetID = AssetsMgr.Instance.LoadAssetAsync<GameObject>(itemConfig.Prefab, (prefab) =>
                {
                    var count = 5;
                    for (int i = 0; i < count; i++)
                    {
                        var go = GameObject.Instantiate(prefab);
                        AudioMgr.Instance.PlaySound("Assets/Audios/Drop.mp3", false, go, 1, 6);
                        go.transform.position = Vector3.zero;
                        Sequence seq = DOTween.Sequence();
                        seq.Append(go.transform.DOMove(pos + new Vector3(Random.Range(-0.5F, 0.5F), Random.Range(0F, 0.5F), Random.Range(-0.5F, 0.5F)), 0.1f));
                        seq.AppendInterval(Random.Range(0.1F, 0.4F));
                        seq.Append(go.transform.DOMove(pos + new Vector3(0, 2, 0), 0.2F));
                        seq.AppendCallback(() =>
                        {
                            AudioMgr.Instance.ClearSound(go);
                            AssetsMgr.Instance.Destroy(go);
                        });
                        seq.onComplete = () => { _sequences.Remove(seq); };
                    }
                });

                DialogMgr.Instance.OpenDialog(20003);
            }

            //EventMgr.Instance.TriggerEvent(50);
            //DialogMgr.Instance.OpenDialog(20003);
#endif

        }

        public override void FixedUpdate(float deltaTime)
        {
            //DebugManager.Instance.Log("FixedUpdate");
        }

        public override void LateUpdate()
        {
            DebugManager.Instance.LateUpdate();
            EventDispatcher.Instance.LateUpdate();
            UIManager.Instance.LateUpdate();
            SceneMgr.Instance.LateUpdate();
            HUDManager.Instance.LateUpdate();
            CameraMgr.Instance.LateUpdate();
            InputManager.Instance.LateUpdate();
            MapTool.Instance.LateUpdate();
            MapManager.Instance.LateUpdate();
            LineMgr.Instance.LateUpdate();
            RoleManager.Instance.LateUpdate();
        }

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            RenderMgr.Instance.OnRenderImage(source, destination);
        }

        public override bool Dispose()
        {
            EventMgr.Instance.Dispose();
            BattleMgr.Instance.Dispose();
            CameraMgr.Instance.Dispose();
            InputManager.Instance.Dispose();
            MapTool.Instance.Dispose();
            MapManager.Instance.Dispose();
            SceneMgr.Instance.Dispose();
            LineMgr.Instance.Dispose();
            HUDManager.Instance.Dispose();
            RoleManager.Instance.Dispose();
            DialogMgr.Instance.Dispose();
            TipsMgr.Instance.Dispose();
            UIManager.Instance.Dispose();
            CoroutineMgr.Instance.Dispose();
            ConfigMgr.Instance.Dispose();
            DatasMgr.Instance.Dispose();
            EventDispatcher.Instance.Dispose();
            DebugManager.Instance.Dispose();
            TimeMgr.Instance.Dispose();
            AudioMgr.Instance.Dispose();
            AssetsMgr.Instance.Dispose();

            System.GC.Collect();
            return base.Dispose();
        }

        public void Quit()
        {
            DatasMgr.Instance.Save();
            Dispose();
            Application.Quit();
        }
    }
}
