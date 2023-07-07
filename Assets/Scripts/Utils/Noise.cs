using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Noise {

    public int octaves = 1;
    public float scale = 1f;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    public Noise (float scale, int octaves, float persistence, float lacunarity) {
        this.scale = scale; this.octaves = octaves; this.persistence = persistence; this.lacunarity = lacunarity;
    }

    public float Perlin3D (float x, float y, float z) {
        float total = 0f;
        float maxvalue = 0f;
        float _scale = scale;
        float _height = 1f;

        for (int i = 0; i < octaves; i++) {
            total += (Perlin.Noise (x / _scale, y / _scale, z / _scale) / 2.0f + 0.5f) * _height;
            maxvalue += _height;
            _scale /= lacunarity;
            _height *= persistence;
        }

        return total / maxvalue;
    }
}
