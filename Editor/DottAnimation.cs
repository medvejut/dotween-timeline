using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dott.Editor
{
    public static class DottAnimation
    {
        [CanBeNull] public static IDOTweenAnimation FromComponent(Component component)
        {
            return component switch
            {
                DOTweenAnimation animation => new AnimationAdapter(animation),
                IDOTweenAnimation animation => animation,
                _ => null
            };
        }

        private class AnimationAdapter : IDOTweenAnimation
        {
            private readonly DOTweenAnimation animation;

            public Tween CreateTween(bool regenerateIfExists, bool andPlay = true)
                // Used only in runtime, not needed in editor
                => throw new System.NotImplementedException();

            public float Delay
            {
                get => animation.delay;
                set => animation.delay = value;
            }

            public float Duration => animation.duration;
            public int Loops => animation.loops;
            public bool IsValid => animation.isValid;
            public bool IsActive => animation.isActive;
            public bool IsFrom => animation.isFrom;

            public string Label
            {
                get
                {
                    var infiniteSuffix = Loops == -1 ? "∞" : "";
                    if (!string.IsNullOrEmpty(animation.id))
                    {
                        return $"{animation.id} {infiniteSuffix}";
                    }

                    if (animation.animationType == DOTweenAnimation.AnimationType.None)
                    {
                        return animation.animationType.ToString();
                    }

                    if (animation.target == null)
                    {
                        return "Invalid target";
                    }

                    return $"{animation.target.name}.{animation.animationType} {infiniteSuffix}";
                }
            }

            public Component Component => animation;
            public IEnumerable<Object> Targets => animation.target != null ? new[] { animation.target } : Enumerable.Empty<Object>();
            public Tween CreateEditorPreview() => animation.CreateEditorPreview();

            public AnimationAdapter(DOTweenAnimation animation)
            {
                this.animation = animation;
            }

            // Needed for comparison with selected animation
            public override bool Equals(object obj) => obj is AnimationAdapter other && animation == other.animation;
            public override int GetHashCode() => animation.GetHashCode();
        }
    }
}