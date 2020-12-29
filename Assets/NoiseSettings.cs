using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    [Range(0,10)]
    public double noiseScale = 0;
    [Range(0,1)]
    public double offsetZ = 0;
    [Range(0, 100)]
    public double weight = 1;
}
