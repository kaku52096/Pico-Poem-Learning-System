using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(RoundedImage), true)]
[CanEditMultipleObjects]
public class RoundedImageEditor : UnityEditor.UI.ImageEditor
{
    SerializedProperty _radius;

    protected override void OnEnable()
    {
        base.OnEnable();

        // Setup the SerializedProperties.
        _radius = serializedObject.FindProperty("radius");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
        serializedObject.Update();
        // Show the custom GUI controls
        EditorGUILayout.Slider(_radius, 0, 1, new GUIContent("Radius"));

        // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
