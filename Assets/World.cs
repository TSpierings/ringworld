using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

[ExecuteInEditMode]
public class World : MonoBehaviour
{
  public WorldSettings worldSettings;
  public NoiseSettings noiseSettings;
  Block[] blocks;

  [SerializeField, HideInInspector]
  GameObject[] blockObjects;

  private bool validated = false;

  private void OnValidate()
  {
    validated = true;
  }

  void Update()
  {
    if (validated)
    {
      validated = false;

      foo();
      Initialize();
      GenerateMesh();
    }
  }

  void foo()
  {
    if (blockObjects == null && this.worldSettings.circumferenceInBlocks > 0)
    {
      blockObjects = new GameObject[this.worldSettings.circumferenceInBlocks * this.worldSettings.widthInBlocks];
    }

    if (blockObjects.Length != worldSettings.circumferenceInBlocks * this.worldSettings.widthInBlocks)
    {
      foreach (var obj in blockObjects)
      {
        if (obj == null)
        {
          continue;
        }

        DestroyImmediate(obj);
      }

      if (this.worldSettings.circumferenceInBlocks > 0)
      {
        blockObjects = new GameObject[this.worldSettings.circumferenceInBlocks * this.worldSettings.widthInBlocks];
      }
    }
  }

  void Initialize()
  {
    if (this.worldSettings.circumferenceInBlocks == 0)
    {
      return;
    }

    blocks = new Block[this.worldSettings.circumferenceInBlocks * this.worldSettings.widthInBlocks];

    double r = (this.worldSettings.circumferenceInBlocks * this.worldSettings.tilesPerBlock) / (Math.PI * 2);

    for (int circumferenceIndex = 0; circumferenceIndex < this.worldSettings.circumferenceInBlocks; circumferenceIndex++)
    {
      for (int widthIndex = 0; widthIndex < this.worldSettings.widthInBlocks; widthIndex++)
      {
        int index = (circumferenceIndex * this.worldSettings.widthInBlocks) + widthIndex;

        if (blockObjects[index] == null)
        {
          GameObject meshObj = new GameObject("mesh");
          blockObjects[index] = meshObj;

          meshObj.transform.SetParent(transform);
          meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
          MeshFilter meshFilter = meshObj.AddComponent<MeshFilter>();
          meshFilter.sharedMesh = new Mesh();
        }
        
        blocks[index] = new Block(blockObjects[index].GetComponent<MeshFilter>().sharedMesh, circumferenceIndex, widthIndex, r, this.worldSettings, this.noiseSettings);
      }
    }
  }

  void GenerateMesh()
  {
    if (this.worldSettings.circumferenceInBlocks == 0)
    {
      return;
    }

    foreach (Block block in blocks)
    {
      if (block == null)
      {
        continue;
      }

      block.ConstructMesh();
    }
  }
}
