using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Dott
{
    [AddComponentMenu("DOTween/DOTween Callback")]
    public class DOTweenCallback : ABSAnimationComponent
    {
        [SerializeField] public string id;
        [SerializeField] public float delay;
        [SerializeField] public UnityEvent onCallback;
        [SerializeField] public bool autoGenerate = true;
        [SerializeField] public bool autoPlay = true;

        private void Awake()
        {
            if (autoGenerate)
            {
                CreateTween(autoPlay);
            }
        }

        public void CreateTween(bool andPlay = true)
        {
            if (tween != null)
            {
                if (tween.active)
                {
                    tween.Kill();
                }

                tween = null;
            }

            tween = DOTween.Sequence().InsertCallback(delay, () => onCallback.Invoke());

            if (andPlay)
            {
                tween.Play();
            }
            else
            {
                tween.Pause();
            }
        }

        #region DOTweenVisualManager dependencies (empty, not supported)

        public override void DOPlay()
        {
        }

        public override void DOPlayBackwards()
        {
        }

        public override void DOPlayForward()
        {
        }

        public override void DOPause()
        {
        }

        public override void DOTogglePause()
        {
        }

        public override void DORewind()
        {
        }

        public override void DORestart()
        {
        }

        public override void DORestart(bool fromHere)
        {
        }

        public override void DOComplete()
        {
        }

        public override void DOGotoAndPause(float time)
        {
        }

        public override void DOGotoAndPlay(float time)
        {
        }

        public override void DOKill()
        {
        }

        #endregion
    }
}