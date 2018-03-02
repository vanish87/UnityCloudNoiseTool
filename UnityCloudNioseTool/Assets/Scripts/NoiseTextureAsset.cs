using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseTextureAsset : ScriptableObject
{
    [SerializeField, Range(2, 3)]
    private int _dimensions = 2;
    
    [SerializeField, Range(1, 6)]
    private int _fractal = 4;

    [SerializeField, Range(16, 256)]
    private int _resolution = 64;
    
    [SerializeField, Range(1, 30)]
    private int _frequency = 4;

    [SerializeField]
    private bool _revert = false;

    [SerializeField]
    private Texture _texture = null;

    public Texture Texture
    {
        get { return this._texture; }
    }

    [SerializeField]
    private NoiseTexture.NoiseType _type = NoiseTexture.NoiseType.Perlin;

    public void UpdateField(int d, int f, int r, int freq, bool revert, Texture texture, NoiseTexture.NoiseType t)
    {
        this._dimensions = d;
        this._fractal = f;
        this._resolution = r;
        this._frequency = freq;
        this._revert = revert;
        this._texture = texture;
        this._type = t;
    }

    public void ReadField(ref int d, ref int f, ref int r, ref int freq, ref bool revert, ref NoiseTexture.NoiseType t)
    {
        d = this._dimensions;
        f = this._fractal;
        r = this._resolution;
        freq = this._frequency;
        revert = this._revert;
        t = this._type;
    }
}
