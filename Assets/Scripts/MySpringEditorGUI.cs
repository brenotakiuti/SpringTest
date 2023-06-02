using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Spring3D))]
public class MySpringEditorGUI : Editor
{
    private bool showInfoSection = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Cast the target to your component type
        Spring3D yourComponent = (Spring3D)target;

        // Info Section
        showInfoSection = EditorGUILayout.BeginFoldoutHeaderGroup(showInfoSection, "Info");
        if (showInfoSection)
        {
            EditorGUI.indentLevel++;

            // Display custom information in the inspector
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.LabelField("Mass: ");
            EditorGUILayout.LabelField(yourComponent.GetMass().ToString());
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.LabelField("Simulated Damping: ");
            EditorGUILayout.LabelField(yourComponent.GetSimulatedDamping().ToString());
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.Vector3Field("Anchor Position", yourComponent.GetAnchorPosition());
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.Vector3Field("Initial Position: ", yourComponent.GetInitialPosition());
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.Vector3Field("Initial Joint Length: ", yourComponent.GetInitialLength());
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.Vector3Field("Distance Vector", yourComponent.GetDistanceVector());
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.Vector3Field("New Joint Length: ", yourComponent.GetNewLength());
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUI.skin.box);
            EditorGUILayout.LabelField("Time: ");
            EditorGUILayout.LabelField(yourComponent.GetTime().ToString());
            EditorGUILayout.EndHorizontal();
            

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        serializedObject.ApplyModifiedProperties();


    }
}
