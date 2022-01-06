using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureCreatorWindow : EditorWindow
{
    string filename = "myProceduralTexture";

    Texture2D pTexture;

    float perlinXScale;
    float perlinYScale;
    float perlinPersistance;
    float perlinHeightScale;

    float brightness = 0.5f;
    float contrast = 0.5f;

    int perlinOctaves;
    int perlinOffsetX;
    int perlinOffsetY;

    bool alphaToggle = false;
    bool seamlessToggle = false;
    bool mapToggle = false;

    [MenuItem("Window/TextureCreatorWindow")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TextureCreatorWindow));
    }

    private void OnEnable() 
    {
        this.pTexture = new Texture2D(513, 513, TextureFormat.ARGB32, false);
    }

    private void OnGUI() 
    {
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        this.filename = EditorGUILayout.TextField("Texture Name", this.filename);

        int wSize = (int) (EditorGUIUtility.currentViewWidth - 100);

        this.perlinXScale = EditorGUILayout.Slider("X Scale", this.perlinXScale, 0f, 0.1f);
        this.perlinYScale = EditorGUILayout.Slider("Y Scale", this.perlinYScale, 0f, 0.1f);
        this.perlinOctaves = EditorGUILayout.IntSlider("Octaves", this.perlinOctaves, 1, 10);
        this.perlinPersistance = EditorGUILayout.Slider("Persistance", this.perlinPersistance, 1f, 10f);
        this.perlinHeightScale = EditorGUILayout.Slider("Height Scale", this.perlinHeightScale, 0f, 1f);
        this.perlinOffsetX = EditorGUILayout.IntSlider("X Offset", this.perlinOffsetX, 0, 10000);
        this.perlinOffsetY = EditorGUILayout.IntSlider("Y Offset", this.perlinOffsetY, 0, 10000);

        this.brightness = EditorGUILayout.Slider("Brightness", this.brightness, 0f, 2f);
        this.contrast = EditorGUILayout.Slider("Contrast", this.contrast, 0f, 2f);

        this.alphaToggle = EditorGUILayout.Toggle("Alpha?", this.alphaToggle);
        this.seamlessToggle = EditorGUILayout.Toggle("Seamless?", this.seamlessToggle);
        this.mapToggle = EditorGUILayout.Toggle("Map?", this.mapToggle);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        float minColour = 1;
        float maxColour = 0;

        if (GUILayout.Button("Generate", GUILayout.Width(wSize)))
        {
            int w = 513;
            int h = 513;
            float pValue;
            Color pixCol = Color.white;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (this.seamlessToggle)
                    {
                        //  Current pixel percentage L>R & D>U
                        float u = (float) x / (float) w;
                        float v = (float) y / (float) h;

                        float noise00 = Utils.fBM((x + this.perlinOffsetX) * this.perlinXScale,
                                                  (y + this.perlinOffsetY) * this.perlinYScale,
                                                       this.perlinOctaves,
                                                       this.perlinPersistance) 
                                                     * this.perlinHeightScale;
                        
                        float noise01 = Utils.fBM((x + this.perlinOffsetX) * this.perlinXScale,
                                                  (y + this.perlinOffsetY + h) * this.perlinYScale,
                                                       this.perlinOctaves,
                                                       this.perlinPersistance) 
                                                     * this.perlinHeightScale;

                        float noise10 = Utils.fBM((x + this.perlinOffsetX + w) * this.perlinXScale,
                                                  (y + this.perlinOffsetY) * this.perlinYScale,
                                                       this.perlinOctaves,
                                                       this.perlinPersistance) 
                                                     * this.perlinHeightScale;

                        float noise11 = Utils.fBM((x + this.perlinOffsetX + w) * this.perlinXScale,
                                                  (y + this.perlinOffsetY + h) * this.perlinYScale,
                                                       this.perlinOctaves,
                                                       this.perlinPersistance) 
                                                     * this.perlinHeightScale;

                        float noiseTotal =      u  *      v  * noise00 +
                                                u  * (1 - v) * noise01 +
                                           (1 - u) *      v  * noise10 +
                                           (1 - u) * (1 - v) * noise11;

                        float value = (int) (256 * noiseTotal) + 50;
                        float r = Mathf.Clamp((int) noise00, 0, 255);
                        float g = Mathf.Clamp(value, 0, 255);
                        float b = Mathf.Clamp(value + 50, 0, 255);
                        float a = Mathf.Clamp(value + 100, 0, 255);

                        pValue = (r + g + b) / (3 * 255.0f);
                    }
                    else
                    {
                        pValue = Utils.fBM((x + this.perlinOffsetX) * this.perlinXScale,
                                           (y + this.perlinOffsetY) * this.perlinYScale,
                                                this.perlinOctaves,
                                                this.perlinPersistance) 
                                              * this.perlinHeightScale;
                    }                    

                    float colValue = this.contrast * (pValue - 0.5f) + 0.5f * this.brightness;
                    if (minColour > colValue) minColour = colValue;
                    if (maxColour < colValue) maxColour = colValue;
                    pixCol = new Color(colValue, colValue, colValue, alphaToggle ? colValue : 1);
                    this.pTexture.SetPixel(x, y, pixCol);
                }
            }

            if (this.mapToggle)
            {
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        pixCol = this.pTexture.GetPixel(x, y);
                        float colValue = pixCol.r;
                        colValue = Utils.Map(colValue, minColour, maxColour, 0, 1);
                        
                        pixCol.r = colValue;
                        pixCol.g = colValue;
                        pixCol.b = colValue;
                        this.pTexture.SetPixel(x, y, pixCol);
                    }
                }
            }

            this.pTexture.Apply(false, false);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(this.pTexture, GUILayout.Width(wSize), GUILayout.Height(wSize));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Save", GUILayout.Width(wSize)))
        {
            byte[] bytes = this.pTexture.EncodeToPNG();
            // Make sure that the destination folder exists
            string parentFolder = "Assets";
            string childFolder = "SavedTextures";
            string folderPath = $"{parentFolder}/{childFolder}";
            if(!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder(parentFolder, childFolder);
            // Define the Texture's path in the AAssetDatabase
            string filePath = $"{folderPath}/{this.filename}.png";
            File.WriteAllBytes(filePath, bytes);
            AssetDatabase.Refresh();
            // Get a reference to the TextureImporter associated to the png that was just saved
            TextureImporter textureAsset = (TextureImporter)AssetImporter.GetAtPath(filePath);
            textureAsset.isReadable = true;
            AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
}
