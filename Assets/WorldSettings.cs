using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldSettings
{
  public int circumferenceInBlocks = 10;
  public int widthInBlocks = 10;
  public int tilesPerBlock = 10;
  [Range(1, 10)]
  public int resolution = 1;
  public bool drawNormals = false;
  public bool fixMeshEdgeNormals = true;
  public bool smoothShading = true;
  public bool fixMeshEdgeVertices = true;
}
