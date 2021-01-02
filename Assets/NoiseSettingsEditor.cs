using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseSettingsEditor : ScriptableObject
{
  public NoiseSettings[] noiseSettings;

  [System.Serializable]
  public class NoiseSettings
  {
    public bool enabled = true;
    [Range(0, 1)]
    public double noiseScale = 0;
    [Range(0, 1)]
    public double offsetZ = 0;
    [Range(0, 10)]
    public double weight = 1;
    public bool useFirstLayerAsMask = false;
    public Noise noise;

    public NoiseSettings()
    {
      noise = new Noise();
    }

    public NoiseSettings(int seed)
    {
      noise = new Noise(seed);
    }

    public double getValue(Vector3 point)
    {
      return (1 +  .5 * noise.Evaluate(point * (float)noiseScale)) * weight;
    }
  }
}
