using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Dott
{
    public partial class DOTweenCallback : IDOTweenAnimation
    {
        float IDOTweenAnimation.Delay
        {
            get => delay;
            set => delay = value;
        }

        float IDOTweenAnimation.Duration => 0;
        int IDOTweenAnimation.Loops => 0;
        bool IDOTweenAnimation.IsValid => true;
        bool IDOTweenAnimation.IsActive => true;
        bool IDOTweenAnimation.IsFrom => false;
        bool IDOTweenAnimation.CallbackView => true;
        string IDOTweenAnimation.Label => string.IsNullOrEmpty(id) ? "<i>Callback</i>" : id;
        Component IDOTweenAnimation.Component => this;
        public IEnumerable<Object> Targets => Enumerable.Empty<Object>();

        Tween IDOTweenAnimation.CreateEditorPreview() =>
            // Need to return a valid tween for preview playback
            DOTween.Sequence().InsertCallback(delay, () => { });
    }
}