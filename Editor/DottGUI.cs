using System;
using DG.DemiEditor;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dott.Editor
{
    public static class DottGUI
    {
        private const float ROW_HEIGHT = 20;
        private const int BOTTOM_HEIGHT = 30;
        private const int TIME_HEIGHT = 20;
        private const int HEADER_HEIGHT = 28;
        private static readonly Vector2 PlayButtonSize = new(44, 24);

        private static readonly Color[] Colors =
        {
            Color.red, Color.green, Color.blue,
            Color.yellow, Color.cyan, Color.magenta
        };

        public static Rect GetTimelineControlRect(int tweenCount)
        {
            return EditorGUILayout.GetControlRect(false, HEADER_HEIGHT + TIME_HEIGHT + tweenCount * ROW_HEIGHT + BOTTOM_HEIGHT);
        }

        public static void Background(Rect rect)
        {
            RoundRect(rect, Color.black.SetAlpha(0.3f), borderRadius: 4);
            RoundRect(rect, Color.black, borderRadius: 4, borderWidth: 1);
        }

        public static void Header(Rect rect)
        {
            rect = rect.SetHeight(HEADER_HEIGHT);

            var style = EditorStyles.boldLabel;
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(rect, "Timeline", style);

            var bottomLine = new Rect(rect.x, rect.y + rect.height, rect.width, 1);
            EditorGUI.DrawRect(bottomLine, Color.black);
        }

        public static Rect Time(Rect rect, float timeScale, ref bool isDragging, Action start, Action end)
        {
            rect = rect.ShiftY(HEADER_HEIGHT).SetHeight(TIME_HEIGHT);

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 9, normal = { textColor = Color.white.SetAlpha(0.5f) }
            };

            const int count = 10;
            const float step = 1f / count;
            for (var i = 0; i < count; i++)
            {
                var time = i * step;
                var position = new Rect(rect.x + i * step * rect.width, rect.y, step * rect.width, rect.height);
                time /= timeScale;
                GUI.Label(position, time.ToString("0.00"), style);
            }

            var bottomLine = new Rect(rect.x, rect.y + rect.height, rect.width, 1);
            EditorGUI.DrawRect(bottomLine, Color.black);

            ProcessDragEvents(rect, ref isDragging, start, end);

            return rect;
        }

        private static void ProcessDragEvents(Rect rect, ref bool isDragging, Action start, Action end)
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown when !isDragging && rect.Contains(Event.current.mousePosition):
                    isDragging = true;
                    start?.Invoke();
                    break;

                case EventType.MouseUp when isDragging:
                    isDragging = false;
                    end?.Invoke();
                    break;
            }
        }

        public static float GetScaledTimeUnderMouse(Rect timeRect)
        {
            var time = (Event.current.mousePosition.x - timeRect.x) / timeRect.width;
            time = Mathf.Clamp01(time);
            return time;
        }

        public static Rect Tweens(Rect rect, DottAnimation[] animations, float timeScale, DottAnimation selected, ref bool isTweenDragging, Action<DottAnimation> tweenSelected)
        {
            rect = rect.ShiftY(HEADER_HEIGHT + TIME_HEIGHT).SetHeight(animations.Length * ROW_HEIGHT);

            DottAnimation startDrag = null;

            for (var i = 0; i < animations.Length; i++)
            {
                var animation = animations[i];
                var rowRect = new Rect(rect.x, rect.y + i * ROW_HEIGHT, rect.width, ROW_HEIGHT);
                var isSelected = selected == animation;
                var tweenRect = Tween(animation, rowRect, isSelected, timeScale);

                ProcessDragEvents(tweenRect, ref isTweenDragging, start: Start, end: null);

                var bottomLine = new Rect(rowRect.x, rowRect.y + rowRect.height, rowRect.width, 1);
                EditorGUI.DrawRect(bottomLine, Color.black);
                continue;

                void Start()
                {
                    startDrag = animation;
                    tweenSelected?.Invoke(animation);
                }
            }

            if (startDrag == null && Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                tweenSelected?.Invoke(null);
            }

            return rect;
        }

        private static Rect Tween(DottAnimation animation, Rect rowRect, bool isSelected, float timeScale)
        {
            var isInfinite = animation.Loops == -1;
            var loops = Mathf.Max(1, animation.Loops);
            var start = rowRect.x + animation.Delay * timeScale * rowRect.width;
            var width = isInfinite
                ? rowRect.width - start + rowRect.x
                : animation.Duration * loops * timeScale * rowRect.width;

            var tweenRect = new Rect(start, rowRect.y, width, rowRect.height).Expand(-1);
            var alphaMultiplier = animation.IsActive ? 1f : 0.4f;

            RoundRect(tweenRect, Color.gray.SetAlpha(0.3f * alphaMultiplier), borderRadius: 4);

            if (isSelected)
            {
                RoundRect(tweenRect, Color.white.SetAlpha(0.9f * alphaMultiplier), borderRadius: 4, borderWidth: 2);
            }
            else
            {
                var mouseHover = tweenRect.Contains(Event.current.mousePosition);
                if (mouseHover)
                {
                    RoundRect(tweenRect, Color.white.SetAlpha(0.9f), borderRadius: 4, borderWidth: 1);
                }
            }

            var colorLine = new Rect(tweenRect.x + 1, tweenRect.y + tweenRect.height - 3, tweenRect.width - 2, 2);
            Random.InitState((int)GlobalObjectId.GetGlobalObjectIdSlow(animation.Component).targetObjectId);
            var color = Colors.GetRandom();
            EditorGUI.DrawRect(colorLine, color.SetAlpha(0.6f * alphaMultiplier));

            var label = animation.Label();
            var style = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold, fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white.SetAlpha(alphaMultiplier) }
            };
            GUI.Label(tweenRect, label, style);

            return tweenRect;
        }

        public static void TimeVerticalLine(Rect rect, float time)
        {
            var verticalLine = new Rect(rect.x + time * rect.width, rect.y, 1, rect.height);
            EditorGUI.DrawRect(verticalLine, Color.white);
        }

        public static void Inspector(UnityEditor.Editor editor)
        {
            EditorGUILayout.Space();
            GUILayout.Label("Inspector", EditorStyles.boldLabel);
            editor.OnInspectorGUI();
        }

        public static bool AddButton(Rect timelineRect)
        {
            var buttonSize = new Vector2(28, 24);
            var position = new Vector2(timelineRect.x + (BOTTOM_HEIGHT - buttonSize.y) / 2, timelineRect.y + timelineRect.height - BOTTOM_HEIGHT + (BOTTOM_HEIGHT - buttonSize.y) / 2);
            var buttonRect = new Rect(position, buttonSize);
            var image = Resources.Load<Texture>("dotween.timeline.add.tween");
            var content = new GUIContent(image) { tooltip = "Add tween" };
            return GUI.Button(buttonRect, content);
        }

        public static bool CallbackButton(Rect timelineRect)
        {
            var buttonSize = new Vector2(22, 24);
            var position = new Vector2(timelineRect.x + (BOTTOM_HEIGHT - buttonSize.y) / 2 + 28 + 2, timelineRect.y + timelineRect.height - BOTTOM_HEIGHT + (BOTTOM_HEIGHT - buttonSize.y) / 2);
            var buttonRect = new Rect(position, buttonSize);
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor.SetAlpha(0.55f);
            var content = EditorGUIUtility.IconContent("d_Animation.AddEvent");
            content.tooltip = "Add callback";
            var result = GUI.Button(buttonRect, content);
            GUI.backgroundColor = backgroundColor;
            return result;
        }

        public static bool RemoveButton(Rect timelineRect)
        {
            var buttonSize = new Vector2(50, 24);
            var position = new Vector2(timelineRect.x + timelineRect.width - buttonSize.x - (BOTTOM_HEIGHT - buttonSize.y) / 2, timelineRect.y + timelineRect.height - BOTTOM_HEIGHT + (BOTTOM_HEIGHT - buttonSize.y) / 2);
            var buttonRect = new Rect(position, buttonSize);

            return GUI.Button(buttonRect, "Delete");
        }

        public static bool DuplicateButton(Rect rect)
        {
            var buttonSize = new Vector2(66, 24);
            var position = new Vector2(rect.x + rect.width - buttonSize.x - (BOTTOM_HEIGHT - buttonSize.y) / 2 - 50 - 2, rect.y + rect.height - BOTTOM_HEIGHT + (BOTTOM_HEIGHT - buttonSize.y) / 2);
            var buttonRect = new Rect(position, buttonSize);

            return GUI.Button(buttonRect, "Duplicate");
        }

        public static bool PlayButton(Rect rect)
        {
            var content = EditorGUIUtility.IconContent("d_PlayButton On");
            var position = rect.position + new Vector2(2, (HEADER_HEIGHT - PlayButtonSize.y) / 2);
            var buttonRect = new Rect(position, PlayButtonSize);
            var contentColor = GUI.contentColor;
            GUI.contentColor = Color.cyan;
            var result = GUI.Button(buttonRect, content);
            GUI.contentColor = contentColor;
            return result;
        }

        public static bool StopButton(Rect rect)
        {
            var position = rect.position + new Vector2(2, (HEADER_HEIGHT - PlayButtonSize.y) / 2);
            var buttonRect = new Rect(position, PlayButtonSize);
            return GUI.Button(buttonRect, "■");
        }

        private static void RoundRect(Rect rect, Color color, float borderRadius, float borderWidth = 0)
        {
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false,
                imageAspect: 0, color, borderWidth, borderRadius);
        }
    }
}