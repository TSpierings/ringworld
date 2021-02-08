using UnityEngine;
using System;

[ExecuteInEditMode]
public class World : MonoBehaviour
{
  public WorldSettings worldSettings;
  public NoiseSettingsEditor noiseSettingsEditor;
  Block[] blocks;

  [SerializeField, HideInInspector]
  GameObject[] blockObjects;

  public Material groundMaterial;

  [HideInInspector]
  public bool noiseSettingsFoldout;

  private bool validated = false;

  public GameObject player;

  private Vector2 lastPlayerIndex = new Vector2(0, 0);

  private void OnValidate()
  {
    validated = true;
  }

  void Update()
  {
    var playerIndex = GetRingworldCoordinates(player.transform);

    if (validated || playerIndex != lastPlayerIndex)
    {
      validated = false;
      lastPlayerIndex = playerIndex;
      CreateWorld();
    }
  }

  public void CreateWorld()
  {
    CreateBlocks();
    Initialize();
    GenerateMesh();
  }

  private void CreateBlocks()
  {
    if (noiseSettingsEditor == null)
    {
      noiseSettingsEditor = new NoiseSettingsEditor();
    }

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

  private void Initialize()
  {
    if (this.worldSettings.circumferenceInBlocks == 0)
    {
      return;
    }

    blocks = new Block[this.worldSettings.circumferenceInBlocks * this.worldSettings.widthInBlocks];

    double r = (this.worldSettings.circumferenceInBlocks * this.worldSettings.tilesPerBlock) / (Math.PI * 2);

    var playerCoord = GetRingworldCoordinates(player.transform);

    for (int circumferenceIndex = 0; circumferenceIndex < this.worldSettings.circumferenceInBlocks; circumferenceIndex++)
    {
      for (int widthIndex = 0; widthIndex < this.worldSettings.widthInBlocks; widthIndex++)
      {
        int index = (circumferenceIndex * this.worldSettings.widthInBlocks) + widthIndex;

        if (blockObjects[index] == null)
        {
          GameObject meshObj = new GameObject($"mesh{index}");
          blockObjects[index] = meshObj;

          meshObj.transform.SetParent(transform);
          meshObj.AddComponent<MeshRenderer>().sharedMaterial = groundMaterial;
          MeshFilter meshFilter = meshObj.AddComponent<MeshFilter>();
          meshFilter.sharedMesh = new Mesh();

          meshObj.AddComponent<MeshCollider>();
        }

        //Debug.Log($"({playerCoord.x} {playerCoord.y}), ({Math.Min(realCircDistance, altCirclDistance)},{relativeWidthIndex}), {playerOffset}");

        blocks[index] = new Block(
          blockObjects[index].GetComponent<MeshFilter>().sharedMesh,
          circumferenceIndex,
          widthIndex,
          r,
          this.worldSettings,
          this.noiseSettingsEditor,
          playerCoord, new Vector2(circumferenceIndex, widthIndex));
      }
    }
  }

  private Vector2 GetRingworldCoordinates(Transform transform)
  {
    var position = transform.position;
    var radians = Math.Atan2(position.y, position.x) + Math.PI;
    var step = (Math.PI * 2) / this.worldSettings.circumferenceInBlocks;
    var circIndex = Math.Floor(radians / step);

    var widthIndex = Math.Floor(position.z / (this.worldSettings.tilesPerBlock));

    //Debug.Log($"rads: {radians / (2 * Math.PI)} ({circIndex})");

    return new Vector2((float)circIndex, -(float)widthIndex);
  }

  private void GenerateMesh()
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
