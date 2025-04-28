using System;
using System.Linq;
using DG.Tweening;
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
        private float? dragTweenTimeShift;
        private IDOTweenAnimation[] animations;

        public override bool RequiresConstantRepaint() => true;

        public override void OnInspectorGUI()
        {
            animations = Timeline.GetComponents<MonoBehaviour>().Select(DottAnimation.FromComponent).Where(animation => animation != null).ToArray();
            selection.Validate(animations);

            view.DrawTimeline(animations, selection.Animation, controller.IsPlaying, controller.ElapsedTime,
                controller.Loop, controller.Paused);

            if (selection.Animation != null)
            {
                view.DrawInspector(selection.GetAnimationEditor());
            }

            if (controller.Paused && Event.current.type == EventType.Repaint)
            {
                controller.GoTo(animations, controller.ElapsedTime);
            }

            // Smoother ui updates
            if (controller.IsPlaying || view.IsTimeDragging || view.IsTweenDragging)
            {
                Repaint();
            }
        }

        private void OnEnable()
        {
            controller = new DottController();
            selection = new DottSelection();
            view = new DottView();

            view.TweenSelected += OnTweenSelected;
            view.TweenDrag += DragSelectedAnimation;

            view.TimeDragEnd += OnTimeDragEnd;
            view.TimeDrag += GoTo;
            view.PreviewDisabled += controller.Stop;

            view.AddClicked += AddAnimation;
            view.AddMore += AddMore;
            view.RemoveClicked += Remove;
            view.DuplicateClicked += Duplicate;

            view.PlayClicked += Play;
            view.StopClicked += controller.Stop;
            view.LoopToggled += ToggleLoop;

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            view.TweenSelected -= OnTweenSelected;
            view.TweenDrag -= DragSelectedAnimation;

            view.TimeDragEnd -= OnTimeDragEnd;
            view.TimeDrag -= GoTo;
            view.PreviewDisabled -= controller.Stop;

            view.AddClicked -= AddAnimation;
            view.AddMore -= AddMore;
            view.RemoveClicked -= Remove;
            view.DuplicateClicked -= Duplicate;

            view.PlayClicked -= Play;
            view.StopClicked -= controller.Stop;
            view.LoopToggled -= ToggleLoop;

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            controller.Dispose();
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

        private void OnTimeDragEnd(Event mouseEvent)
        {
            const int mouseButtonMiddle = 2;
            if (mouseEvent.IsRightMouseButton() || mouseEvent.button == mouseButtonMiddle)
            {
                controller.Stop();
                return;
            }

            controller.Pause();
        }

        private void DragSelectedAnimation(float time)
        {
            // Sometimes (e.g., for Frame) undo is not recorded when dragging, so we force it
            Undo.RecordObject(selection.Animation.Component, $"Drag {selection.Animation.Label}");

            dragTweenTimeShift ??= time - selection.Animation.Delay;

            var delay = time - dragTweenTimeShift.Value;
            delay = Mathf.Max(0, delay);
            delay = (float)Math.Round(delay, 2);
            selection.Animation.Delay = delay;

            // Complete undo record
            Undo.FlushUndoRecordObjects();
        }

        private void OnTweenSelected(IDOTweenAnimation animation)
        {
            selection.Set(animation);
            // clear focus to correctly update inspector
            GUIUtility.keyboardControl = 0;

            dragTweenTimeShift = null;
        }

        private void AddAnimation()
        {
            Add(Timeline, typeof(DOTweenAnimation));
        }

        private void AddMore(Type type)
        {
            Add(Timeline, type);
        }

        private void Add(DOTweenTimeline timeline, Type type)
        {
            var component = ObjectFactory.AddComponent(timeline.gameObject, type);
            var animation = DottAnimation.FromComponent(component);
            if (controller.Paused)
            {
                animation!.Delay = (float)Math.Round(controller.ElapsedTime, 2);
            }

            selection.Set(animation);
        }

        private void Remove()
        {
            Undo.DestroyObjectImmediate(selection.Animation.Component);
            selection.Clear();
        }

        private void Duplicate()
        {
            Undo.SetCurrentGroupName($"Duplicate {selection.Animation.Label}");

            var source = selection.Animation.Component;

            var dest = Undo.AddComponent(source.gameObject, source.GetType());
            EditorUtility.CopySerialized(source, dest);

            var animation = DottAnimation.FromComponent(dest);
            selection.Set(animation);

            var components = source.GetComponents<Component>();
            var targetIndex = Array.IndexOf(components, source) + 1;
            var index = Array.IndexOf(components, dest);
            while (index > targetIndex)
            {
                UnityEditorInternal.ComponentUtility.MoveComponentUp(dest);
                index--;
            }
        }

        private void ToggleLoop(bool value)
        {
            controller.Loop = value;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange stateChange)
        {
            // Rewind tweens before play mode. OnDisable is too late (runs after dirty state is saved)
            if (stateChange == PlayModeStateChange.ExitingEditMode)
            {
                controller.Stop();
            }
        }
    }
}