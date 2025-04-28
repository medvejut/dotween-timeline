using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Dott
{
    public partial class DOTweenFrame : MonoBehaviour
    {
        [SerializeField] private string id;
        [Min(0), SerializeField] private float delay;
        [SerializeField] private FrameProperty[] properties = new FrameProperty[1];

        private Sequence tween;
        public FrameProperty[] Properties => properties;

        public Tween CreateTween(bool regenerateIfExists, bool andPlay = true)
        {
            if (tween != null)
            {
                if (tween.IsActive())
                {
                    if (!regenerateIfExists)
                    {
                        return tween;
                    }

                    tween.Kill();
                }

                tween = null;
            }

            tween = DOTween.Sequence().SetDelay(delay);
            var scopes = GenerateScopes();

            var isScopeOpen = false;
            tween.OnComplete(() =>
            {
                if (isScopeOpen)
                {
                    return;
                }

                isScopeOpen = true;
                foreach (var scope in scopes)
                {
                    scope.Open();
                }
            });

            tween.OnRewind(() =>
            {
                // Sequences starts without real delay, so onRewind can be called before onComplete
                if (!isScopeOpen)
                {
                    return;
                }

                isScopeOpen = false;
                foreach (var scope in scopes)
                {
                    scope.Close();
                }
            });

            return tween;
        }

        private PropertyScope[] GenerateScopes()
        {
            return properties.Select(CreateScope).Where(scope => scope != null).ToArray();
        }
    }
}