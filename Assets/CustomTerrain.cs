using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
    public Texture2D heightMapImage;
    public Vector3 heightMapScale = new Vector3(1, 1, 1);
    public Vector2 randomHeightRange = new Vector2(0,0.1f);

    public bool resetTerrain = true;    
    
    //  Single Perlin Noise  --------------------
    public float perlinXScale = 0.01f;
    public float perlinYScale = 0.01f;
    public int perlinXOffset = 0;
    public int perlinYOffset = 0;
    public int perlinOctaves = 3;
    public float perlinPersistance = 8f;
    public float perlinHeightScale = 0.09f;
    
    //  Multiple Perlin Noise  ------------------
    [System.Serializable]
    public class PerlinParameters
    {
        public float mPerlinXScale = 0.01f;
        public float mPerlinYScale = 0.01f;
        public int mPerlinXOffset = 0;
        public int mPerlinYOffset = 0;
        public int mPerlinOctaves = 3;
        public float mPerlinPersistance = 8f;
        public float mPerlinHeightScale = 0.09f;
        public bool remove = false;
    }

    public List<PerlinParameters> perlinParameters = new List<PerlinParameters>()
    {
        new PerlinParameters()  //  Initialize Index 0  --  Table cannot be Empty
    };

    public Terrain terrain;
    public TerrainData terrainData;
    public int heightMapResolution;

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
        this.heightMapResolution = this.terrainData.heightmapResolution;
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

    float[,] GetHeightMap()
    {
        if (this.resetTerrain)
        {
            return new float[this.heightMapResolution, this.heightMapResolution];
        }
        else
            return this.terrainData.GetHeights(0, 0, this.heightMapResolution, this.heightMapResolution);
    }
    
    public void RandomTerrain()
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < this.heightMapResolution; x++)
        {
            for (int y = 0; y < this.heightMapResolution; y++)
            {
                heightMap[ x, y ] += UnityEngine.Random.Range(randomHeightRange.x, randomHeightRange.y);
            }
        }
        this.terrainData.SetHeights(0, 0, heightMap);
    }

    public void LoadTexture()
    {
        float[,] heightMap = GetHeightMap();

        for (int x = 0; x < this.heightMapResolution; x++)
        {
            for (int y = 0; y < this.heightMapResolution; y++)
            {
                heightMap[ x, y ] += heightMapImage.GetPixel((int)(x * this.heightMapScale.x),
                                                             (int)(y * this.heightMapScale.z)).grayscale
                                                                     * this.heightMapScale.y;  
            }
        }
        this.terrainData.SetHeights(0, 0, heightMap);
    }

    public void Perlin()
    {
        float[,] heightMap = GetHeightMap();
    
        for (int y = 0; y < this.heightMapResolution; y++)
        {
            for (int x = 0; x < this.heightMapResolution; x++)
            {
                heightMap[ x, y ] += Utils.fBM((x + this.perlinXOffset) * this.perlinXScale,
                                               (y + this.perlinYOffset) * this.perlinYScale,
                                                    this.perlinOctaves,
                                                    this.perlinPersistance) * this.perlinHeightScale;
            }
        }
        this.terrainData.SetHeights(0, 0, heightMap);
    }

    public void MultiplePerlinTerrain()
    {
        float[,] heightMap = GetHeightMap();
    
        for (int y = 0; y < this.heightMapResolution; y++)
        {
            for (int x = 0; x < this.heightMapResolution; x++)
            {
                foreach (PerlinParameters p in this.perlinParameters)
                {
                    heightMap[ x, y ] += Utils.fBM((x + p.mPerlinXOffset) * p.mPerlinXScale,
                                                   (y + p.mPerlinYOffset) * p.mPerlinYScale,
                                                        p.mPerlinOctaves,
                                                        p.mPerlinPersistance) * p.mPerlinHeightScale;
                }
            }
        }
        this.terrainData.SetHeights(0, 0, heightMap);
    }

    public void AddNewPerlin()
    {
        this.perlinParameters.Add(new PerlinParameters());
    }

    public void RemovePerlin()
    {
        List<PerlinParameters> keptPerlinParameters = new List<PerlinParameters>();
        for (int i = 0; i < this.perlinParameters.Count; i++)
        {
            if (!this.perlinParameters[i].remove)
            {
                keptPerlinParameters.Add(this.perlinParameters[i]);
            }
        }

        if (keptPerlinParameters.Count == 0)  //  0 Elements retained
        {
            keptPerlinParameters.Add(this.perlinParameters[0]);  //  Always Retain Index 0  --  Table cannot be Empty
        }

        this.perlinParameters = keptPerlinParameters;
    }

    public void Voronoi()
    {
        float[,] heightMap = GetHeightMap();
        float fallOff = 0.5f;

        Vector3 peak = new Vector3(UnityEngine.Random.Range(0, this.heightMapResolution),
                                   UnityEngine.Random.Range(0.0f, 1.0f),
                                   UnityEngine.Random.Range(0, this.heightMapResolution));
        
        heightMap[(int)peak.x, (int)peak.z] = peak.y;

        Vector2 peakLocation = new Vector2(peak.x, peak.z);
        float maxDistance = Vector2.Distance(new Vector2(0,0), new Vector2(this.heightMapResolution, this.heightMapResolution));

        for (int y = 0; y < this.heightMapResolution; y++)
        {
            for (int x = 0; x < this.heightMapResolution; x++)
            {
                if (!(x == peak.x && y == peak.z))
                {
                    float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x,y)) * fallOff;
                    heightMap[ x, y ] = peak.y - (distanceToPeak / maxDistance);
                }
            }
        }
        this.terrainData.SetHeights(0, 0, heightMap);
    }

    public void ResetTerrain()
    {
        float[,] heightMap = new float[this.heightMapResolution, this.heightMapResolution];

        for (int x = 0; x < this.heightMapResolution; x++)
        {
            for (int y = 0; y < this.heightMapResolution; y++)
            {
                heightMap[ x, y ] = 0;
            }
        }
        this.terrainData.SetHeights(0, 0, heightMap);
    }
}
