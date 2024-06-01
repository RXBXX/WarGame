using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class Weather
    {
        private float _time = 0;
        private float day = 800f;
        private  Light _light;

        public Weather(float time = 0)
        {
            if (0 == time)
                _time = day / 4;
            else
                _time = time;
            _light = GameObject.FindObjectsOfType<Light>()[0];
        }

        // Update is called once per frame
        public void Update(float deltaTime)
        {
            _time += deltaTime;
            var dayTime = _time % day / day;

            RenderSettings.skybox.SetFloat("_Exposure", 0.7F + Mathf.Sin(dayTime * 2 * Mathf.PI) * 0.3F);
            RenderSettings.skybox.SetFloat("_Rotation", dayTime * 360);

            _light.transform.rotation = Quaternion.Euler(new Vector3(45, dayTime * 360, 180));
            _light.intensity = 0.7F + Mathf.Sin(_time % day / day * 2 * Mathf.PI) * 0.3F;
        }

        public void Dispose()
        { 
        
        }
    }
}
