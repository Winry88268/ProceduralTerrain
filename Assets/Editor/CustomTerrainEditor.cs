using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor
{
    //GUI Properties ----------------------
    SerializedProperty randomHeightRange;
    SerializedProperty heightMapScale;
    SerializedProperty heightMapImage;

    //GUI Fold Outs ------------
    bool showRandom = false;
    bool showLoadHeights = false;

    private void OnEnable() 
    {
        this.randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        this.heightMapScale = serializedObject.FindProperty("heightMapScale");
        this.heightMapImage = serializedObject.FindProperty("heightMapImage");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain) target;
        
        this.showRandom = EditorGUILayout.Foldout(this.showRandom, "Random");
        if (this.showRandom)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set Heights between Values", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(this.randomHeightRange);

            if (GUILayout.Button("Random Heights"))
            {
                terrain.RandomTerrain();
            }
        }

        this.showLoadHeights = EditorGUILayout.Foldout(this.showLoadHeights, "Load");
        if (this.showLoadHeights)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Load Heights from Texture", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(this.heightMapImage);
            EditorGUILayout.PropertyField(this.heightMapScale);
            
            if (GUILayout.Button("Load Texture"))
            {
                terrain.LoadTexture();
            }
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Reset"))
        {
            terrain.ResetTerrain();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
