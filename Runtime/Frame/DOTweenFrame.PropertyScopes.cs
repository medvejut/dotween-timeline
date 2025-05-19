using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace Dott
{
    public partial class DOTweenFrame
    {
        [CanBeNull] private static PropertyScope CreateScope(FrameProperty property)
        {
            if (property.Target == null)
            {
                return null;
            }

            return property.Property switch
            {
                FrameProperty.PropertyType.None => null,

                FrameProperty.PropertyType.Position => new MoveScope(property, isLocal: false),
                FrameProperty.PropertyType.LocalPosition => new MoveScope(property, isLocal: true),
                FrameProperty.PropertyType.Scale => new ScaleScope(property),
                FrameProperty.PropertyType.Fade => new FadeScope(property),
                FrameProperty.PropertyType.Color => new ColorScope(property),
                FrameProperty.PropertyType.Active => new ActiveScope(property),
                FrameProperty.PropertyType.Enabled => new EnabledScope(property),

                _ => throw new System.NotImplementedException()
            };
        }

        private abstract class PropertyScope
        {
            protected readonly FrameProperty Property;

            protected PropertyScope(FrameProperty property)
            {
                Property = property;
            }

            public abstract void Open();
            public abstract void Close();
        }

        private class MoveScope : PropertyScope
        {
            private Transform Target => (Transform)Property.Target;
            private Vector3 startValue;
            private readonly bool isLocal;

            public MoveScope(FrameProperty property, bool isLocal) : base(property)
            {
                this.isLocal = isLocal;
            }

            public override void Open()
            {
                startValue = GetValue();

                var endValue = Property.EndValueVector3;
                if (Property.IsRelative)
                {
                    endValue += startValue;
                }

                Apply(endValue);
            }

            public override void Close()
            {
                Apply(startValue);
            }

            private Vector3 GetValue()
            {
                if (isLocal)
                {
                    return Target.localPosition;
                }

                if (Target is RectTransform rectTransform)
                {
                    return rectTransform.anchoredPosition3D;
                }

                return Target.position;
            }

            private void Apply(Vector3 value)
            {
                if (isLocal)
                {
                    Target.localPosition = value;
                    return;
                }

                if (Target is RectTransform rectTransform)
                {
                    rectTransform.anchoredPosition3D = value;
                    return;
                }

                Target.position = value;
            }
        }

        private class ScaleScope : PropertyScope
        {
            private Transform Target => (Transform)Property.Target;
            private Vector3 startValue;

            public ScaleScope(FrameProperty property) : base(property) { }

            public override void Open()
            {
                startValue = Target.localScale;

                var value = Property.EndValueVector3;
                if (Property.IsRelative)
                {
                    value += startValue;
                }

                Target.localScale = value;
            }

            public override void Close()
            {
                Target.localScale = startValue;
            }
        }

        private class FadeScope : PropertyScope
        {
            private float startValue;

            public FadeScope(FrameProperty property) : base(property) { }

            public override void Open()
            {
                var endValue = Property.EndValueFloat;
                Apply(endValue, out startValue);
            }

            public override void Close()
            {
                Apply(startValue, out _);
            }

            private void Apply(float value, out float previousValue)
            {
                switch (Property.Target)
                {
                    case Graphic graphic:
                        previousValue = graphic.color.a;

                        var color = graphic.color;
                        color.a = value;
                        graphic.color = color;
                        break;

                    case CanvasGroup canvasGroup:
                        previousValue = canvasGroup.alpha;
                        canvasGroup.alpha = value;
                        break;

                    default:
                        throw new System.NotImplementedException();
                }
            }
        }

        private class ColorScope : PropertyScope
        {
            private Color startValue;

            public ColorScope(FrameProperty property) : base(property) { }

            public override void Open()
            {
                var endValue = Property.EndValueColor;
                Apply(endValue, out startValue);
            }

            public override void Close()
            {
                Apply(startValue, out _);
            }

            private void Apply(Color color, out Color previousValue)
            {
                switch (Property.Target)
                {
                    case Graphic graphic:
                        previousValue = graphic.color;
                        graphic.color = color;
                        break;

                    case Camera camera:
                        previousValue = camera.backgroundColor;
                        camera.backgroundColor = color;
                        break;

                    default:
                        throw new System.NotImplementedException();
                }
            }
        }

        private class ActiveScope : PropertyScope
        {
            private GameObject Target => Property.TargetGameObject;
            private bool startValue;

            public ActiveScope(FrameProperty property) : base(property) { }

            public override void Open()
            {
                startValue = Property.TargetGameObject.activeSelf;
                Target.SetActive(Property.OptionalBool);
            }

            public override void Close()
            {
                Target.SetActive(startValue);
            }
        }

        private class EnabledScope : PropertyScope
        {
            private Behaviour Target => (Behaviour)Property.Target;
            private bool startValue;

            public EnabledScope(FrameProperty property) : base(property) { }

            public override void Open()
            {
                startValue = Target.enabled;
                Target.enabled = Property.OptionalBool;
            }

            public override void Close()
            {
                Target.enabled = startValue;
            }
        }
    }
}