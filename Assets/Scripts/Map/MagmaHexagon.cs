using UnityEngine;

namespace WarGame
{
    public class MagmaHexagon : Hexagon
    {
        private float _floatOffset;
        public MagmaHexagon(int id, int configId, bool isReachable, Vector3 coor) : base(id, configId, isReachable, coor)
        {
            _floatOffset = coor.y / 5.0f;
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
