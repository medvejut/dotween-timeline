using System.Linq;
using DG.DemiEditor;
using UnityEditor;
using UnityEngine;

namespace Dott.Editor
{
    [CustomPropertyDrawer(typeof(DOTweenCallback))]
    public class DOTweenCallbackPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var selectedCallback = property.objectReferenceValue as DOTweenCallback;
            if (selectedCallback == null)
            {
                DrawDefault(position, property, label);
                return;
            }

            var callbacks = selectedCallback.GetComponents<DOTweenCallback>();
            if (callbacks.Length == 1)
            {
                DrawDefault(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var controlRect = EditorGUI.PrefixLabel(position, label);
            var halfWidth = controlRect.width / 2;

            var popupRect = controlRect.SetWidth(halfWidth);
            var index = DrawCallbackPopup(popupRect, callbacks, selectedCallback);
            if (index >= 0)
            {
                property.objectReferenceValue = callbacks[index];
            }

            var selectedRect = controlRect.ShiftX(halfWidth);
            EditorGUI.PropertyField(selectedRect, property, GUIContent.none);

            EditorGUI.EndProperty();

            if (GUI.changed)
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private static int DrawCallbackPopup(Rect popupRect, DOTweenCallback[] options, DOTweenCallback selected)
        {
            var ids = options.Select((callback, i) => $"{i}: {callback.id}").ToArray();
            var index = options.IndexOf(selected);
            index = EditorGUI.Popup(popupRect, index, ids);
            return index;
        }

        private static void DrawDefault(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndProperty();
        }
    }
}