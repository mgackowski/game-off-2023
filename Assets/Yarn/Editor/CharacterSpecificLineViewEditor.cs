using UnityEditor;
using Yarn.Unity.Editor;

/**
 * Inherits from YarnSpinner's original Line View editor as it looks
 * much neater.
 */
[CustomEditor(typeof(CharacterSpecificLineView))]
public class CharacterSpecificLineViewEditor : LineViewEditor
{
    private SerializedProperty activeForCharactersProperty;
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty(nameof(CharacterSpecificLineView.activeForCharacters)));

        base.OnInspectorGUI();
    }
}