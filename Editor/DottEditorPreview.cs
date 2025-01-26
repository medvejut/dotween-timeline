using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEditor;

namespace Dott.Editor
{
    public static class DottEditorPreview
    {
        private struct TweenData
        {
            public readonly Tween Tween;
            public readonly bool IsFrom;

            public TweenData(Tween tween, bool isFrom)
            {
                Tween = tween;
                IsFrom = isFrom;
            }
        }

        private static readonly List<TweenData> Tweens = new();

        public static bool IsPlaying { get; private set; }
        public static double CurrentTime { get; private set; }

        static DottEditorPreview()
        {
        }

        public static void Start()
        {
            if (IsPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            IsPlaying = true;
            CurrentTime = EditorApplication.timeSinceStartup;
            EditorApplication.update += Update;
        }

        public static void Stop()
        {
            IsPlaying = false;
            EditorApplication.update -= Update;

            for (var i = Tweens.Count - 1; i >= 0; i--)
            {
                var tweenData = Tweens[i];
                if (tweenData.IsFrom)
                {
                    tweenData.Tween.Complete();
                }
                else
                {
                    tweenData.Tween.Rewind();
                }

                tweenData.Tween.Kill();
            }

            Tweens.Clear();
        }

        public static void Add([NotNull] Tween tween, bool isFrom)
        {
            Tweens.Add(new TweenData(tween, isFrom));
            tween.SetUpdate(UpdateType.Manual);
            tween.SetAutoKill(false);
            tween.OnComplete(null).OnStart(null).OnPlay(null).OnPause(null).OnUpdate(null).OnWaypointChange(null).OnStepComplete(null).OnRewind(null).OnKill(null);
            tween.Play();
        }

        private static void Update()
        {
            var prevTime = CurrentTime;
            CurrentTime = EditorApplication.timeSinceStartup;
            var delta = CurrentTime - prevTime;
            DOTween.ManualUpdate((float)delta, (float)delta);

            var activeTweens = Tweens.Any(tweenData => tweenData.Tween.IsPlaying());
            if (!activeTweens)
            {
                Stop();
            }
        }
    }
}