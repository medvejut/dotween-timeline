using System;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using UnityEditor;
using UnityEngine;

namespace Dott.Editor
{
    [CustomEditor(typeof(DOTweenTimeline))]
    public class DOTweenTimelineEditor : UnityEditor.Editor
    {
        private DOTweenTimeline Timeline => (DOTweenTimeline)target;

        private DottController controller;
        private DottSelection selection;
        private DottView view;
        private float dragTweenTimeShift = -1;
        private DottAnimation[] animations;

        public override bool RequiresConstantRepaint() => true;

        public override void OnInspectorGUI()
        {
            animations = Timeline.GetComponents<ABSAnimationComponent>().Select(DottAnimation.Create).ToArray();
            selection.Validate(animations);

            view.DrawTimeline(animations, selection.Animation, controller.IsPlaying, controller.ElapsedTime);

            if (selection.Animation != null)
            {
                view.DrawInspector(selection.GetAnimationEditor());
            }
        }

        private void OnEnable()
        {
            controller = new DottController();
            selection = new DottSelection();
            view = new DottView();

            view.TweenSelected += OnTweenSelected;
            view.TweenDrag += DragSelectedAnimation;

            view.TimeDragStart += controller.Stop;
            view.TimeDragEnd += controller.Stop;
            view.TimeDrag += GoTo;

            view.AddClicked += AddAnimation;
            view.CallbackClicked += AddCallback;
            view.RemoveClicked += Remove;
            view.DuplicateClicked += Duplicate;

            view.PlayClicked += Play;
            view.StopClicked += controller.Stop;
        }

        private void OnDisable()
        {
            view.TweenSelected -= OnTweenSelected;
            view.TweenDrag -= DragSelectedAnimation;

            view.TimeDragStart -= controller.Stop;
            view.TimeDragEnd -= controller.Stop;
            view.TimeDrag += GoTo;

            view.AddClicked -= AddAnimation;
            view.CallbackClicked -= AddCallback;
            view.RemoveClicked -= Remove;
            view.DuplicateClicked -= Duplicate;

            view.PlayClicked -= Play;
            view.StopClicked -= controller.Stop;

            controller.Stop();
            controller = null;

            selection.Dispose();
            selection = null;

            view = null;

            animations = null;
        }

        private void Play()
        {
            controller.Play(animations);
        }

        private void GoTo(float time)
        {
            controller.GoTo(animations, time);
        }

        private void DragSelectedAnimation(float time)
        {
            if (dragTweenTimeShift < 0)
            {
                dragTweenTimeShift = time - selection.Animation.Delay;
            }

            var delay = time - dragTweenTimeShift;
            delay = Mathf.Max(0, delay);
            delay = (float)Math.Round(delay, 2);
            selection.Animation.Delay = delay;
        }

        private void OnTweenSelected(DottAnimation animation)
        {
            selection.Set(animation);
            // clear focus to correctly update inspector
            GUIUtility.keyboardControl = 0;

            dragTweenTimeShift = -1;
        }

        private void AddAnimation()
        {
            Add<DOTweenAnimation>(Timeline);
        }

        private void AddCallback()
        {
            Add<DOTweenCallback>(Timeline);
        }

        private void Add<T>(DOTweenTimeline timeline) where T : ABSAnimationComponent
        {
            var component = ObjectFactory.AddComponent<T>(timeline.gameObject);
            var animation = DottAnimation.Create(component);
            selection.Set(animation);
        }

        private void Remove()
        {
            Undo.DestroyObjectImmediate(selection.Animation.Component);
            selection.Clear();
        }

        private void Duplicate()
        {
            var source = selection.Animation.Component;

            var dest = source.gameObject.AddComponent(source.GetType());
            EditorUtility.CopySerialized(source, dest);

            var animation = DottAnimation.Create((ABSAnimationComponent)dest);
            selection.Set(animation);
        }
    }
}