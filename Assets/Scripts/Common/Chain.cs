using UnityEngine;

namespace WarGame
{
    public class Chain
    {
        private int segment = 5;
        private GameObject startGO;
        private GameObject endGO;
        private LineRenderer lr;
        private float droop = 1F;
        private float _time = 0;

        public Chain(GameObject startGO, GameObject endGO, Material mat, int segment = 5, float droop = 0.12f)
        {
            this.startGO = startGO;
            this.endGO = endGO;
            this.segment = segment;
            this.droop = droop;

            lr = endGO.AddComponent<LineRenderer>();
            lr.positionCount = segment + 1;
            lr.material = mat;
            lr.textureMode = LineTextureMode.Tile;
            lr.startWidth = 0.04f;
            lr.endWidth = 0.04f;
            lr.textureScale = new Vector2(6, 1);

            UpdateLine(0);
        }

        public void Update(float deltaTime)
        {
            if (_time < 1)
                _time += deltaTime * 5;

            UpdateLine(_time * _time);
        }

        private void UpdateLine(float lerp)
        {
            var startPos = startGO.transform.position;
            var endPos = (endGO.transform.position - startPos) * lerp + startPos;
            lr.SetPosition(0, startPos);
            for (int i = 1; i < lr.positionCount - 1; i++)
            {
                float index = i;
                var tempDroop = Mathf.Sqrt(droop * (1 - Mathf.Abs((index - segment / 2.0F) / (segment / 2.0F)))) * lerp;
                var frontPart = (segment - index) / segment;
                var backPart = index / segment;
                var posY = startPos.y * frontPart + endPos.y * backPart - tempDroop;
                var posX = startPos.x * frontPart + endPos.x * backPart;
                var posZ = startPos.z * frontPart + endPos.z * backPart;
                lr.SetPosition(i, new Vector3(posX, posY, posZ));
            }

            lr.SetPosition(segment, endPos);
        }

        public void Dispose()
        {
            AssetsMgr.Instance.Destroy(lr);
        }
    }
}
