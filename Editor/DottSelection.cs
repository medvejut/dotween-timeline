using System.Linq;
using UnityEngine;

namespace Dott.Editor
{
    public class DottSelection
    {
        private static DottAnimation animation;
        private UnityEditor.Editor editor;

        public DottAnimation Animation => animation;

        public void Validate(DottAnimation[] animations)
        {
            if (animation != null && !animations.Contains(animation))
            {
                Clear();
            }
        }

        public void Set(DottAnimation animation)
        {
            DottSelection.animation = animation;
        }

        public void Clear() => Set(null);

        public UnityEditor.Editor GetAnimationEditor()
        {
            if (editor != null && editor.target != animation.Component)
            {
                DisposeEditor();
            }

            if (animation == null)
            {
                return null;
            }

            if (editor == null)
            {
                editor = UnityEditor.Editor.CreateEditor(animation.Component);
            }

            return editor;
        }

        public void Dispose()
        {
            if (editor != null)
            {
                DisposeEditor();
            }
        }

        private void DisposeEditor()
        {
            Object.DestroyImmediate(editor);
            editor = null;
        }
    }
}