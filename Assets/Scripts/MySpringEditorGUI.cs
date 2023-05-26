using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Spring1D))]
public class MySpringEditorGUI : Editor
{
    private bool showInfoSection = true;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Cast the target to your component type
        Spring1D yourComponent = (Spring1D)target;

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
            EditorGUILayout.LabelField("Time: ");
            EditorGUILayout.LabelField(yourComponent.GetTime().ToString());
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Initial State");
            // Iterate through the array elements
            if (yourComponent.GetInitialState() != null)
            {
                for (int i = 0; i < yourComponent.GetInitialState().Length; i++)
                {
                    EditorGUILayout.LabelField("[" + i.ToString() + "]", yourComponent.GetInitialState()[i].ToString());
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Final State");
            // Iterate through the array elements
            if (yourComponent.GetInitialState() != null)
            {
                for (int i = 0; i < yourComponent.GetFinalState().Length; i++)
                {
                    EditorGUILayout.LabelField("[" + i.ToString() + "]", yourComponent.GetFinalState()[i].ToString());
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        serializedObject.ApplyModifiedProperties();


    }
}
