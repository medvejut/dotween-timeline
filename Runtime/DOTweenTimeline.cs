using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;

namespace Dott
{
    [AddComponentMenu("DOTween/DOTween Timeline")]
    public class DOTweenTimeline : MonoBehaviour
    {
        [CanBeNull] public Sequence Sequence { get; private set; }
		
        //  If TRUE the sequence will ignore Unity's Time.timeScale
        public bool isIndependentUpdate;

        // Do not override the onKill callback because it is used internally to reset the Sequence
        public Sequence Play()
        {
            TryGenerateSequence();
            return Sequence.Play();
        }

        // Wrapper for UnityEvent (requires void return type)
        public void DOPlay() => Play();

        public Sequence Restart()
        {
            TryGenerateSequence();
            Sequence.Restart();
            return Sequence;
        }

        private void TryGenerateSequence()
        {
            if (Sequence != null) { return; }

            Sequence = DOTween.Sequence();
            Sequence.SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            Sequence.OnKill(() => Sequence = null);
            var components = GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                switch (component)
                {
                    case DOTweenAnimation animation:
                        if (!animation.isValid || !animation.isActive) continue;

                        animation.CreateTween(regenerateIfExists: true);
                        Sequence.Insert(0, animation.tween);
                        break;

                    case IDOTweenAnimation animation:
                        if (!animation.IsValid || !animation.IsActive) continue;

                        var tween = animation.CreateTween(regenerateIfExists: true);
                        Sequence.Insert(0, tween);
                        break;
                }
            }
            Sequence.SetUpdate( isIndependentUpdate );
        }

        private void OnDestroy()
        {
            // Already handled by SetLink, but needed to avoid warnings from children DOTweenAnimation.OnDestroy
            Sequence?.Kill();
        }

        public void OnValidate()
        {
            foreach (var doTweenAnimation in GetComponents<DOTweenAnimation>())
            {
                doTweenAnimation.autoGenerate = false;
            }
        }
    }
}