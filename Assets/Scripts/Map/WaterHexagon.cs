using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class WaterHexagon : Hexagon
    {
        private float _floatOffset;
        public WaterHexagon(int id, int configId, bool isReachable, Vector3 coor) : base(id, configId, isReachable, coor)
        {
            var x = coor.x / 5.0f;
            var z = coor.z / 5.0f;
            _floatOffset = x * x + z * z;
        }

        public override void Update(float deltaTime)
        {
            if (null == _gameObject)
                return;

            var newCoor = new Vector3(coor.x, coor.y - (Mathf.Sin(TimeMgr.Instance.GetTimeSinceLevelLoad()+ _floatOffset) + 1) / 2.0f, coor.z);
            _gameObject.transform.position = MapTool.Instance.GetPosFromCoor(newCoor);
        }
    }
}
