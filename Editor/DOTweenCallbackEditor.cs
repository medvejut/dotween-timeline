using UnityEditor;

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

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DOTweenCallback.onCallback)));

            serializedObject.ApplyModifiedProperties();
        }
    }
}