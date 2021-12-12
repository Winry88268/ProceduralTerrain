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
    SerializedProperty perlinXScale;
    SerializedProperty perlinYScale;
    SerializedProperty perlinXOffset;
    SerializedProperty perlinYOffset;

    //GUI Fold Outs ------------
    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlin = false;

    private void OnEnable() 
    {
        this.randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        this.heightMapScale = serializedObject.FindProperty("heightMapScale");
        this.heightMapImage = serializedObject.FindProperty("heightMapImage");
        this.perlinXScale = serializedObject.FindProperty("perlinXScale");
        this.perlinYScale = serializedObject.FindProperty("perlinYScale");
        this.perlinXOffset = serializedObject.FindProperty("perlinXOffset");
        this.perlinYOffset = serializedObject.FindProperty("perlinYOffset");
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

        this.showPerlin = EditorGUILayout.Foldout(this.showPerlin, "Single Perlin Noise");
        if (this.showPerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Perlin Noise", EditorStyles.boldLabel);
            EditorGUILayout.Slider(this.perlinXScale, 0, 1, new GUIContent("X Scale"));
            EditorGUILayout.Slider(this.perlinYScale, 0, 1, new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(this.perlinXOffset, 0, 10000, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(this.perlinYOffset, 0, 10000, new GUIContent("Y Offset"));
            
            if (GUILayout.Button("Perlin"))
            {
                terrain.Perlin();
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
