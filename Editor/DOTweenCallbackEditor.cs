using UnityEditor;
using UnityEngine;

namespace Dott.Editor
{
    [CustomEditor(typeof(DOTweenCallback))]
    public class DOTweenCallbackEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var callback = (DOTweenCallback)target;

            serializedObject.Update();

            Undo.RecordObject(callback, "DOTweenCallback");

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DOTweenCallback.id)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DOTweenCallback.delay)));

            EditorGUILayout.BeginHorizontal();

            callback.autoGenerate = GUILayout.Toggle(callback.autoGenerate, "Auto Generate", GUI.skin.button);
            if (callback.autoGenerate)
            {
                callback.autoPlay = GUILayout.Toggle(callback.autoPlay, "Auto Play", GUI.skin.button);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DOTweenCallback.onCallback)));

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}