namespace game
{
    using UnityEngine;
    using UnityEngine.UI;

    public class FPS : MonoBehaviour
    {
        [SerializeField] private bool isVisible;
        [SerializeField] private float mUpdateInterval = 0.1f;
        [SerializeField] private int mMaxFps = 60;
        [SerializeField] private Text fpsText;
     
        private float mAccum;
        private int mFrames;
        private float mTimeleft;
        private float mCalculateFps;

        public void Start()
        {
            mTimeleft = mUpdateInterval;
            mAccum = 0.0f;
            mFrames = 0;
        }

        public void Update()
        {
            mTimeleft -= Time.deltaTime;
            float fps = Time.timeScale / Time.deltaTime;

            mAccum += fps;
            ++mFrames;

            if (mTimeleft <= 0)
            {
                mCalculateFps = mAccum / mFrames;
                if (mCalculateFps > mMaxFps)
                {
                    mCalculateFps = mMaxFps;
                }
                fpsText.text = "FPS:" + mCalculateFps.ToString("####");
                this.Start();
            }
        }
    }
}
