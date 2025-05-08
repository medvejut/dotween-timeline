using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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
            return timeline && timeline.isActiveAndEnabled ? timeline.Play().SetDelay(delay, asPrependedIntervalIfSequence: true) : null;
        }

        Tween IDOTweenAnimation.CreateEditorPreview()
        {
            var sequence = DOTween.Sequence();
            foreach (var child in Children(timeline))
            {
                if (child.CreateEditorPreview() is { } tween)
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

        float IDOTweenAnimation.Duration =>
            timeline ? Children(timeline).Select(child => child.FullDuration).Max() : 1;

        int IDOTweenAnimation.Loops => 0;
        bool IDOTweenAnimation.IsValid => timeline != null;
        bool IDOTweenAnimation.IsActive => timeline && timeline.isActiveAndEnabled;
        bool IDOTweenAnimation.IsFrom => false;
        Component IDOTweenAnimation.Component => this;

        string IDOTweenAnimation.Label
        {
            get
            {
                if (timeline == null)
                {
                    return "Invalid timeline";
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

        private static IEnumerable<Child> Children(DOTweenTimeline timeline)
        {
            var components = timeline.GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                switch (component)
                {
                    case DOTweenAnimation doChild:
                        yield return new Child(null, doChild);
                        break;

                    case IDOTweenAnimation child:
                        yield return new Child(child, null);
                        break;
                }
            }
        }

        private readonly struct Child
        {
            private readonly IDOTweenAnimation child;
            private readonly DOTweenAnimation doChild;

            public Child(IDOTweenAnimation child, DOTweenAnimation doChild)
            {
                this.child = child;
                this.doChild = doChild;
            }

            private float Duration => child?.Duration ?? doChild.duration;
            private int Loops => child?.Loops ?? doChild.loops;
            public float FullDuration => Duration * Mathf.Max(1, Loops);

            public Tween CreateEditorPreview()
            {
                if (child != null)
                {
                    return child.CreateEditorPreview();
                }

                if (doChild != null)
                {
                    return doChild.CreateEditorPreview();
                }

                return null;
            }

            public IEnumerable<Object> Targets => child?.Targets ?? new[] { doChild.target };
        }
    }
}