using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Dott.Editor
{
    [CustomEditor(typeof(DOTweenFrame))]
    public class DOTweenFrameEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var idProperty = serializedObject.FindProperty("id");
            var delayProperty = serializedObject.FindProperty("delay");
            var properties = serializedObject.FindProperty("properties");

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(idProperty, new GUIContent("ID"));
            EditorGUILayout.PropertyField(delayProperty);
            EditorGUILayout.Space();

            for (var i = 0; i < properties.arraySize; i++)
            {
                OnPropertyGUI(properties, i);
            }

            if (GUILayout.Button("Add"))
            {
                properties.arraySize++;

                // Unity duplicates the last element when increasing arraySize, so reset the new one manually
                serializedObject.ApplyModifiedProperties();
                ((DOTweenFrame)target).Properties[properties.arraySize - 1] = new DOTweenFrame.FrameProperty();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void OnPropertyGUI(SerializedProperty properties, int index)
        {
            var boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(4, 0, 6, 6),
                margin = new RectOffset(0, 0, 0, 8)
            };

            var current = new CurrentProperty(properties, index);

            Rect targetRect;
            using (new GUILayout.VerticalScope(boxStyle))
            using (new LabelWidthScope(NARROW_LABEL_WIDTH))
            {
                Component targetComponent;
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(current.TargetGameObjectProp, new GUIContent("Target"));
                    targetRect = GUILayoutUtility.GetLastRect();
                    targetComponent = TargetComponent(current);
                }

                if (current.TargetGameObject != null)
                {
                    EditorGUILayout.PropertyField(current.PropertyTypeProp);
                }

                EndValue(current, targetComponent);
            }

            MenuButton(current, targetRect, boxStyle.padding);
        }

        private static void MenuButton(CurrentProperty current, Rect targetRect, RectOffset boxPadding)
        {
            var icon = EditorGUIUtility.IconContent("_Menu");

            var removeButtonRect = new Rect(targetRect.xMin - boxPadding.left - MENU_BUTTON_SIZE, targetRect.y, MENU_BUTTON_SIZE, MENU_BUTTON_SIZE);
            if (GUI.Button(removeButtonRect, icon, EditorStyles.iconButton))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Remove"), false, () =>
                {
                    current.Properties.DeleteArrayElementAtIndex(current.Index);
                    current.Properties.serializedObject.ApplyModifiedProperties();
                });
                menu.ShowAsContext();
            }
        }

        private static Component TargetComponent(CurrentProperty current)
        {
            var targetProperty = current.TargetProp;
            var targetGameObject = current.TargetGameObject;
            var propertyType = current.PropertyType;

            if (targetGameObject == null || propertyType == DOTweenFrame.FrameProperty.PropertyType.None)
            {
                targetProperty.objectReferenceValue = null;
                return null;
            }

            var components = FindTargetComponents(targetGameObject, propertyType);
            if (components.Length == 0)
            {
                EditorGUILayout.HelpBox("No suitable target component found", MessageType.Error);
                targetProperty.objectReferenceValue = null;
                return null;
            }

            Component component;
            if (components.Length == 1)
            {
                targetProperty.objectReferenceValue = component = components[0];
                return component;
            }

            var componentNames = components.Select(c => c.GetType().Name).ToArray();
            var selectedIndex = Array.IndexOf(components, (Component)targetProperty.objectReferenceValue);
            if (selectedIndex == -1)
            {
                selectedIndex = 0;
            }

            selectedIndex = EditorGUILayout.Popup(selectedIndex, componentNames, GUILayout.MaxWidth(104f));
            targetProperty.objectReferenceValue = component = components[selectedIndex];
            return component;
        }

        private static void EndValue(CurrentProperty current, Component targetComponent)
        {
            if (targetComponent == null)
            {
                return;
            }

            var vector3Property = current.EndValueVector3Prop;
            var floatProperty = current.EndValueFloatProp;
            var colorProperty = current.EndValueColorProp;

            switch (current.PropertyType)
            {
                case DOTweenFrame.FrameProperty.PropertyType.Position:
                case DOTweenFrame.FrameProperty.PropertyType.LocalPosition:
                {
                    EditorGUILayout.PropertyField(vector3Property, new GUIContent("Position"));
                    break;
                }

                case DOTweenFrame.FrameProperty.PropertyType.Scale:
                {
                    ScalePropertyField(current);
                    break;
                }

                case DOTweenFrame.FrameProperty.PropertyType.Fade:
                {
                    EditorGUILayout.PropertyField(floatProperty, new GUIContent("Alpha"));
                    floatProperty.floatValue = Mathf.Clamp(floatProperty.floatValue, 0f, 1f);
                    break;
                }

                case DOTweenFrame.FrameProperty.PropertyType.Color:
                    EditorGUILayout.PropertyField(colorProperty, new GUIContent("Color"));
                    break;

                case DOTweenFrame.FrameProperty.PropertyType.None:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(current.PropertyType), current.PropertyType, null);
            }

            IsRelativeField(current);
        }

        private static void IsRelativeField(CurrentProperty current)
        {
            switch (current.PropertyType)
            {
                case DOTweenFrame.FrameProperty.PropertyType.Position:
                case DOTweenFrame.FrameProperty.PropertyType.LocalPosition:
                case DOTweenFrame.FrameProperty.PropertyType.Scale:
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(current.IsRelativeProp);
                    EditorGUI.indentLevel--;
                    break;

                case DOTweenFrame.FrameProperty.PropertyType.None:
                case DOTweenFrame.FrameProperty.PropertyType.Fade:
                case DOTweenFrame.FrameProperty.PropertyType.Color:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(current.PropertyType), current.PropertyType, null);
            }
        }

        private static void ScalePropertyField(CurrentProperty current)
        {
            var uniformScaleButtonStyle = new GUIStyle("FloatFieldLinkButton");
            var flexibleWidthOption = GUILayout.MinWidth(0f);

            var uniformProperty = current.OptionalBoolProp;
            var vector3Property = current.EndValueVector3Prop;

            var uniformScale = uniformProperty.boolValue;

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label("Scale", GUILayout.MaxWidth(NARROW_LABEL_WIDTH));
            var lastRect = GUILayoutUtility.GetLastRect();
            var toggleRect = new Rect(lastRect.xMax - UNIFORM_SCALE_BUTTON_WIDTH, lastRect.y, UNIFORM_SCALE_BUTTON_WIDTH, lastRect.height);
            uniformScale = EditorGUI.Toggle(toggleRect, uniformScale, uniformScaleButtonStyle);

            uniformProperty.boolValue = uniformScale;
            if (!uniformScale)
            {
                EditorGUILayout.PropertyField(vector3Property, GUIContent.none);
                EditorGUILayout.EndHorizontal();
                return;
            }

            using (new LabelWidthScope(AXIS_LABEL_WIDTH))
            {
                var scale = vector3Property.vector3Value.x;
                scale = EditorGUILayout.FloatField("X", scale, flexibleWidthOption);
                vector3Property.vector3Value = Vector3.one * scale;

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.FloatField("Y", scale, flexibleWidthOption);
                    EditorGUILayout.FloatField("Z", scale, flexibleWidthOption);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private static Component[] FindTargetComponents(GameObject targetGameObject, DOTweenFrame.FrameProperty.PropertyType propertyType)
        {
            switch (propertyType)
            {
                case DOTweenFrame.FrameProperty.PropertyType.Position:
                case DOTweenFrame.FrameProperty.PropertyType.LocalPosition:
                case DOTweenFrame.FrameProperty.PropertyType.Scale:
                    return new Component[] { targetGameObject.GetComponent<Transform>() };

                case DOTweenFrame.FrameProperty.PropertyType.Fade:
                {
                    var graphic = targetGameObject.GetComponent<Graphic>();
                    var canvasGroup = targetGameObject.GetComponent<CanvasGroup>();
                    return new Component[] { graphic, canvasGroup }.Where(c => c != null).ToArray();
                }

                case DOTweenFrame.FrameProperty.PropertyType.Color:
                {
                    var graphic = targetGameObject.GetComponent<Graphic>();
                    var camera = targetGameObject.GetComponent<Camera>();
                    return new Component[] { graphic, camera }.Where(c => c != null).ToArray();
                }

                case DOTweenFrame.FrameProperty.PropertyType.None:
                default:
                    return Array.Empty<Component>();
            }
        }

        private struct CurrentProperty
        {
            public readonly SerializedProperty Properties;
            public readonly int Index;

            public readonly SerializedProperty TargetGameObjectProp;
            public readonly SerializedProperty PropertyTypeProp;
            public readonly SerializedProperty TargetProp;
            public readonly SerializedProperty IsRelativeProp;

            public readonly SerializedProperty EndValueVector3Prop;
            public readonly SerializedProperty EndValueFloatProp;
            public readonly SerializedProperty EndValueColorProp;
            public readonly SerializedProperty OptionalBoolProp;

            public readonly DOTweenFrame.FrameProperty.PropertyType PropertyType => (DOTweenFrame.FrameProperty.PropertyType)PropertyTypeProp.intValue;
            public readonly GameObject TargetGameObject => TargetGameObjectProp.objectReferenceValue as GameObject;

            public CurrentProperty(SerializedProperty properties, int index)
            {
                Properties = properties;
                Index = index;
                var property = properties.GetArrayElementAtIndex(index);

                TargetGameObjectProp = property.FindPropertyRelative("TargetGameObject");
                PropertyTypeProp = property.FindPropertyRelative("Property");
                TargetProp = property.FindPropertyRelative("Target");
                IsRelativeProp = property.FindPropertyRelative("IsRelative");

                EndValueVector3Prop = property.FindPropertyRelative("EndValueVector3");
                EndValueFloatProp = property.FindPropertyRelative("EndValueFloat");
                EndValueColorProp = property.FindPropertyRelative("EndValueColor");
                OptionalBoolProp = property.FindPropertyRelative("OptionalBool");
            }
        }

        #region Styles

        private const float NARROW_LABEL_WIDTH = 90f;
        private const int AXIS_LABEL_WIDTH = 12;
        private const float UNIFORM_SCALE_BUTTON_WIDTH = 18f;
        private const float MENU_BUTTON_SIZE = 16f;

        #endregion

        #region GUI

        private class LabelWidthScope : GUI.Scope
        {
            private readonly float prevLabelWidth;

            public LabelWidthScope(float labelWidth)
            {
                prevLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = labelWidth;
            }

            protected override void CloseScope()
            {
                EditorGUIUtility.labelWidth = prevLabelWidth;
            }
        }

        #endregion
    }
}