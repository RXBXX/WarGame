using FairyGUI;

namespace WarGame.UI
{
    public class StoryItem : UIBase
    {
        private Transition _fadeIn;
        private GButton _pic1, _pic2, _pic3;

        public StoryItem(GComponent gCom, string name, object[] args) : base(gCom, name)
        {
            _fadeIn = GetTransition("fadeIn");
            _pic1 = GetGObjectChild<GButton>("pic1");
            _pic2 = GetGObjectChild<GButton>("pic2");
            _pic3 = GetGObjectChild<GButton>("pic3");

            _fadeIn.SetHook("Pic1", () => {
                AudioMgr.Instance.PlaySound("Assets/Audios/Story_FadeIn.wav");
            });
            _fadeIn.SetHook("Pic2", () => {
                AudioMgr.Instance.PlaySound("Assets/Audios/Story_FadeIn.wav");
            });
            _fadeIn.SetHook("Pic3", () => {
                AudioMgr.Instance.PlaySound("Assets/Audios/Story_FadeIn.wav");
            });
        }

        public void Play(int storyID, WGCallback callback)
        {
            var storyConfig = ConfigMgr.Instance.GetConfig<StoryConfig>("StoryConfig", storyID);
            if (null != storyConfig.Pic1)
            {
                _pic1.icon = storyConfig.Pic1;
                _pic2.icon = storyConfig.Pic2;
                _pic3.icon = storyConfig.Pic3;
                _fadeIn.Play();
            }
            callback();
        }
    }
}
