using System;
using DG.DemiEditor;
using JetBrains.Annotations;
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
        private static readonly Vector2 LoopToggleSize = new(24, 24);
        private static readonly Color PlayheadColor = new(0.19f, 0.44f, 0.89f);

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

        public static Rect Header(Rect rect)
        {
            rect = rect.SetHeight(HEADER_HEIGHT);

            var style = EditorStyles.boldLabel;
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(rect, "Timeline", style);

            var bottomLine = new Rect(rect.x, rect.y + rect.height, rect.width, 1);
            EditorGUI.DrawRect(bottomLine, Color.black);

            return rect;
        }

        public static bool PreviewEye(Rect headerRect, bool isPlaying, bool isPaused, bool isTimeDragging)
        {
            if (!isPlaying && !isPaused && !isTimeDragging) { return false; }

            var iconSize = Vector2.one * 16f;
            var eyeShift = new Vector2(31f, -0f);
            var iconRect = new Rect(headerRect.x + headerRect.width * 0.5f + eyeShift.x, headerRect.y + (headerRect.height - iconSize.y) / 2 + eyeShift.y, iconSize.x, iconSize.y);

            var clickArea = isPlaying ? iconRect : headerRect.Expand(-48f, 0);
            var hover = !isTimeDragging && clickArea.Contains(Event.current.mousePosition);

            var eyeIcon = EditorGUIUtility.TrIconContent(hover ? "animationvisibilitytoggleoff" : "animationvisibilitytoggleon",
                "Disable scene preview mode.");

            using (new DeGUI.ColorScope(background: Color.white.SetAlpha(0f), main: Color.white.SetAlpha(0.3f)))
            {
                EditorGUIUtility.AddCursorRect(iconRect, MouseCursor.Link);
                if (GUI.Button(iconRect, eyeIcon, EditorStyles.iconButton))
                {
                    return true;
                }
            }

            if (Event.current.type == EventType.MouseDown && clickArea.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                return true;
            }

            return false;
        }

        public static Rect Time(Rect rect, float timeScale, ref bool isDragging, Action start, Action<Event> end)
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
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            ProcessDragEvents(rect, ref isDragging, start, end);

            return rect;
        }

        private static void ProcessDragEvents(Rect rect, ref bool isDragging, Action start, Action<Event> end)
        {
            var current = Event.current;
            switch (current.type)
            {
                case EventType.MouseDown when !isDragging && rect.Contains(current.mousePosition):
                    isDragging = true;
                    start?.Invoke();
                    current.Use();
                    break;

                case EventType.MouseUp when isDragging:
                    isDragging = false;
                    end?.Invoke(current);
                    current.Use();
                    break;
            }
        }

        public static void PlayheadLabel(Rect timeRect, float scaledTime, float rawTime)
        {
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 9,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white },
                hover = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter
            };

            var position = new Vector2(timeRect.x + scaledTime * timeRect.width, timeRect.y);
            var labelContent = new GUIContent(rawTime.ToString("0.00"));

            const int yShift = 1;
            var labelRect = new Rect(position.x, position.y + yShift, 32, timeRect.height - yShift * 2);
            labelRect.x -= labelRect.width * 0.5f;
            const int maxXShift = 4;
            labelRect.x = Mathf.Clamp(labelRect.x, timeRect.x - maxXShift, timeRect.xMax - labelRect.width + maxXShift);

            var labelBackground = new Rect(labelRect.x, labelRect.y, labelRect.width, labelRect.height);
            RoundRect(labelBackground, PlayheadColor, borderRadius: 8);

            GUI.Label(labelRect, labelContent, labelStyle);
        }

        public static float GetScaledTimeUnderMouse(Rect timeRect)
        {
            var time = (Event.current.mousePosition.x - timeRect.x) / timeRect.width;
            time = Mathf.Clamp01(time);
            return time;
        }

        public static Rect Tweens(Rect rect, IDOTweenAnimation[] animations, float timeScale, [CanBeNull] IDOTweenAnimation selected, ref bool isTweenDragging, Action<IDOTweenAnimation> tweenSelected)
        {
            rect = rect.ShiftY(HEADER_HEIGHT + TIME_HEIGHT).SetHeight(animations.Length * ROW_HEIGHT);

            IDOTweenAnimation startDrag = null;

            for (var i = 0; i < animations.Length; i++)
            {
                var animation = animations[i];
                var rowRect = new Rect(rect.x, rect.y + i * ROW_HEIGHT, rect.width, ROW_HEIGHT);
                var isSelected = selected?.Component == animation.Component;
                var tweenRect = Element(animation, rowRect, isSelected, timeScale);

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

            return rect;
        }

        private static Rect Element(IDOTweenAnimation animation, Rect rowRect, bool isSelected, float timeScale)
        {
            if (animation.CallbackView)
            {
                return Callback(animation, rowRect, isSelected, timeScale);
            }

            return Tween(animation, rowRect, isSelected, timeScale);
        }

        private static Rect Callback(IDOTweenAnimation animation, Rect rowRect, bool isSelected, float timeScale)
        {
            void Label(Rect rect, GUIContent content, GUIStyle style)
            {
                GUI.Label(rect, content, style);
            }

            void Icon(bool isHovered, Rect iconRect)
            {
                var iconColor = Color.white.SetAlpha(0.6f);
                if (isSelected)
                {
                    iconColor = new Color(0.2f, 0.6f, 1f);
                }
                else if (isHovered)
                {
                    iconColor = Color.white.SetAlpha(0.5f);
                }

                var icon = animation.CustomIcon ?? IconCallback;
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true, 0, iconColor, 0, 0);
            }

            void Underline(bool isHovered, Rect textRect)
            {
                if (!isSelected && !isHovered) { return; }

                var underlineRect = new Rect(textRect.x, textRect.yMax - 4, textRect.width, 1);
                var color = isHovered ? Color.white.SetAlpha(0.7f) : Color.white;
                EditorGUI.DrawRect(underlineRect, color);
            }

            var textStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold, fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                richText = true
            };

            var iconX = CalculateX(rowRect, animation.Delay, timeScale);
            var iconRect = new Rect(iconX, rowRect.y, width: 10, height: 20);

            var labelContent = new GUIContent(animation.Label);

            textStyle.padding = new RectOffset((int)iconRect.width + 4, 0, 0, 1);
            var textWidth = textStyle.CalcSize(labelContent).x;
            var rect = new Rect(iconRect.x, rowRect.y, textWidth, rowRect.height);

            var onRightSide = rect.x > rowRect.x + rowRect.width * 0.5f;
            var outOfBounds = rect.xMax > rowRect.xMax;
            if (onRightSide && outOfBounds)
            {
                (textStyle.padding.right, textStyle.padding.left) = (textStyle.padding.left, textStyle.padding.right);
                rect.x = iconRect.xMax - textWidth;
            }

            var textOnlyRect = rect.Shift(textStyle.padding.left, 0, -textStyle.padding.horizontal, 0);
            var isHovered = rect.Contains(Event.current.mousePosition);

            Icon(isHovered, iconRect);
            Underline(isHovered, textOnlyRect);
            Label(rect, labelContent, textStyle);

            return rect;
        }

        private static Rect Tween(IDOTweenAnimation animation, Rect rowRect, bool isSelected, float timeScale)
        {
            var isInfinite = animation.Loops == -1;
            var loops = Mathf.Max(1, animation.Loops);
            var start = CalculateX(rowRect, animation.Delay, timeScale);
            var width = isInfinite
                ? rowRect.width - start + rowRect.x
                : animation.Duration * loops * timeScale * rowRect.width;

            var tweenRect = new Rect(start, rowRect.y, width, rowRect.height).Expand(-1);
            var alphaMultiplier = animation.IsActive ? 1f : 0.4f;

            RoundRect(tweenRect, Color.gray.SetAlpha(0.3f * alphaMultiplier), borderRadius: 4);

            var mouseHover = tweenRect.Contains(Event.current.mousePosition);
            if (isSelected)
            {
                RoundRect(tweenRect, Color.white.SetAlpha(0.9f * alphaMultiplier), borderRadius: 4, borderWidth: 2);
            }
            else
            {
                if (mouseHover)
                {
                    RoundRect(tweenRect, Color.white.SetAlpha(0.9f), borderRadius: 4, borderWidth: 1);
                }
            }

            var colorLine = new Rect(tweenRect.x + 1, tweenRect.y + tweenRect.height - 3, tweenRect.width - 2, 2);
            Random.InitState(animation.Component.GetInstanceID());
            var color = Colors.GetRandom();
            EditorGUI.DrawRect(colorLine, color.SetAlpha(0.6f * alphaMultiplier));

            var label = new GUIContent(animation.Label);
            var style = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold, fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white.SetAlpha(alphaMultiplier) }
            };
            var labelWidth = style.CalcSize(label).x;
            var labelRect = tweenRect;
            if (labelWidth > labelRect.width)
            {
                label.tooltip = animation.Label;
                style.alignment = mouseHover ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;

                // just to make it look nice
                labelRect.xMin += 4f;
            }

            GUI.Label(labelRect, label, style);

            return tweenRect;
        }

        private static float CalculateX(Rect rowRect, float time, float timeScale)
        {
            return rowRect.x + time * timeScale * rowRect.width;
        }

        public static void TimeVerticalLine(Rect rect, float scaledTime, bool underLabel)
        {
            // some extra shift to nice look on borders
            var shift = underLabel ? 10 : 1;
            var verticalLine = new Rect(rect.x + scaledTime * rect.width, rect.y + shift, 1, rect.height - shift);
            EditorGUI.DrawRect(verticalLine, PlayheadColor);
        }

        public static void Inspector(UnityEditor.Editor editor)
        {
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft
            };

            EditorGUILayout.Space();

            Splitter(new Color(0.12f, 0.12f, 0.12f, 1.333f));

            var backgroundRect = GUILayoutUtility.GetRect(1f, EditorGUIUtility.singleLineHeight);
            var labelRect = backgroundRect;
            backgroundRect = ToFullWidth(backgroundRect);
            EditorGUI.DrawRect(backgroundRect, new Color(0.1f, 0.1f, 0.1f, 0.2f));
            EditorGUI.LabelField(labelRect, "Inspector", headerStyle);

            Splitter(new Color(0.19f, 0.19f, 0.19f, 1.333f));

            editor.OnInspectorGUI();
        }

        private static void Splitter(Color color)
        {
            var rect = GUILayoutUtility.GetRect(1f, 1f);
            rect = ToFullWidth(rect);
            EditorGUI.DrawRect(rect, color);
        }

        private static Rect ToFullWidth(Rect rect)
        {
            rect.xMin = 0f;
            rect.width += 4f;
            return rect;
        }

        public static bool AddButton(Rect timelineRect)
        {
            var buttonRect = CalculateAddButtonRect(timelineRect);
            var image = Resources.Load<Texture>("dotween.timeline.add.tween");
            var content = new GUIContent(image) { tooltip = "Add tween" };
            var style = new GUIStyle(EditorStyles.miniButtonLeft) { fixedHeight = 0 };
            return GUI.Button(buttonRect, content, style);
        }

        private static Rect CalculateAddButtonRect(Rect timelineRect)
        {
            var buttonSize = new Vector2(32, 24);
            var position = new Vector2(timelineRect.x + (BOTTOM_HEIGHT - buttonSize.y) / 2, timelineRect.y + timelineRect.height - BOTTOM_HEIGHT + (BOTTOM_HEIGHT - buttonSize.y) / 2);
            return new Rect(position, buttonSize);
        }

        public static void AddMoreButton(Rect timelineRect, DottView.AddMoreItem[] items, Action<DottView.AddMoreItem> clicked)
        {
            const float buttonWidth = 20;
            var addButtonRect = CalculateAddButtonRect(timelineRect);
            var buttonRect = addButtonRect.ShiftX(addButtonRect.width).SetWidth(buttonWidth);
            var dropDrownIcon = EditorGUIUtility.IconContent("icon dropdown");

            var style = new GUIStyle(EditorStyles.miniButtonRight) { fixedHeight = 0 };
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor.SetAlpha(0.55f);
            var result = EditorGUI.DropdownButton(buttonRect, dropDrownIcon, FocusType.Passive, style);
            GUI.backgroundColor = backgroundColor;

            if (!result)
            {
                return;
            }

            var menu = new GenericMenu();
            foreach (var item in items)
            {
                menu.AddItem(item.Content, false, userData => clicked?.Invoke((DottView.AddMoreItem)userData), item);
            }

            menu.DropDown(addButtonRect.ShiftX(-4));
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
            var content = EditorGUIUtility.IconContent("d_PlayButton");
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

        public static bool LoopToggle(Rect rect, bool value)
        {
            var position = rect.position + new Vector2(rect.width - LoopToggleSize.x - 2, (HEADER_HEIGHT - LoopToggleSize.y) / 2);
            var toggleRect = new Rect(position, LoopToggleSize);
            var iconContent = EditorGUIUtility.TrIconContent("preAudioLoopOff", "Toggle loop playback");
            var style = new GUIStyle(GUI.skin.button) { padding = new RectOffset(0, 0, 0, 0) };
            return GUI.Toggle(toggleRect, value, iconContent, style);
        }

        private static void RoundRect(Rect rect, Color color, float borderRadius, float borderWidth = 0)
        {
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, alphaBlend: false,
                imageAspect: 0, color, borderWidth, borderRadius);
        }

        #region Icons

        private static Texture2D IconCallback => DottUtils.ImageFromString(ICON_CALLBACK);

        private const string ICON_CALLBACK =
            "iVBORw0KGgoAAAANSUhEUgAAABQAAAAoCAYAAAD+MdrbAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAD4SURBVHgB7ZWxCoJQFIaPkkuOQVtDLQ02tPQGDrn6CvU+tbeH4NTmCzgLRWM0NGhBuCiBBXZO3eISFXppMLgf/ChX7ufPWQ6ApHIo3HsX08ZoUI4zZodZ84c9x3GmWZYleUmSJNnTXVboiUUfckGoCDqGJFKZsKbrehME0TRNBzYqFX6MFEqhFErhnwsvcRxvQZA0TQ/4OPFnHdu2RyJrAIts8O4YXnYK0cKYvu/Pi4hoj3ieN8M7Fty35VvqmD61jaJo+UkWhuGKtRpAwbV7a+u67oQfA9fKxDSgJPRng9oGQbCgsFYGfGmlFBDTGB4zijBHkEgqzhX38zVoGGkfagAAAABJRU5ErkJggg==";

        #endregion
    }
}