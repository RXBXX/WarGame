using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class Weather
    {
        private float _time = 0;
        private float day = 1800f;

        public Weather(float time = 1000)
        {
            if (0 == time)
                _time = day / 4;
            else
                _time = time;
        }

        // Update is called once per frame
        public void Update(float deltaTime)
        {
            _time += deltaTime;
            var dayTime =1 -  _time % day / day;

            //RenderSettings.skybox.SetFloat("_Exposure", 0.7F + Mathf.Sin(dayTime * 2 * Mathf.PI) * 0.3F);
            //RenderSettings.skybox.SetFloat("_Rotation", dayTime * 360);

            SceneMgr.Instance.BattleField.mainLight.transform.rotation = Quaternion.Euler(new Vector3(dayTime * 360, 45, 0));
            //SceneMgr.Instance.BattleField.mainLight.intensity = Mathf.Min(1.2F, 1.0F + Mathf.Sin(_time % day / day * 2 * Mathf.PI) * 1F);
        }

        public float GetLightIntensity()
        {
            return 0.7F + Mathf.Sin(_time % day / day * 2 * Mathf.PI) * 0.3F;
        }

        public void Dispose()
        { 
        
        }
    }
}
