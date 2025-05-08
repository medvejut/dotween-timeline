using UnityEditor;

namespace Dott.Editor
{
    [CustomEditor(typeof(DOTweenLink))]
    public class DOTweenLinkEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var link = (DOTweenLink)target;
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            Undo.RecordObject(link, link.name);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DOTweenLink.id)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DOTweenLink.timeline)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DOTweenLink.delay)));

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}