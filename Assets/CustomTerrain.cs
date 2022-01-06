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

    //  Voronoi Tessellation  -------------------
    public int voronoiCount = 1;
    public float voronoiFallOff = 0.2f;
    public float voronoiDropOff = 0.6f;
    public float voronoiMinHeight = 0.1f;
    public float voronoiMaxHeight = 1.0f;
    
    public enum VoronoiType { TestCase = 0, Linear = 1, Power = 2, Combined = 3, SinPow = 4 }
    public VoronoiType voronoiType = VoronoiType.Linear;

    //  Midpoint Displacement  ------------------
    public float mpdHeightMin = -2.0f;
    public float mpdHeightMax = 2.0f;
    public float mpdDampener = 2.0f;
    public float mpdRoughness = 2.0f;

   //  Splat Maps  ------------------------------
    [System.Serializable]
    public class SplatHeights
    {
        public Texture2D texture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 1f;
        public float minSlope = 0f;
        public float maxSlope = 90f;
        public Vector2 tileOffset = new Vector2(0, 0);
        public Vector2 tileSize = new Vector2(50, 50);
        public float splatOffset = 0.1f;
        public float splatNoiseXScale = 0.01f;
        public float splatNoiseYScale = 0.01f;
        public float splatNoiseScaler = 0.1f;
        public bool remove = false;
    }

    public List<SplatHeights> splatHeights = new List<SplatHeights>()
    {
        new SplatHeights()  //  Initialize Index 0  --  Table cannot be Empty
    };

    //  Vegetation  -----------------------------
    public int maxVegetation = 5000;
    public int vegetationSpacing = 5;

    [System.Serializable]
    public class VegetationMaps
    {
        public GameObject mesh;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
        public float minSlope = 0;
        public float maxSlope = 90;
        public float minWidthScale = 0.5f;
        public float maxWidthScale = 1.0f;
        public float minHeightScale = 0.5f;
        public float maxHeightScale = 1.0f;
        public float density = 0.5f;
        public Gradient colour = new Gradient();
        public Color lightColour = Color.white;
        public bool remove = false;
    }

    public List<VegetationMaps> vegetationMaps = new List<VegetationMaps>()
    {
        new VegetationMaps()  //  Initialize Index 0  --  Table cannot be Empty
    };

    //  Smoothing  ------------------------------
    public int smoothReps = 1;

    public Terrain terrain;
    public TerrainData terrainData;
    public int heightMapResolution;

    public enum TagType { Tag = 0, Layer = 1 }
    [SerializeField] private int terrainLayer = -1;

    private void Awake() 
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        AddTag(tagsProp, "Terrain", TagType.Tag);
        AddTag(tagsProp, "Cloud", TagType.Tag);
        AddTag(tagsProp, "Shore", TagType.Tag);
        tagManager.ApplyModifiedProperties();

        SerializedProperty layerProp = tagManager.FindProperty("layers");
        this.terrainLayer = AddTag(layerProp, "Terrain", TagType.Layer);
        tagManager.ApplyModifiedProperties();

        this.gameObject.tag = "Terrain";
        this.gameObject.layer = this.terrainLayer;
    }

    private void OnEnable() 
    {
        Debug.Log("Terrain Data Init");
        this.terrain = this.GetComponent<Terrain>();
        this.terrainData = Terrain.activeTerrain.terrainData;
        this.heightMapResolution = this.terrainData.heightmapResolution;
    }

    private int AddTag(SerializedProperty tagsProp, string newTag, TagType tType)
    {
        bool found = false;

        for (int i = 0; i< tagsProp.arraySize; i++)
        {
            SerializedProperty t = tagsProp.GetArrayElementAtIndex(i);
            if (t.stringValue.Equals(newTag)) { found = true; return i; }
        }

        if (!found && tType == TagType.Tag)
        {
            tagsProp.InsertArrayElementAtIndex(0);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(0);
            newTagProp.stringValue = newTag;
        }
        else if (!found && tType == TagType.Layer)
        {
            for (int j = 8; j < tagsProp.arraySize; j++)
            {
                SerializedProperty newLayer = tagsProp.GetArrayElementAtIndex(j);
                if (newLayer.stringValue == "")
                {
                    newLayer.stringValue = newTag;
                    return j;
                }
            }
        }

        return -1;
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
    
    //  Random  ---------------------------------
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

    //  Load  -----------------------------------
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

    //  Single Perlin Noise  --------------------
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

    //  Multiple Perlin Noise  ------------------
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

    //  Voronoi Tesselation  --------------------
    public void Voronoi()
    {
        float[,] heightMap = GetHeightMap();

        for (int i = 0; i < this.voronoiCount; i++)
        {
            Vector3 peak = new Vector3(UnityEngine.Random.Range(0, this.heightMapResolution),
                                       UnityEngine.Random.Range(this.voronoiMinHeight, this.voronoiMaxHeight),
                                       UnityEngine.Random.Range(0, this.heightMapResolution));
            
            if (heightMap[(int)peak.x, (int)peak.z] < peak.y)
                heightMap[(int)peak.x, (int)peak.z] = peak.y;
            else
                continue;

            Vector2 peakLocation = new Vector2(peak.x, peak.z);
            float maxDistance = Vector2.Distance(new Vector2(0,0), new Vector2(this.heightMapResolution, this.heightMapResolution));

            for (int y = 0; y < this.heightMapResolution; y++)
            {
                for (int x = 0; x < this.heightMapResolution; x++)
                {
                    if (!(x == peak.x && y == peak.z))
                    {
                        float distanceToPeak = Vector2.Distance(peakLocation, new Vector2(x,y)) / maxDistance;
                        float h = DefineVoronoiType(peak, distanceToPeak);

                        if (heightMap[ x, y ] < h)
                        {
                            heightMap[ x, y ] = h;
                        }
                    }
                }
            }
        }
        this.terrainData.SetHeights(0, 0, heightMap);
    }

    private float DefineVoronoiType(Vector3 peak, float distanceToPeak)
    {
        switch (this.voronoiType)
        {
            case VoronoiType.SinPow:
                return peak.y - Mathf.Pow(distanceToPeak * 3, this.voronoiFallOff) - Mathf.Sin(distanceToPeak * 2 * Mathf.PI) / this.voronoiDropOff;

            case VoronoiType.Combined:
                return peak.y - distanceToPeak * this.voronoiFallOff - Mathf.Pow(distanceToPeak, this.voronoiDropOff);

            case VoronoiType.Power:
                return peak.y - Mathf.Pow(distanceToPeak, this.voronoiDropOff) * this.voronoiFallOff;

            case VoronoiType.Linear:
                return peak.y - distanceToPeak * this.voronoiFallOff;

            default:
                return peak.y - distanceToPeak;
        }
    }

    //  Midpoint Displacement  ------------------
    public void MidPointDisplacement()
    {
        float[,] heightMap = GetHeightMap();

        int width = this.heightMapResolution - 1;
        int squareSize = width;

        float heightMin = this.mpdHeightMin;
        float heightMax = this.mpdHeightMax;
        float heightDampen = (float) Mathf.Pow(this.mpdDampener, -1 * this.mpdRoughness);

        int cornerX, cornerY;
        int midX, midY;
        int pmidXL, pmidXR, pmidYU, pmidYD;

        while (squareSize > 0)
        {
            //  Diamond Step  -------------------
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int) (x + squareSize / 2.0f);
                    midY = (int) (y + squareSize / 2.0f);

                    heightMap[ midX, midY ] = (float) ((heightMap[ x, y ] + 
                                                        heightMap[ cornerX, y ] +
                                                        heightMap[ x, cornerY ] +
                                                        heightMap[ cornerX, cornerY ]) / 
                                                        4.0f + UnityEngine.Random.Range(heightMin, heightMax));
                }
            }

            //  Square Step  --------------------
            for (int x = 0; x < width; x += squareSize)
            {
                for (int y = 0; y < width; y += squareSize)
                {
                    cornerX = (x + squareSize);
                    cornerY = (y + squareSize);

                    midX = (int) (x + squareSize / 2.0f);
                    midY = (int) (y + squareSize / 2.0f);

                    pmidXR = (int) (midX + squareSize);
                    pmidYU = (int) (midY + squareSize);
                    pmidXL = (int) (midX - squareSize);
                    pmidYD = (int) (midY - squareSize);

                    if (pmidXL <= 0 || pmidYD <= 0 || pmidXR >= width -1 || pmidYU >= width -1) 
                        continue;

                    //  Bottom Side  ------------
                    heightMap[ midX, y ] = (float) ((heightMap[ midX, midY ] +
                                                     heightMap[ x, y ] +
                                                     heightMap[ midX, pmidYD ] + 
                                                     heightMap[ cornerX, y ]) / 
                                                     4.0f + UnityEngine.Random.Range(heightMin, heightMax));

                    //  Left Side  --------------
                    heightMap[ x, midY ] = (float) ((heightMap[ midX, midY ] +
                                                     heightMap[ x, y ] +
                                                     heightMap[ pmidXL, midY ] + 
                                                     heightMap[ x, cornerY ]) / 
                                                     4.0f + UnityEngine.Random.Range(heightMin, heightMax));

                    //  Top Side  ---------------
                    heightMap[ midX, cornerY ] = (float) ((heightMap[ midX, midY ] +
                                                           heightMap[ x, cornerY ] +
                                                           heightMap[ midX, pmidYU ] + 
                                                           heightMap[ cornerX, cornerY ]) / 
                                                           4.0f + UnityEngine.Random.Range(heightMin, heightMax));

                    //  Right Side  -------------
                    heightMap[ cornerX, midY ] = (float) ((heightMap[ midX, midY ] +
                                                           heightMap[ cornerX, cornerY ] +
                                                           heightMap[ pmidXR, midY ] + 
                                                           heightMap[ cornerX, y ]) / 
                                                           4.0f + UnityEngine.Random.Range(heightMin, heightMax));
                }
            }
            squareSize = (int) (squareSize / 2.0f);
            heightMin *= heightDampen;
            heightMax *= heightDampen;
        }
        this.terrainData.SetHeights(0, 0, heightMap);
    }

    //  Splat Maps  -----------------------------
    public void SplatMaps()
    {
        TerrainLayer[] newSplatPrototypes;
        newSplatPrototypes = new TerrainLayer[this.splatHeights.Count];
        int spindex = 0;

        foreach (SplatHeights sh in this.splatHeights)
        {
            newSplatPrototypes[spindex] = new TerrainLayer();
            newSplatPrototypes[spindex].diffuseTexture = sh.texture;
            newSplatPrototypes[spindex].tileOffset = sh.tileOffset;
            newSplatPrototypes[spindex].tileSize = sh.tileSize;
            newSplatPrototypes[spindex].diffuseTexture.Apply(true);

            // string path = "Assets.New Terrain Layer " + spindex + ".terrainlayer";
            // AssetDatabase.CreateAsset(newSplatPrototypes[spindex], path);
            spindex++;
            // Selection.activeObject = this.gameObject;
        }

        this.terrainData.terrainLayers = newSplatPrototypes;

        float[,] heightMap = this.terrainData.GetHeights(0, 0, this.heightMapResolution, this.heightMapResolution);
        float[,,] splatmapData = new float[this.terrainData.alphamapWidth, this.terrainData.alphamapHeight, this.terrainData.alphamapLayers]; 

        for (int y = 0; y < this.terrainData.alphamapHeight; y++)
        {
            for (int x = 0; x < this.terrainData.alphamapWidth; x++)
            {
                float[] splat = new float[this.terrainData.alphamapLayers];

                for (int i = 0; i < this.splatHeights.Count; i++)
                {
                    float noise = Mathf.PerlinNoise(x * this.splatHeights[i].splatNoiseXScale, 
                                                    y * this.splatHeights[i].splatNoiseYScale) 
                                                      * this.splatHeights[i].splatNoiseScaler;
                    float offset = this.splatHeights[i].splatOffset + noise;
                    float thisHeightStart = this.splatHeights[i].minHeight - offset;
                    float thisHeightStop = this.splatHeights[i].maxHeight + offset;
                    //  Flip (X,Y) for Steepness Method to prevent Steep Slope placement bug
                    float steepness = this.terrainData.GetSteepness(y / (float) this.terrainData.alphamapHeight,
                                                                    x / (float) this.terrainData.alphamapWidth);
                    
                    if ((heightMap[ x, y ] >= thisHeightStart && heightMap[ x, y ] <= thisHeightStop) &&
                        (steepness >= this.splatHeights[i].minSlope && steepness <= this.splatHeights[i].maxSlope))
                    {
                        splat[i] = 1;
                    }
                }

                NormalizeVector(splat);

                for (int j = 0; j < this.splatHeights.Count; j++)
                {
                    splatmapData[ x, y, j ] = splat[j];
                }
            }
        }

        this.terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    public void AddSplatHeight()
    {
        this.splatHeights.Add(new SplatHeights());
    }

    public void RemoveSplatHeight()
    {
        List<SplatHeights> keptSplatHeights = new List<SplatHeights>();

        for (int i = 0; i < this.splatHeights.Count; i++)
        {
            if(!this.splatHeights[i].remove)
            {
                keptSplatHeights.Add(this.splatHeights[i]);
            }
        }

        if (keptSplatHeights.Count == 0)  //  0 Elements retained
        {
            keptSplatHeights.Add(this.splatHeights[0]);  //  Always Retain Index 0  --  Table cannot be Empty
        }

        this.splatHeights = keptSplatHeights;
    }

    private void NormalizeVector(float[] v) 
    {
        float total = 0f;

        for (int i = 0; i < v.Length; i++)
        {
            total += v[i];
        }

        for (int j = 0; j < v.Length; j++)
        {
            v[j] /= total;
        }
    }

    //  Vegetation ------------------------------
    public void Vegetation()
    {
        TreePrototype[] newTreePrototypes;
        newTreePrototypes = new TreePrototype[this.vegetationMaps.Count];
        int vindex = 0;

        foreach (VegetationMaps v in this.vegetationMaps)
        {
            newTreePrototypes[vindex] = new TreePrototype();
            newTreePrototypes[vindex].prefab = v.mesh;
            vindex++;
        }

        this.terrainData.treePrototypes = newTreePrototypes;

        List<TreeInstance> allVegetation = new List<TreeInstance>();

        for (int z = 0; z < this.terrainData.size.z; z += this.vegetationSpacing)
        {
            for (int x = 0; x < this.terrainData.size.x; x += this.vegetationSpacing)
            {
                for (int tp = 0; tp < this.terrainData.treePrototypes.Length; tp++)
                {
                    if (UnityEngine.Random.Range(0.0f, 1.0f) > this.vegetationMaps[tp].density) continue;

                    float thisHeight = this.terrainData.GetHeight((int) (x * terrainData.alphamapWidth / terrainData.size.x),
                                                                  (int) (z * terrainData.alphamapHeight / terrainData.size.z)) 
                                                                       / this.terrainData.size.y;
                    float thisHeightStart = this.vegetationMaps[tp].minHeight;
                    float thisHeightStop = this.vegetationMaps[tp].maxHeight;

                    float steepness = this.terrainData.GetSteepness(x / (float) this.terrainData.size.x,
                                                                    z / (float) this.terrainData.size.z);

                    if ((thisHeight >= thisHeightStart && thisHeight <= thisHeightStop) &&
                         (steepness >= this.vegetationMaps[tp].minSlope && steepness <= this.vegetationMaps[tp].maxSlope))
                    {
                        TreeInstance treeinstance = new TreeInstance();

                        treeinstance.position = new Vector3((x + UnityEngine.Random.Range(-5.0f, 5.0f)) / this.terrainData.size.x, 
                                                            thisHeight, 
                                                            (z + UnityEngine.Random.Range(-5.0f, 5.0f)) / this.terrainData.size.z);

                        Vector3 treeWorldPos = new Vector3(treeinstance.position.x * this.terrainData.size.x,
                                                           treeinstance.position.y * this.terrainData.size.y,
                                                           treeinstance.position.z * this.terrainData.size.z)
                                                         + this.transform.position;

                        RaycastHit hit;
                        int layerMask = 1 << terrainLayer;

                        //  Raycast to Correct Spawn Height
                        if (Physics.Raycast(treeWorldPos + new Vector3(0, 10, 0), -Vector3.up, out hit, 100, layerMask) ||
                            Physics.Raycast(treeWorldPos - new Vector3(0, 10, 0), Vector3.up, out hit, 100, layerMask))
                        {
                            float treeHeight = (hit.point.y - this.transform.position.y) / this.terrainData.size.y;
                            treeinstance.position = new Vector3(treeinstance.position.x,
                                                                treeHeight,
                                                                treeinstance.position.z);

                            treeinstance.prototypeIndex = tp;
                            treeinstance.color = vegetationMaps[tp].colour.Evaluate(UnityEngine.Random.Range(0f, 1f));
                            treeinstance.lightmapColor = vegetationMaps[tp].lightColour;
                            treeinstance.heightScale = UnityEngine.Random.Range(vegetationMaps[tp].minHeightScale, vegetationMaps[tp].maxHeightScale);
                            treeinstance.widthScale = UnityEngine.Random.Range(vegetationMaps[tp].minWidthScale, vegetationMaps[tp].maxWidthScale);                            

                            allVegetation.Add(treeinstance);
                            if (allVegetation.Count >= maxVegetation) goto TREESDONE;
                        }    
                    }  
                }
            }
        }

        TREESDONE:
                this.terrainData.treeInstances = allVegetation.ToArray();
    }

    public void AddVegetation()
    {
        this.vegetationMaps.Add(new VegetationMaps());
    }

    public void RemoveVegetation()
    {
        List<VegetationMaps> keptVegetationMaps = new List<VegetationMaps>();

        for (int i = 0; i < this.vegetationMaps.Count; i++)
        {
            if(!this.vegetationMaps[i].remove)
            {
                keptVegetationMaps.Add(this.vegetationMaps[i]);
            }
        }

        if (keptVegetationMaps.Count == 0)  //  0 Elements retained
        {
            keptVegetationMaps.Add(this.vegetationMaps[0]);  //  Always Retain Index 0  --  Table cannot be Empty
        }

        this.vegetationMaps = keptVegetationMaps;
    }

    //  Smoothing  ------------------------------
    public void Smooth()
    {
        float[,] originalHeightMap = this.terrainData.GetHeights(0, 0, this.heightMapResolution, this.heightMapResolution);
        float[,] smoothHeightMap = originalHeightMap;
        float smoothProgress = 0;

        EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress);

        for (int i = 0; i < this.smoothReps; i++)
        {
            for (int y = 0; y < this.heightMapResolution; y++)
            {
                for (int x = 0; x < this.heightMapResolution; x++)
                {
                    float avgHeight = originalHeightMap[ x, y ];
                    
                    List<Vector2> neighbours = GenerateNeighbours(new Vector2(x, y), this.heightMapResolution, this.heightMapResolution);

                    foreach (Vector2 n in neighbours)
                    {
                        avgHeight += originalHeightMap[(int) n.x, (int) n.y ];
                    }
                    smoothHeightMap[ x, y ] = avgHeight / ((float) neighbours.Count + 1);
                }
            }
            originalHeightMap = smoothHeightMap;
            smoothProgress++;

            EditorUtility.DisplayProgressBar("Smoothing Terrain", "Progress", smoothProgress / this.smoothReps);
        }
        this.terrainData.SetHeights(0, 0, smoothHeightMap);
        EditorUtility.ClearProgressBar();
    }

    private List<Vector2> GenerateNeighbours(Vector2 pos, int width, int height)
    {
        List<Vector2> neighbours = new List<Vector2>();

        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if(!(x == 0 && y == 0))
                {
                    Vector2 nPos = new Vector2(Mathf.Clamp(pos.x + x, 0, width - 1), Mathf.Clamp(pos.y + y, 0, height - 1));

                    if (!neighbours.Contains(nPos))
                         neighbours.Add(nPos);
                }
            }
        }

        return neighbours;
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
