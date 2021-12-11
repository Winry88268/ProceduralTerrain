using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
    public Texture2D heightMapImage;
    public Vector2 randomHeightRange = new Vector2(0,0.1f);
    public Vector3 heightMapScale = new Vector3(1, 1, 1);
    public Terrain terrain;
    public TerrainData terrainData;
 
    private void Awake() 
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        AddTag(tagsProp, "Terrain");
        AddTag(tagsProp, "Cloud");
        AddTag(tagsProp, "Shore");

        tagManager.ApplyModifiedProperties();

        this.gameObject.tag = "Terrain";
    }

    private void OnEnable() 
    {
        Debug.Log("Terrain Data Init");
        this.terrain = this.GetComponent<Terrain>();
        this.terrainData = Terrain.activeTerrain.terrainData;
    }

    void AddTag(SerializedProperty tagsProp, string newTag)
    {
        bool found = false;

        for (int i = 0; i< tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag)) { found = true; break; }
        }

        if (!found)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
    }
    
    public void RandomTerrain()
    {
        int heightMapResolution = this.terrainData.heightmapResolution;

        float[,] heightMap = this.terrainData.GetHeights(0, 0, heightMapResolution, heightMapResolution);

        for (int x = 0; x < heightMapResolution; x++)
        {
            for (int y = 0; y < heightMapResolution; y++)
            {
                heightMap[x, y] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        this.terrainData.SetHeights(0, 0, heightMap);
    }

    public void LoadTexture()
    {
        int heightMapResolution = this.terrainData.heightmapResolution;

        float[,] heightMap = new float[heightMapResolution, heightMapResolution];

        for (int x = 0; x < heightMapResolution; x++)
        {
            for (int y = 0; y < heightMapResolution; y++)
            {
                heightMap[x, y] = heightMapImage.GetPixel((int)(x * this.heightMapScale.x),
                                                          (int)(y * this.heightMapScale.z)).grayscale
                                                                  * this.heightMapScale.y;  
            }
        }
        this.terrainData.SetHeights(0, 0, heightMap);
    }

    public void ResetTerrain()
    {
        int heightMapResolution = this.terrainData.heightmapResolution;

        float[,] heightMap = new float[heightMapResolution, heightMapResolution];

        for (int x = 0; x < heightMapResolution; x++)
        {
            for (int y = 0; y < heightMapResolution; y++)
            {
                heightMap[x, y] = 0;
            }
        }
        this.terrainData.SetHeights(0, 0, heightMap);
    }
}
