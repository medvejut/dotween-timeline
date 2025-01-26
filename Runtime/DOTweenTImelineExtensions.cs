using DG.Tweening;
using DG.Tweening.Core;

namespace Dott
{
    public static class DOTweenTimelineExtensions
    {
        public static Sequence AsSequence(this DOTweenTimeline timeline)
        {
            var sequence = DOTween.Sequence();
            var components = timeline.GetComponents<ABSAnimationComponent>();
            foreach (var component in components)
            {
                switch (component)
                {
                    case DOTweenAnimation animation:
                        animation.CreateTween(regenerateIfExists: true);
                        break;

                    case DOTweenCallback callback:
                        callback.CreateTween();
                        break;
                }

                sequence.Insert(0, component.tween);
            }

            return sequence;
        }
    }
}