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
        this.alphaToggle = EditorGUILayout.Toggle("Alpha?", this.alphaToggle);
        this.seamlessToggle = EditorGUILayout.Toggle("Seamless?", this.seamlessToggle);
        this.mapToggle = EditorGUILayout.Toggle("Map?", this.mapToggle);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

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

                    float colValue = pValue;
                    pixCol = new Color(colValue, colValue, colValue, alphaToggle ? colValue : 1);
                    this.pTexture.SetPixel(x, y, pixCol);
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

        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
}
