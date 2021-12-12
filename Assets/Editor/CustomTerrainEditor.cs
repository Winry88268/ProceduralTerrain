using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(CustomTerrain))]
[CanEditMultipleObjects]
public class CustomTerrainEditor : Editor
{
    //  GUI Properties  -------------------------
    SerializedProperty resetTerrain;
    SerializedProperty randomHeightRange;
    SerializedProperty heightMapScale;
    SerializedProperty heightMapImage;
    SerializedProperty perlinXScale;
    SerializedProperty perlinYScale;
    SerializedProperty perlinXOffset;
    SerializedProperty perlinYOffset;
    SerializedProperty perlinOctaves;
    SerializedProperty perlinPersistance;
    SerializedProperty perlinHeightScale;

    GUITableState perlinParameterTable;
    SerializedProperty perlinParameters;

    //  GUI Fold Outs  --------------------------
    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlin = false;
    bool showMultiplePerlin = false;

    private void OnEnable() 
    {
        this.resetTerrain = serializedObject.FindProperty("resetTerrain");
        this.randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        this.heightMapScale = serializedObject.FindProperty("heightMapScale");
        this.heightMapImage = serializedObject.FindProperty("heightMapImage");
        this.perlinXScale = serializedObject.FindProperty("perlinXScale");
        this.perlinYScale = serializedObject.FindProperty("perlinYScale");
        this.perlinXOffset = serializedObject.FindProperty("perlinXOffset");
        this.perlinYOffset = serializedObject.FindProperty("perlinYOffset");
        this.perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        this.perlinPersistance = serializedObject.FindProperty("perlinPersistance");
        this.perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");

        this.perlinParameterTable = new GUITableState("perlinParameterTable");
        this.perlinParameters = serializedObject.FindProperty("perlinParameters");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CustomTerrain terrain = (CustomTerrain) target;

        EditorGUILayout.PropertyField(this.resetTerrain);
        
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
            GUILayout.Label("Single Perlin Noise", EditorStyles.boldLabel);
            EditorGUILayout.Slider(this.perlinXScale, 0, 1, new GUIContent("X Scale"));
            EditorGUILayout.Slider(this.perlinYScale, 0, 1, new GUIContent("Y Scale"));
            EditorGUILayout.IntSlider(this.perlinXOffset, 0, 10000, new GUIContent("X Offset"));
            EditorGUILayout.IntSlider(this.perlinYOffset, 0, 10000, new GUIContent("Y Offset"));
            EditorGUILayout.IntSlider(this.perlinOctaves, 1, 10, new GUIContent("Octaves"));
            EditorGUILayout.Slider(this.perlinPersistance, 0.1f, 10, new GUIContent("Persistance"));
            EditorGUILayout.Slider(this.perlinHeightScale, 0, 1, new GUIContent("Height Scale"));
            
            if (GUILayout.Button("Perlin"))
            {
                terrain.Perlin();
            }
        }

        this.showMultiplePerlin = EditorGUILayout.Foldout(this.showMultiplePerlin, "Multiple Perlin Noise");
        if (this.showMultiplePerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Multiple Perlin Noise", EditorStyles.boldLabel);
            this.perlinParameterTable = GUITableLayout.DrawTable(this.perlinParameterTable, this.perlinParameters);
            
            GUILayout.Space(20);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                terrain.AddNewPerlin();
            }

            if (GUILayout.Button("-"))
            {
                terrain.RemovePerlin();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Apply Multiple Perlin"))
            {
                terrain.MultiplePerlinTerrain();
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
