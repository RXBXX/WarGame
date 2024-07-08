using UnityEngine;

namespace WarGame
{
    public class PalettePP :PostProcessing
    {
        private int _assetID;

        public PalettePP(params object[] args) : base(args)
        {
            _assetID = AssetsMgr.Instance.LoadAssetAsync<Material>("Assets/Materials/PaletteMat.mat", (Material mat) => {
                _mat = mat;
                _mat.SetColor("_Color", (Color)args[0]);
            });
        }

        public override RenderTexture Render(RenderTexture source, RenderTexture destination)
        {
            if (null == _mat)
                return null;
            Graphics.Blit(source, destination, _mat);
            return destination;
        }

        public override void Clear()
        {
            AssetsMgr.Instance.ReleaseAsset(_assetID);
            _assetID = 0;
            base.Clear();
        }
    }
}