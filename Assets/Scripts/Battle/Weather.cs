using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class Weather
    {
        public Weather(float time = 1000)
        {
        }

        // Update is called once per frame
        public void Update(float deltaTime)
        {
            var dayTime = TimeMgr.Instance.GetGameTimePercent() - 0.5F;

            //RenderSettings.skybox.SetFloat("_Exposure", 0.7F + Mathf.Sin(dayTime * 2 * Mathf.PI) * 0.3F);
            //RenderSettings.skybox.SetFloat("_Rotation", dayTime * 360);

            SceneMgr.Instance.BattleField.mainLight.transform.rotation = Quaternion.Euler(new Vector3(-dayTime * 360, 45, 0));
            SceneMgr.Instance.BattleField.nightLight.transform.rotation = Quaternion.Euler(new Vector3 (-dayTime * 360 + 180, 45, 0));
            //if (dayTime < 180 && dayTime > 0)
            //{
            //    if (SceneMgr.Instance.BattleField.mainLight.intensity != 1.2F)
            //        SceneMgr.Instance.BattleField.mainLight.intensity = 1.2F;
            //}
            //else
            //{
            //    if (SceneMgr.Instance.BattleField.mainLight.intensity != 0F)
            //        SceneMgr.Instance.BattleField.mainLight.intensity = 0F;
            //}
        }

        //public float GetLightIntensity()
        //{
        //    return 0.7F + Mathf.Sin(_time % day / day * 2 * Mathf.PI) * 0.3F;
        //}

        public void Dispose()
        {

        }
    }
}
