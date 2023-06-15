using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RawRotation))]
public class RawRotationEditorGUI : Editor
{
    private bool showInfoSection = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Cast the target to your component type
        RawRotation yourComponent = (RawRotation)target;

        // Info Section
        showInfoSection = EditorGUILayout.BeginFoldoutHeaderGroup(showInfoSection, "Info");
        if (showInfoSection)
        {
            EditorGUI.indentLevel++;

            // Display custom information in the inspector
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.Vector3Field("Raw Rotation", yourComponent.GetRawRotation());
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.Vector3Field("Delta Euler", yourComponent.GetDeltaEuler());
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.Vector3Field("Turns", yourComponent.GetTurns());
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        serializedObject.ApplyModifiedProperties();


    }
}
