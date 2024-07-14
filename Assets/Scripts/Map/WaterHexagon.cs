using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarGame
{
    public class WaterHexagon : Hexagon
    {
        private float _floatOffset;
        public WaterHexagon(int id, int configId, bool isReachable, WGVector3 coor) : base(id, configId, isReachable, coor)
        {
            var x = coor.x / 5.0f;
            var z = coor.z / 5.0f;
            _floatOffset = x * x + z * z;
        }

        public override void Update(float deltaTime)
        {
            if (null == _gameObject)
                return;

            var yFloat = (Mathf.Sin(TimeMgr.Instance.GetTimeSinceLevelLoad() + _floatOffset) + 1) / 2.0f;
            //var newCoor = new CustomVector3(coor.x, coor.y - (Mathf.Sin(TimeMgr.Instance.GetTimeSinceLevelLoad()+ _floatOffset) + 1) / 2.0f, coor.z);
            _gameObject.transform.position = MapTool.Instance.GetPosFromCoor(coor) - new Vector3(0, yFloat * CommonParams.Offset.y, 0);
        }
    }
}
