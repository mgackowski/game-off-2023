using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(WireframeRenderer))]
[CanEditMultipleObjects]
public class WireframeRendererEditor : Editor
{
    VisualTreeAsset uxml;
    WireframeRenderer script;

    void OnEnable()
    {
        script = (WireframeRenderer)target;
        uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            AssetDatabase.GUIDToAssetPath("03d316cf852f48f4f9bd773ad9eb7b06"));
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement inspector = new VisualElement();
        uxml.CloneTree(inspector);

        /* Add behaviour to buttons */
        inspector.Query<Button>("prepareButton").First().clicked += script.PrepareLineSegments;
        inspector.Query<Button>("prepareAllButton").First().clicked += script.PrepareAllInScene;

        return inspector;
    }
}