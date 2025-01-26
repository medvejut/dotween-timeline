using System;
using System.Linq;
using UnityEngine;

namespace Dott.Editor
{
    public class DottView
    {
        private bool isTimeDragging;
        private bool isTweenDragging;

        public event Action TimeDragStart;
        public event Action TimeDragEnd;
        public event Action<float> TimeDrag;
        public event Action<DottAnimation> TweenSelected;
        public event Action<float> TweenDrag;
        public event Action AddClicked;
        public event Action CallbackClicked;
        public event Action RemoveClicked;
        public event Action DuplicateClicked;
        public event Action StopClicked;
        public event Action PlayClicked;

        public void DrawTimeline(DottAnimation[] animations, DottAnimation selected, bool isPlaying, float currentPlayingTime)
        {
            var rect = DottGUI.GetTimelineControlRect(animations.Length);

            DottGUI.Background(rect);
            DottGUI.Header(rect);

            var timeScale = CalculateTimeScale(animations);
            var timeRect = DottGUI.Time(rect, timeScale, ref isTimeDragging, TimeDragStart, TimeDragEnd);
            var tweensRect = DottGUI.Tweens(rect, animations, timeScale, selected, ref isTweenDragging, TweenSelected);

            if (DottGUI.AddButton(rect))
            {
                AddClicked?.Invoke();
            }

            if (DottGUI.CallbackButton(rect))
            {
                CallbackClicked?.Invoke();
            }

            if (selected != null && DottGUI.RemoveButton(rect))
            {
                RemoveClicked?.Invoke();
            }

            if (selected != null && DottGUI.DuplicateButton(rect))
            {
                DuplicateClicked?.Invoke();
            }

            if (isPlaying)
            {
                var time = currentPlayingTime * timeScale;
                DottGUI.TimeVerticalLine(tweensRect, time);
            }

            if (isTimeDragging)
            {
                var time = DottGUI.GetScaledTimeUnderMouse(timeRect);
                DottGUI.TimeVerticalLine(tweensRect, time);

                if (Event.current.type == EventType.MouseDrag)
                {
                    TimeDrag?.Invoke(time / timeScale);
                }
            }

            if (isTweenDragging && selected != null)
            {
                var time = DottGUI.GetScaledTimeUnderMouse(timeRect);

                if (Event.current.type == EventType.MouseDrag)
                {
                    TweenDrag?.Invoke(time / timeScale);
                }
            }

            switch (isPlaying)
            {
                case true when DottGUI.StopButton(rect):
                    StopClicked?.Invoke();
                    break;
                case false when DottGUI.PlayButton(rect):
                    PlayClicked?.Invoke();
                    break;
            }
        }

        public void DrawInspector(UnityEditor.Editor editor)
        {
            DottGUI.Inspector(editor);
        }

        private static float CalculateTimeScale(DottAnimation[] animations)
        {
            var maxTime = animations.Length > 0
                ? animations.Max(animation => animation.Delay + animation.Duration * Mathf.Max(1, animation.Loops))
                : 1f;
            return 1f / maxTime;
        }
    }
}