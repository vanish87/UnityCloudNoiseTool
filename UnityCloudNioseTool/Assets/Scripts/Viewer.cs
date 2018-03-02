using UnityEngine;
using NoiseTools;
using System.IO;

[ExecuteInEditMode]
public class Viewer : MonoBehaviour
{
    enum NoiseType { Perlin, Worley, Perlin_Worley }
    
    [SerializeField, Range(2, 3)]
    int _dimensions = 3;

    [SerializeField, Range(1, 6)]
    int _fractal_perlin = 4;

    [SerializeField, Range(1, 6)]
    int _fractal_worley = 2;

    [SerializeField, Range(64, 256)]
    int _resolution = 64;

    [SerializeField, Range(1, 30)]
    int _frequency_perlin = 4;
    [SerializeField, Range(1, 30)]
    int _frequency_worley = 10;

    [SerializeField]
    bool _revert = true;

    Texture2D[] _texture;

    void OnEnable()
    {
        _texture = new Texture2D[3];

        _texture[0] = new Texture2D(_resolution, _resolution);
        _texture[1] = new Texture2D(_resolution, _resolution);
        _texture[2] = new Texture2D(_resolution, _resolution);

        ResetTexture();
        
    }

    void OnDisable()
    {
    }

    void Update()
    {
        ResetTexture();
    }

    void OnGUI()
    {
        var w = Screen.height / 3;
        GUI.DrawTexture(new Rect(0, 0, w, w), _texture[0]);
        GUI.DrawTexture(new Rect(w, 0, w, w), _texture[1]);
        GUI.DrawTexture(new Rect(2*w, 0, w, w), _texture[2]);
    }

    // Utility function that maps a value from one range to another.
    float Remap(float original_value, float original_min, float original_max, float new_min, float new_max)
    {
        return new_min + (((original_value - original_min) / (original_max - original_min)) * (new_max - new_min));
    }

    float GetNoise(NoiseGeneratorBase noise, float frequency, int dimension, int fractal, float x, float y, float z = 0)
    {
        float c = 0;
        if (frequency == 1)
        {
            if (dimension == 2)
            {
                c = noise.GetAt(x, y);
            }
            else
            {
                c = noise.GetAt(x, y, z);
            }
        }
        else
        if (dimension == 2)
        {
            c = noise.GetFractal(x, y, fractal);
        }
        else
        {
            c = noise.GetFractal(x, y, z, fractal);
        }

        return c;
    }
    void FillPerlinWorleyTexture(NoiseGeneratorBase noise_perlin, NoiseGeneratorBase noise_worley, Texture2D texture)
    {
        var scale = 1.0f / _resolution;
        var z = Time.time * 0.1f;

        for (var iy = 0; iy < _resolution; iy++)
        {
            var y = scale * iy;

            for (var ix = 0; ix < _resolution; ix++)
            {
                var x = scale * ix;

                float c_perlin = GetNoise(noise_perlin, _frequency_perlin, _dimensions, _fractal_perlin, x, y, z);
                float c_worley = GetNoise(noise_worley, _frequency_worley, _dimensions, _fractal_worley, x, y, z);

                //c_worley = NewWorleyNoise.WorleyNoiseAtPosition(new Vector3(x, y, z), (int)_frequency_worley);

                if (_revert) c_worley = InvertWorley(c_worley);

                float c = Remap(c_perlin, -c_worley, 1, 0, 1);

                texture.SetPixel(ix, iy, new Color(c, c, c));
            }
        }

        texture.Apply();
    }

    float InvertWorley(float worley)
    {
        //if (worley > 0.5) worley *=2;
        //if (worley < 0.25) worley -= 0.25f;

        //worley = worley * Mathf.Lerp(1,2.0f,worley);
        return 1 - worley;
    }
    void FillTexture(NoiseGeneratorBase noise, Texture2D texture, int fractal, bool revert = false)
    {
        var scale = 1.0f / _resolution;
        var z = Time.time * 0.1f;

        for (var iy = 0; iy < _resolution; iy++)
        {
            var y = scale * iy;

            for (var ix = 0; ix < _resolution; ix++)
            {
                var x = scale * ix;

                float c = GetNoise(noise, noise.Frequency, _dimensions, fractal, x, y, z);

                //c = NewWorleyNoise.WorleyNoiseAtPosition(new Vector3(x, y, z), (int)_frequency_worley);

                if (revert) c = InvertWorley(c);

                texture.SetPixel(ix, iy, new Color(c, c, c));
            }
        }

        texture.Apply();
    }

    void ResetTexture()
    {

        NoiseGeneratorBase noise_perlin;
        NoiseGeneratorBase noise_worley;

        noise_perlin = new PerlinNoise(_frequency_perlin, 1, 0);
        noise_worley = new NewWorleyNoise(_frequency_worley, 1, 0);
        
        this.FillTexture(noise_perlin, _texture[0], _fractal_perlin);
        this.FillTexture(noise_worley, _texture[1], _fractal_worley, _revert);
        this.FillPerlinWorleyTexture(noise_perlin, noise_worley, _texture[2]);

    }
}
