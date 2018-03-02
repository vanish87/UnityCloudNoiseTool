using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

[ExecuteInEditMode]
public class NoiseTexture : MonoBehaviour
{
    public enum NoiseType { Perlin, Worley, NewWorley, PerlinWorley }

    [SerializeField]
    bool _reset = false;
    //to track a generation/reset progress
    [SerializeField, Range(0, 1)]
    private float _currentProgress = 0;

    [SerializeField, Range(2, 3)]
    private int _dimensions = 2;

    #region Debug Field
    [SerializeField, Range(0,256-1)]
    private int _debugLayer = 0;
    private int _currentLayer = 0;
    //cpu data updated but texture need to be update
    private bool _pendingReset = false;

    [SerializeField, Range(0, 3)]
    private int _colorLayer = 0;
    private int _currentColorLayer = 0;
    [SerializeField]
    private Texture2D _debugTexture;
    #endregion

    [SerializeField, Range(1, 6)]
    private int _fractal = 4;

    [SerializeField, Range(16, 256)]
    private int _resolution = 64;

    public int Resolution
    {
        get { return this._resolution; }
        set { this._resolution = value; }
    }

    [SerializeField, Range(1, 128)]
    private int _frequency = 4;

    [SerializeField]
    private bool _revert = false;

    [SerializeField]
    private Texture _texture = null;

    [SerializeField]
    private NoiseType _type = NoiseType.Perlin;

    private NoiseTools.NoiseGeneratorBase _noise;

    //a CUP copy of noise for 3D textures
    private Color[] _data;

    [SerializeField]
    private string _fileName = "Noise";
    

    // Use this for initialization
    void Start()
    {
        this.Reset();
//         this.InitTextureData();
//         this.Regenerate();
//         this.FillDebugTexture();
    }

    /// <summary>
    /// Init texture data and keep a cpu color array copy and a debug texture copy
    /// </summary>
    void InitTextureData()
    {
        this._data = new Color[(int)Mathf.Pow(this._resolution, this._dimensions)];

        if (_dimensions == 2)
        {
            this._texture = new Texture2D(this._resolution, this._resolution, TextureFormat.RGBA32, false);
        }
        else
        {
            this._texture = new Texture3D(this._resolution, this._resolution, this._resolution, TextureFormat.RGBA32, false);
        }

        this._debugTexture = new Texture2D(this._resolution, this._resolution, TextureFormat.RGBA32, false);
    }

    /// <summary>
    /// get noise of position (x,y,z) from noise generator 
    /// by frequency, dimension and apply fractal if desired
    /// </summary>
    /// <param name="noise">noise generator</param>
    /// <param name="frequency"></param>
    /// <param name="dimension"></param>
    /// <param name="fractal"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    protected virtual Color GetNoise(NoiseTools.NoiseGeneratorBase noise, float frequency, int dimension, int fractal, int x, int y, int z = 0)
    {
        var scale = 1.0f / this._resolution;
        var sx = x * scale;
        var sy = y * scale;
        var sz = z * scale;
        float c = 0;
        if (frequency == 1)
        {
            if (dimension == 2)
            {
                c = noise.GetAt(sx, sy);
            }
            else
            {
                c = noise.GetAt(sx, sy, sz);
            }
        }
        else
        if (dimension == 2)
        {
            c = noise.GetFractal(sx, sy, fractal);
        }
        else
        {
            c = noise.GetFractal(sx, sy, sz, fractal);
        }

        return new Color(c, c, c, 1);
    }

    public float GetNoiseData(int px, int py, int pz)
    {
        var index = px + py * this._resolution + pz * this._resolution * this._resolution;
        return this._data[index].r;
    }
    Color Invert(Color c)
    {
        return new Color(1 - c.r, 1 - c.g, 1 - c.b, 1);
    }

    void Regenerate()
    {
        this._currentProgress = 0;

        _fileName = this._type.ToString();
        switch (this._type)
        {
            case NoiseType.Perlin:
                _noise = new NoiseTools.PerlinNoise(this._frequency, 1);
                break;
            case NoiseType.Worley:
                _noise = new NoiseTools.WorleyNoise(this._frequency, 1);
                break;
            case NoiseType.NewWorley:
                _noise = new NoiseTools.NewWorleyNoise(this._frequency, 1);
                break;
            default:
                Debug.LogWarning("Can not found noise generator");
                break;
        }

        var scale = 1.0f / _resolution;
        var z_dim = this._dimensions == 2 ? 1 : _resolution;
        var index = 0;
        
        for (var iz = 0; iz < z_dim; iz++)
        {
            for (var iy = 0; iy < _resolution; iy++)
            {

                for (var ix = 0; ix < _resolution; ix++)
                {
                    var c = GetNoise(this._noise, this._frequency, this._dimensions, this._fractal, ix, iy, iz);

                    if (this._revert) c = Invert(c);

                    this._data[index++] = c;
                }
            }
            this._currentProgress = iz * scale;
        }

        this._currentProgress = 1;
        this._pendingReset = true;
    }

    void ReadPixelFromTexture()
    {
        if (this._dimensions == 2)
        {
            var tex = ((Texture2D)this._texture);
            this._data = tex.GetPixels();
        }
        else
        {
            var tex = ((Texture3D)this._texture);
            this._data = tex.GetPixels();
        }
    }

    void FillDebugTexture()
    {
        this._debugLayer = Mathf.Clamp(this._debugLayer, 0, this._resolution - 1);
        if (this._dimensions == 2)
        {
            Graphics.CopyTexture(this._texture, this._debugTexture);
        }
        else
        {
            var index = this._debugLayer * this._resolution * this._resolution;
            for (int j = 0; j < this._resolution; ++j)
            {
                for (int k = 0; k < this._resolution; ++k)
                {
                    var c = this._data[index][_colorLayer];
                    this._debugTexture.SetPixel(j, k, new Color(c, c, c, 1));
                    index++;
                }
            }

            this._debugTexture.Apply();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if reset is checked, then regenerate
        //this flag is used to regenerate noise every frame, which is very slow
        if(this._reset)
        {
            this.Reset();
            this._reset = false;
        }
        //debug layer is used to view 2D slice of 3D texture
        if (this._currentLayer != this._debugLayer || this._currentColorLayer != this._colorLayer)
        {
            if (this._resolution == this._texture.width)
            {
                this.FillDebugTexture();
                this._currentLayer = this._debugLayer;
                this._currentColorLayer = this._colorLayer;
            }
            else
            {
                Debug.LogWarning("Resolution has been change, do Reset first");
            }
        }

    }

    private void OnRenderObject()
    {
        if(this._pendingReset)
        {
            this.FillTexture();
            this.FillDebugTexture();
            this._pendingReset = false;
        }
    }

    private void Reset()
    {
        this.InitTextureData();
        this.RegenerateNoise();
    }

    private void FillTexture()
    {
        if (this._dimensions == 2)
        {
            var tex = ((Texture2D)this._texture);
            tex.SetPixels(this._data);
            tex.Apply();
        }
        else
        {
            var tex = ((Texture3D)this._texture);
            tex.SetPixels(this._data);
            tex.Apply();
        }
    }

    async void RegenerateNoise()
    {
        await Task.Run(() =>
        {
            this.Regenerate();
        });
        this.FillTexture();
        this.FillDebugTexture();
    }

    public void Draw(Vector2 pos)
    {
        if (this.gameObject.activeSelf == false) return;
        var w = this._resolution;
        GUI.DrawTexture(new Rect(pos.x,pos.y, w, w), this._debugTexture);
    }

    public void SaveToAsset()
    {
        // Make a proper path from the current selection.
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (string.IsNullOrEmpty(path))
            path = "Assets";
        else if (Path.GetExtension(path) != "")
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        var assetPathName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + this._fileName + ".asset");

        // Create an asset.
        var asset = ScriptableObject.CreateInstance<NoiseTextureAsset>();
        AssetDatabase.CreateAsset(asset, assetPathName);
        AssetDatabase.AddObjectToAsset(this._texture, asset);

        asset.UpdateField(this._dimensions, this._fractal, this._resolution, this._frequency, this._revert, this._texture, this._type);

        // Save the generated mesh asset.
        AssetDatabase.SaveAssets();

        // Tweak the selection.
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
       

    public void LoadAsset()
    {
        // Make a proper path from the current selection.
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (string.IsNullOrEmpty(path))
            path = "Assets";
        else if (Path.GetExtension(path) != "")
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        var assetPathName = (path + "/" + this._fileName + ".asset");

        Debug.LogFormat("Loading from {0}", assetPathName);

        var asset = AssetDatabase.LoadAssetAtPath<NoiseTextureAsset>(assetPathName);
        Debug.Log(asset);

        asset.ReadField(ref this._dimensions, ref this._fractal, ref this._resolution, ref this._frequency, ref this._revert, ref this._type);

        //resize texture
        this.InitTextureData();
        this._texture = asset.Texture;

        //get data from loaded texture
        this.ReadPixelFromTexture();
        this.FillDebugTexture();
    }
}
