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

    //  Perlin Properties  ----------------------
    SerializedProperty perlinXScale;
    SerializedProperty perlinYScale;
    SerializedProperty perlinXOffset;
    SerializedProperty perlinYOffset;
    SerializedProperty perlinOctaves;
    SerializedProperty perlinPersistance;
    SerializedProperty perlinHeightScale;

    GUITableState perlinParameterTable;
    SerializedProperty perlinParameters;

    //  Voronoi Properties  ---------------------
    SerializedProperty voronoiCount;
    SerializedProperty voronoiFallOff;
    SerializedProperty voronoiDropOff;
    SerializedProperty voronoiMinHeight;
    SerializedProperty voronoiMaxHeight;
    SerializedProperty voronoiType;

    //  Mid-Point Displacement Properties  ------
    SerializedProperty mpdHeightMin;
    SerializedProperty mpdHeightMax;
    SerializedProperty mpdDampener;
    SerializedProperty mpdRoughness;

    //  Smoothing  ------------------------------
    SerializedProperty smoothReps;

    //  GUI Fold Outs  --------------------------
    bool showRandom = false;
    bool showLoadHeights = false;
    bool showPerlin = false;
    bool showMultiplePerlin = false;
    bool showVoronoi = false;
    bool showMidPointDisplacement = false;
    bool showSmooth = false;

    private void OnEnable() 
    {
        //  GUI Properties  ---------------------
        this.resetTerrain = serializedObject.FindProperty("resetTerrain");
        this.randomHeightRange = serializedObject.FindProperty("randomHeightRange");
        this.heightMapScale = serializedObject.FindProperty("heightMapScale");
        this.heightMapImage = serializedObject.FindProperty("heightMapImage");

        //  Perlin Properties  ------------------
        this.perlinXScale = serializedObject.FindProperty("perlinXScale");
        this.perlinYScale = serializedObject.FindProperty("perlinYScale");
        this.perlinXOffset = serializedObject.FindProperty("perlinXOffset");
        this.perlinYOffset = serializedObject.FindProperty("perlinYOffset");
        this.perlinOctaves = serializedObject.FindProperty("perlinOctaves");
        this.perlinPersistance = serializedObject.FindProperty("perlinPersistance");
        this.perlinHeightScale = serializedObject.FindProperty("perlinHeightScale");

        this.perlinParameterTable = new GUITableState("perlinParameterTable");
        this.perlinParameters = serializedObject.FindProperty("perlinParameters");

        //  Voronoi Properties  -----------------
        this.voronoiCount = serializedObject.FindProperty("voronoiCount");
        this.voronoiFallOff = serializedObject.FindProperty("voronoiFallOff");
        this.voronoiDropOff = serializedObject.FindProperty("voronoiDropOff");
        this.voronoiMinHeight = serializedObject.FindProperty("voronoiMinHeight");
        this.voronoiMaxHeight = serializedObject.FindProperty("voronoiMaxHeight");
        this.voronoiType = serializedObject.FindProperty("voronoiType");

        //  Mid Point Displacement Properties  --
        this.mpdHeightMin = serializedObject.FindProperty("mpdHeightMin");
        this.mpdHeightMax = serializedObject.FindProperty("mpdHeightMax");
        this.mpdDampener = serializedObject.FindProperty("mpdDampener");
        this.mpdRoughness = serializedObject.FindProperty("mpdRoughness");

        //  Smoothing  --------------------------
        this.smoothReps = serializedObject.FindProperty("smoothReps");
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
            EditorGUILayout.PropertyField(this.randomHeightRange);

            if (GUILayout.Button("Random Heights"))
            {
                terrain.RandomTerrain();
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        this.showLoadHeights = EditorGUILayout.Foldout(this.showLoadHeights, "Load");
        if (this.showLoadHeights)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.PropertyField(this.heightMapImage);
            EditorGUILayout.PropertyField(this.heightMapScale);
            
            if (GUILayout.Button("Load Texture"))
            {
                terrain.LoadTexture();
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        this.showPerlin = EditorGUILayout.Foldout(this.showPerlin, "Single Perlin Noise");
        if (this.showPerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
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

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        this.showMultiplePerlin = EditorGUILayout.Foldout(this.showMultiplePerlin, "Multiple Perlin Noise");
        if (this.showMultiplePerlin)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
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

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        this.showVoronoi = EditorGUILayout.Foldout(this.showVoronoi, "Voronoi");
        if (this.showVoronoi)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.PropertyField(this.voronoiType);
            EditorGUILayout.IntSlider(this.voronoiCount, 1, 10, new GUIContent("Peak Count"));
            EditorGUILayout.Slider(this.voronoiFallOff, 0, 10, new GUIContent("Falloff"));
            EditorGUILayout.Slider(this.voronoiDropOff, 0, 10, new GUIContent("Dropoff"));
            EditorGUILayout.Slider(this.voronoiMinHeight, 0, 1, new GUIContent("Min Height"));
            EditorGUILayout.Slider(this.voronoiMaxHeight, 0, 1, new GUIContent("Max Height"));

            if (GUILayout.Button("Voronoi"))
            {
                terrain.Voronoi();
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        this.showMidPointDisplacement = EditorGUILayout.Foldout(this.showMidPointDisplacement, "Midpoint Displacement");
        if (this.showMidPointDisplacement)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Slider(this.mpdHeightMin, -10f, -0.1f, new GUIContent("MPD Min Height"));
            EditorGUILayout.Slider(this.mpdHeightMax, 0.1f, 10f, new GUIContent("MPD Max Height"));
            EditorGUILayout.Slider(this.mpdDampener, 2f, 10f, new GUIContent("MPD Height Dampening"));
            EditorGUILayout.Slider(this.mpdRoughness, 1f, 10f, new GUIContent("MPD Roughness"));

            if (GUILayout.Button("MPD"))
            {
                terrain.MidPointDisplacement();
            }

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        this.showSmooth = EditorGUILayout.Foldout(this.showSmooth, "Smoothing");
        if (this.showSmooth)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.IntSlider(this.smoothReps, 1, 10, new GUIContent("Smoothing Iterations"));

            if (GUILayout.Button("Smooth"))
            {
                terrain.Smooth();
            }
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if (GUILayout.Button("Full Terrain Reset"))
        {
            terrain.ResetTerrain();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
