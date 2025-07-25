using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Dott
{
    public class DOTweenLink : MonoBehaviour, IDOTweenAnimation
    {
        [SerializeField] public string id;
        [SerializeField] public DOTweenTimeline timeline;
        [Min(0)] [SerializeField] public float delay;

        Tween IDOTweenAnimation.CreateTween(bool regenerateIfExists, bool andPlay)
        {
            if (!timeline || !timeline.isActiveAndEnabled)
                return null;

            return timeline.Play().SetDelay(delay, asPrependedIntervalIfSequence: true);
        }

        Tween IDOTweenAnimation.CreateEditorPreview()
        {
            var sequence = DOTween.Sequence();
            foreach (var child in Children(timeline))
            {
                if (child.IsActiveAndValid && child.CreateEditorPreview() is { } tween)
                {
                    sequence.Insert(0, tween);
                }
            }

            sequence.SetDelay(delay, asPrependedIntervalIfSequence: true);
            return sequence;
        }

        float IDOTweenAnimation.Delay
        {
            get => delay;
            set => delay = value;
        }

        float IDOTweenAnimation.Duration => timeline
            ? Children(timeline).Aggregate(0.0f, (current, child) => Mathf.Max(current, child.FullDuration))
            : 1;

        int IDOTweenAnimation.Loops => 0;
        bool IDOTweenAnimation.IsValid => timeline != null;
        bool IDOTweenAnimation.IsActive => timeline == null || timeline.isActiveAndEnabled;
        bool IDOTweenAnimation.IsFrom => false;
        Component IDOTweenAnimation.Component => this;

        string IDOTweenAnimation.Label
        {
            get
            {
                if (timeline == null)
                {
                    return "↪ None";
                }

                if (!string.IsNullOrEmpty(id))
                {
                    return $"↪ {id}";
                }

                return $"↪ {timeline.name}";
            }
        }

        IEnumerable<Object> IDOTweenAnimation.Targets =>
            timeline ? Children(timeline).SelectMany(child => child.Targets) : Enumerable.Empty<Object>();

        private static IEnumerable<IChild> Children(DOTweenTimeline timeline)
        {
            var components = timeline.GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                IChild child = component switch
                {
                    IDOTweenAnimation doChild => new Child(doChild),
                    DOTweenAnimation doChild => new DOChild(doChild),
                    _ => null
                };

                if (child != null)
                {
                    yield return child;
                }
            }
        }

        private interface IChild
        {
            float Duration { get; }
            int Loops { get; }
            float Delay { get; }
            bool IsActiveAndValid { get; }
            float FullDuration => Delay + Duration * Mathf.Max(1, Loops);
            [CanBeNull] Tween CreateEditorPreview();
            IEnumerable<Object> Targets { get; }
        }

        private readonly struct Child : IChild
        {
            private readonly IDOTweenAnimation child;
            public Child(IDOTweenAnimation child) => this.child = child;
            public float Duration => child.Duration;
            public int Loops => child.Loops;
            public float Delay => child.Delay;
            public bool IsActiveAndValid => child.IsActive && child.IsValid;
            public Tween CreateEditorPreview() => child.CreateEditorPreview();
            public IEnumerable<Object> Targets => child.Targets;
        }

        private readonly struct DOChild : IChild
        {
            private readonly DOTweenAnimation child;
            public DOChild(DOTweenAnimation child) => this.child = child;
            public float Duration => child.duration;
            public int Loops => child.loops;
            public float Delay => child.delay;
            public bool IsActiveAndValid => child.isActive && child.isValid;
            public Tween CreateEditorPreview() => child.CreateEditorPreview();
            public IEnumerable<Object> Targets => new[] { child.target };
        }
    }
}