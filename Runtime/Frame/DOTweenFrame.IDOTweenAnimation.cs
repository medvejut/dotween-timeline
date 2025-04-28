using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Dott
{
    public partial class DOTweenFrame : IDOTweenAnimation
    {
        private const string ICON_FRAME =
            "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsTAAALEwEAmpwYAAAA1klEQVR4nM2WPQ6CQBBGsaPQeAAVC2oLT8EhrIxaqBXXUW9goyextPAQ2hjKZyZZIon8DOxCfNUEsu9LdpcZPE8JMAY2wFpq7TqtPAJefJE6ciUPgSe/yLPQVu4DN4qRd75NwIlqjk3lC4U8ZVlXPgPeNQISYK6VD4A79XkAwyp5DzjTnKs4ygJi7ImL5FOzl7YkQJAXsMcd27yAXdsBgcMtmnR/yI6u6aX0mpqQfmsfWietopNmlyKtWCE/eH87cIRWR2aKGfrZEHdDPxMyMr8sK6m1Cz8PKLC39qdilAAAAABJRU5ErkJggg==";

        float IDOTweenAnimation.Delay
        {
            get => delay;
            set => delay = value;
        }

        float IDOTweenAnimation.Duration => 0f;
        int IDOTweenAnimation.Loops => 0;
        bool IDOTweenAnimation.IsValid => true;
        bool IDOTweenAnimation.IsActive => true;
        bool IDOTweenAnimation.IsFrom => false;
        public bool AllowEditorCallbacks => true;
        public bool CallbackView => true;
        Texture2D IDOTweenAnimation.CustomIcon => DottUtils.ImageFromString(ICON_FRAME);
        string IDOTweenAnimation.Label => string.IsNullOrEmpty(id) ? "<i>Frame</i>" : id;
        Component IDOTweenAnimation.Component => this;
        public IEnumerable<Object> Targets => properties.Select(property => property.Target);

        Tween IDOTweenAnimation.CreateEditorPreview() => CreateTween(regenerateIfExists: true);
    }
}