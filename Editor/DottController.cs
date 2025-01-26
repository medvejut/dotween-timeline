using DG.Tweening;
using JetBrains.Annotations;
using UnityEditor;

namespace Dott.Editor
{
    public class DottController
    {
        private double startTime;

        public bool IsPlaying => DottEditorPreview.IsPlaying;
        public float ElapsedTime => (float)(DottEditorPreview.CurrentTime - startTime);

        public void Play(DottAnimation[] animations)
        {
            animations.ForEach(PreviewTween);
            DottEditorPreview.Start();
            startTime = DottEditorPreview.CurrentTime;
        }

        public void GoTo(DottAnimation[] animations, in float time)
        {
            DottEditorPreview.Stop();

            foreach (var animation in animations)
            {
                var tween = PreviewTween(animation);
                if (tween != null)
                {
                    var tweenTime = time - animation.Delay;
                    tween.Goto(tweenTime, andPlay: false);
                }
            }
        }

        public void Stop()
        {
            DottEditorPreview.Stop();
        }

        [CanBeNull]
        private static Tween PreviewTween(DottAnimation animation)
        {
            if (!animation.IsValid || !animation.IsActive) { return null; }

            var tween = animation.CreateEditorPreview();
            if (tween == null) { return null; }

            DottEditorPreview.Add(tween, animation.IsFrom);
            return tween;
        }
    }
}