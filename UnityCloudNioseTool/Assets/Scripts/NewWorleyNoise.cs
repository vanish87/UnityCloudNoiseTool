/*!
 * File: WorleyNoise.cs
 * Date: 2018/02/28 19:51
 *
 * Author: Yuan Li
 * Contact: vanish8.7@gmail.com
 *
 * Description: To Generate Worley Noise, 
 * this noise is slightly different than original noise in class WorleyNoise
 * this function will generate darker and lighter area, compared with WorleyNoise function
 * 
 * Noise function from https://github.com/sebh/TileableVolumeNoise
 *
 * 
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoiseTools
{
    public class NewWorleyNoise : NoiseGeneratorBase
    {
        #region Constructor

        public NewWorleyNoise(int frequency, int repeat, int seed = 0)
        : base(frequency, repeat, seed)
        {
        }
        #endregion
        
        float Frac(float x)
        {
            int floor = Mathf.FloorToInt(x);
            return x - floor;
        }

        //this is different from XXHash
        float Hash(float n)
        {
            float t = Mathf.Sin(n + 1.951f) * 43758.5453f;
            return Frac(t);
        }
        Vector3 VectorFrac(Vector3 x)
        {
            return new Vector3(Frac(x.x), Frac(x.y), Frac(x.z));
        }
        Vector3 VectorFloor(Vector3 x)
        {
            return new Vector3(Mathf.Floor(x.x), Mathf.Floor(x.y), Mathf.Floor(x.z));
        }

        Vector3 VectorMod(Vector3 x, float y)
        {
            return new Vector3(x.x - y * Mathf.Floor(x.x / y), x.y - y * Mathf.Floor(x.y / y), x.z - y * Mathf.Floor(x.z / y));
        }
        Vector3 VectorMul(Vector3 x, Vector3 y)
        {
            return new Vector3(x.x * y.x, x.y * y.y, x.z * y.z);
        }

        // Hash based 3d value noise
        float Noise(Vector3 x)
        {
            Vector3 p = VectorFloor(x);
            Vector3 f = VectorFrac(x);

            f = VectorMul(VectorMul(f, f), (new Vector3(3.0f, 3.0f, 3.0f) - VectorMul(new Vector3(2.0f, 2.0f, 2.0f), f)));
            float n = p.x + p.y * 57.0f + 113.0f * p.z;
            return Mathf.Lerp(
                Mathf.Lerp(
                    Mathf.Lerp(Hash(n + 0.0f), Hash(n + 1.0f), f.x),
                    Mathf.Lerp(Hash(n + 57.0f), Hash(n + 58.0f), f.x),
                    f.y),
                Mathf.Lerp(
                    Mathf.Lerp(Hash(n + 113.0f), Hash(n + 114.0f), f.x),
                    Mathf.Lerp(Hash(n + 170.0f), Hash(n + 171.0f), f.x),
                    f.y),
                f.z);
        }

        float Cells(Vector3 p, float cellCount)
        {
            Vector3 pCell = p * cellCount;
            float d = 1.0e10f;
            for (int xo = -1; xo <= 1; xo++)
            {
                for (int yo = -1; yo <= 1; yo++)
                {
                    for (int zo = -1; zo <= 1; zo++)
                    {
                        Vector3 tp = VectorFloor(pCell) + new Vector3(xo, yo, zo);

                        float n = Noise(VectorMod(tp, cellCount / 1.0f));
                        tp = pCell - tp - new Vector3(n, n, n);

                        d = Mathf.Min(d, Vector3.Dot(tp, tp));
                    }
                }
            }
            d = Mathf.Min(d, 1.0f);
            d = Mathf.Max(d, 0.0f);
            return d;
        }

        protected override float Calculate2D(Vector2 point)
        {
            return Cells(new Vector3(point.x, point.y, 0), this.Frequency);
        }

        protected override float Calculate3D(Vector3 point)
        {
            return Cells(point, this.Frequency);
        }
    }
}