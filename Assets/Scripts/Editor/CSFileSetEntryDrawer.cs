using CodeInspector;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CSFileSetEntry))]
public class CSFileSetEntryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Get the _file property (the CSFile field) and _solutionFile field
        SerializedProperty fileProperty = property.FindPropertyRelative("_file");
        SerializedProperty solutionFileProperty = property.FindPropertyRelative("_solutionFile");

        // Calculate the width for each field
        float fileWidth = position.width * 0.39f; // Adjust as needed
        float solutionFileWidth = position.width * 0.39f; // Adjust as needed
        float labelWidth = position.width * 0.1f; // Adjust label width as needed

        // Draw the label and the _file field (CSFile) at the top left
        Rect labelRect = new Rect(position.x, position.y, labelWidth, position.height);
        EditorGUI.PrefixLabel(labelRect, label); // This will draw the label with the specified width

        // Draw the _file field to the right of the label
        Rect fileRect = new Rect(position.x + labelWidth, position.y, fileWidth, position.height);
        EditorGUI.PropertyField(fileRect, fileProperty, GUIContent.none); // No label here because the label is already drawn

        // Get the current position and adjust it to draw the next field to the right
        position.x += fileWidth + labelWidth + 4; // Move the position to the right of the label and the _file field

        // If _file is a StaticCSFile, don't draw the _solutionFile field
        if (fileProperty.objectReferenceValue is StaticCSFile)
        {
            solutionFileProperty.objectReferenceValue = null;
        }
        // If _file is an EditableCSFile, draw the _solutionFile field to the right
        else if (fileProperty.objectReferenceValue is EditableCSFile)
        {
            // Calculate the width and position for the solution file field
            Rect solutionLabelRect = new Rect(position.x, position.y, labelWidth, position.height);
            EditorGUI.PrefixLabel(solutionLabelRect, new GUIContent("Solution File")); // This will draw the label with the specified width
            // Draw the Solution File property
            Rect solutionFileRect = new Rect(position.x + labelWidth, position.y, solutionFileWidth, position.height);
            EditorGUI.PropertyField(solutionFileRect, solutionFileProperty, GUIContent.none);
        }
    }


    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty fileProperty = property.FindPropertyRelative("_file");
        SerializedProperty solutionFileProperty = property.FindPropertyRelative("_solutionFile");

        // If the _file is an EditableCSFile, we need extra space to draw the SolutionFile field next to it
        if (fileProperty.objectReferenceValue is EditableCSFile)
        {
            return Mathf.Max(EditorGUI.GetPropertyHeight(fileProperty), EditorGUI.GetPropertyHeight(solutionFileProperty));
        }

        // If it's not EditableCSFile, we don't need extra space
        return EditorGUI.GetPropertyHeight(fileProperty);
    }
}
