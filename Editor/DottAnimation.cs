using DG.Tweening;
using DG.Tweening.Core;
using JetBrains.Annotations;

namespace Dott.Editor
{
    public class DottAnimation
    {
        private IAdapter adapter;

        public float Delay
        {
            get => adapter.Delay;
            set => adapter.Delay = value;
        }

        public float Duration => adapter.Duration;
        public int Loops => adapter.Loops;
        public ABSAnimationComponent Component => adapter.Component;
        public bool IsValid => adapter.IsValid;
        public bool IsActive => adapter.IsActive;
        public bool IsFrom => adapter.IsFrom;
        [CanBeNull] public Tween CreateEditorPreview() => adapter.CreateEditorPreview();
        public string Label() => adapter.Label();

        public static bool operator ==(DottAnimation a, DottAnimation b) => a?.Component == b?.Component;
        public static bool operator !=(DottAnimation a, DottAnimation b) => !(a == b);
        public override bool Equals(object obj) => obj is DottAnimation animation && this == animation;
        public override int GetHashCode() => Component.GetHashCode();

        public static DottAnimation Create(ABSAnimationComponent animation)
        {
            IAdapter adapter = animation switch
            {
                DOTweenAnimation doTweenAnimation => new AnimationAdapter(doTweenAnimation),
                DOTweenCallback callback => new CallbackAdapter(callback),
                _ => new EmptyAdapter(animation)
            };

            return new DottAnimation { adapter = adapter };
        }

        private interface IAdapter
        {
            float Delay { get; set; }
            float Duration { get; }
            int Loops { get; }
            ABSAnimationComponent Component { get; }
            bool IsValid { get; }
            bool IsActive { get; }
            bool IsFrom { get; }
            [CanBeNull] Tween CreateEditorPreview();
            string Label();
        }

        private struct CallbackAdapter : IAdapter
        {
            private readonly DOTweenCallback callback;

            public float Delay
            {
                get => callback.delay;
                set => callback.delay = value;
            }

            public float Duration => 0.2f;
            public int Loops => 0;
            public ABSAnimationComponent Component => callback;
            public bool IsValid => true;
            public bool IsActive => true;
            public bool IsFrom => false;

            public Tween CreateEditorPreview() =>
                // Need to return a valid tween for preview playback
                DOTween.Sequence().InsertCallback(Delay + Duration, () => { });

            public string Label() => $"[{callback.id}]";

            public CallbackAdapter(DOTweenCallback callback)
            {
                this.callback = callback;
            }
        }

        private struct AnimationAdapter : IAdapter
        {
            private readonly DOTweenAnimation doTweenAnimation;

            public float Delay
            {
                get => doTweenAnimation.delay;
                set => doTweenAnimation.delay = value;
            }

            public float Duration => doTweenAnimation.duration;
            public int Loops => doTweenAnimation.loops;
            public ABSAnimationComponent Component => doTweenAnimation;
            public bool IsValid => doTweenAnimation.isValid;
            public bool IsActive => doTweenAnimation.isActive;
            public bool IsFrom => doTweenAnimation.isFrom;
            public Tween CreateEditorPreview() => doTweenAnimation.CreateEditorPreview();

            public string Label()
            {
                var infiniteSuffix = Loops == -1 ? "∞" : "";
                if (!string.IsNullOrEmpty(doTweenAnimation.id))
                {
                    return $"{doTweenAnimation.id} {infiniteSuffix}";
                }

                if (doTweenAnimation.animationType == DOTweenAnimation.AnimationType.None)
                {
                    return doTweenAnimation.animationType.ToString();
                }

                if (doTweenAnimation.target == null)
                {
                    return "Invalid target";
                }

                return $"{doTweenAnimation.target.name}.{doTweenAnimation.animationType} {infiniteSuffix}";
            }

            public AnimationAdapter(DOTweenAnimation doTweenAnimation)
            {
                this.doTweenAnimation = doTweenAnimation;
            }
        }

        private struct EmptyAdapter : IAdapter
        {
            public float Delay
            {
                get => 0;
                set { }
            }

            public float Duration => 0.2f;
            public int Loops => 0;
            public ABSAnimationComponent Component { get; }
            public bool IsValid => false;
            public bool IsActive => false;
            public bool IsFrom => false;
            public Tween CreateEditorPreview() => null;
            public string Label() => "Unknown type";

            public EmptyAdapter(ABSAnimationComponent animation)
            {
                Component = animation;
            }
        }
    }
}