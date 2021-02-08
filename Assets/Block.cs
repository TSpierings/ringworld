using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

public class Block
{
  Mesh mesh;
  Vector3 localUp = Vector3.up;
  Vector3 axisA;
  Vector3 axisB;
  int blockCircumferenceIndex;
  int blockWidthIndex;
  double r;
  NoiseSettingsEditor noiseSettingsEditor;
  WorldSettings worldSettings;
  int resolution;
  Vector2 playerIndex;
  Vector2 blockIndex;

  public Block(Mesh mesh, int blockCircumferenceIndex, int blockWidthIndex, double r, WorldSettings worldSettings, NoiseSettingsEditor noiseSettingsEditor, Vector2 playerIndex, Vector2 blockIndex)
  {
    this.mesh = mesh;
    this.blockCircumferenceIndex = blockCircumferenceIndex;
    this.blockWidthIndex = blockWidthIndex;
    this.r = r;
    this.worldSettings = worldSettings;
    this.noiseSettingsEditor = noiseSettingsEditor;
    this.playerIndex = playerIndex;
    this.blockIndex = blockIndex;
    this.resolution = VectorDistance(playerIndex, blockIndex);

    axisA = new Vector3(localUp.y, localUp.z, localUp.x);
    axisB = Vector3.Cross(localUp, axisA);
  }

  private int VectorDistance(Vector2 player, Vector2 block)
  {
    var realCircDistance = Math.Abs(block.x - player.x);
    var altCirclDistance = Math.Abs(block.x - (player.x + this.worldSettings.circumferenceInBlocks));
    var altaltCirclDistance = Math.Abs(block.x - (player.x - this.worldSettings.circumferenceInBlocks));

    var relativeWidthIndex = block.y + 1;
    var playerOffset = Math.Max(Math.Max(
      Math.Min(realCircDistance, Math.Min(altCirclDistance, altaltCirclDistance)),
      Math.Abs(relativeWidthIndex - player.y)) - 1, 0);

    return (int)playerOffset;
  }

  public (Vector3[], int[], Vector2[]) SmoothShading(int tilesPerBlock, double size, double tiles, double origin)
  {
    int verticesPerRow = (int)(tilesPerBlock) + 1;
    int trianglesPerRow = (int)(tilesPerBlock) * 2;
    Vector3[] vertices = new Vector3[verticesPerRow * verticesPerRow];
    int[] triangles = new int[trianglesPerRow * trianglesPerRow * 6];
    int triIndex = 0;

    Vector2[] uvs = new Vector2[vertices.Length];
    for (int z = 0; z < verticesPerRow; z++)
    {
      for (int x = 0; x < verticesPerRow; x++)
      {
        int i = x + z * verticesPerRow;
        vertices[i] = GetCoordinate(
          (double)x,
          (double)z,
          tiles,
          size,
          origin, tilesPerBlock);

        if (x != verticesPerRow - 1 && z != verticesPerRow - 1)
        {
          triangles[triIndex] = i;
          triangles[triIndex + 1] = i + verticesPerRow + 1;
          triangles[triIndex + 2] = i + verticesPerRow;

          triangles[triIndex + 3] = i;
          triangles[triIndex + 4] = i + 1;
          triangles[triIndex + 5] = i + verticesPerRow + 1;
          triIndex += 6;
        }

        uvs[i] = new Vector2((float)z / verticesPerRow, (float)x / verticesPerRow);
      }
    }

    return (vertices, triangles, uvs);
  }

  public (Vector3[], int[], Vector2[]) HardShading(int tilesPerBlock, double size, double tiles, double origin)
  {
    int verticesPerRow = (int)(tilesPerBlock * resolution) + 1;
    int trianglesPerRow = (int)(tilesPerBlock * resolution) * 2;
    Vector3[] vertices = new Vector3[verticesPerRow * verticesPerRow * 6];
    int[] triangles = new int[(trianglesPerRow * trianglesPerRow) * 3];


    Vector2[] uvs = new Vector2[vertices.Length];


    for (int z = 0; z < verticesPerRow - 1; z++)
    {
      for (int x = 0; x < verticesPerRow - 1; x++)
      {
        int i = x + z * (verticesPerRow);
        double realX = (double)x / resolution;
        double realZ = (double)z / resolution;
        double offset = 1.0 / resolution;

        vertices[i * 6] = GetCoordinate(realX, realZ, tiles, size, origin, tilesPerBlock);
        vertices[i * 6 + 1] = GetCoordinate(realX + offset, realZ, tiles, size, origin, tilesPerBlock);
        vertices[i * 6 + 2] = GetCoordinate(realX, realZ + offset, tiles, size, origin, tilesPerBlock);

        vertices[i * 6 + 3] = GetCoordinate(realX + offset, realZ, tiles, size, origin, tilesPerBlock);
        vertices[i * 6 + 4] = GetCoordinate(realX + offset, realZ + offset, tiles, size, origin, tilesPerBlock);
        vertices[i * 6 + 5] = GetCoordinate(realX, realZ + offset, tiles, size, origin, tilesPerBlock);

        triangles[i * 6] = i * 6;
        triangles[i * 6 + 1] = i * 6 + 1;
        triangles[i * 6 + 2] = i * 6 + 2;

        triangles[i * 6 + 3] = i * 6 + 3;
        triangles[i * 6 + 4] = i * 6 + 4;
        triangles[i * 6 + 5] = i * 6 + 5;

        uvs[i] = new Vector2((float)z / verticesPerRow, (float)x / verticesPerRow);
      }
    }

    return (vertices, triangles, uvs);
  }

  public void ConstructMesh()
  {
    var adjustedTilesPerBlock = Math.Max(1, this.worldSettings.tilesPerBlock / Math.Pow(2, this.resolution));

    // Converted units
    double size = (double)this.worldSettings.circumferenceInBlocks;
    double vertexCount = (double)adjustedTilesPerBlock + 1;
    int tilesPerBlock = (int)adjustedTilesPerBlock;

    // Start of the block in the ring.
    double origin = ((double)blockCircumferenceIndex / size) * (Math.PI * 2);

    (Vector3[] vertices, int[] triangles, Vector2[] uvs) =
      this.worldSettings.smoothShading ?
        SmoothShading(tilesPerBlock, size, adjustedTilesPerBlock, origin) :
        HardShading(tilesPerBlock, size, adjustedTilesPerBlock, origin);

    if (worldSettings.fixMeshEdgeVertices)
    {
      vertices = FixVertices(vertices, adjustedTilesPerBlock);
    }

    mesh.Clear();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.uv = uvs;
    mesh.RecalculateNormals();

    if (worldSettings.fixMeshEdgeNormals)
    {
      Vector3[] normals = mesh.normals;
      mesh.normals = FixNormals(normals, tilesPerBlock, adjustedTilesPerBlock, size, origin);
    }
  }

  private Vector3[] FixVertices(Vector3[] vertices, double adjustedTilesPerBlock)
  {
    if (VectorDistance(playerIndex, new Vector2(blockIndex.x - 1, blockIndex.y)) == resolution + 1)
    {
      for (int z = 1; z < (int)adjustedTilesPerBlock; z += 2)
      {
        int vertexIndex = z * ((int)adjustedTilesPerBlock + 1);
        int leftIndex = (z - 1) * ((int)adjustedTilesPerBlock + 1);
        int rightIndex = (z + 1) * ((int)adjustedTilesPerBlock + 1);

        vertices[vertexIndex] = GetPointBetweenVectors(vertices[leftIndex], vertices[rightIndex]);
      }
    }
    if (VectorDistance(playerIndex, new Vector2(blockIndex.x + 1, blockIndex.y)) == resolution + 1)
    {
      for (int z = 1; z < (int)adjustedTilesPerBlock; z += 2)
      {
        int vertexIndex = (int)adjustedTilesPerBlock + z * ((int)adjustedTilesPerBlock + 1);
        int leftIndex = (int)adjustedTilesPerBlock + (z - 1) * ((int)adjustedTilesPerBlock + 1);
        int rightIndex = (int)adjustedTilesPerBlock + (z + 1) * ((int)adjustedTilesPerBlock + 1);

        vertices[vertexIndex] = GetPointBetweenVectors(vertices[leftIndex], vertices[rightIndex]);
      }
    }

    if (VectorDistance(playerIndex, new Vector2(blockIndex.x, blockIndex.y - 1)) == resolution + 1)
    {
      for (int x = 1; x < (int)adjustedTilesPerBlock; x += 2)
      {
        int vertexIndex = x;
        int leftIndex = (x - 1);
        int rightIndex = (x + 1);

        vertices[vertexIndex] = GetPointBetweenVectors(vertices[leftIndex], vertices[rightIndex]);
      }
    }

    if (VectorDistance(playerIndex, new Vector2(blockIndex.x, blockIndex.y + 1)) == resolution + 1)
    {
      for (int x = 1; x < (int)adjustedTilesPerBlock; x += 2)
      {
        int vertexIndex = x + ((int)adjustedTilesPerBlock + 1) * (int)adjustedTilesPerBlock;
        int leftIndex = (x - 1) + ((int)adjustedTilesPerBlock + 1) * (int)adjustedTilesPerBlock;
        int rightIndex = (x + 1) + ((int)adjustedTilesPerBlock + 1) * (int)adjustedTilesPerBlock;

        vertices[vertexIndex] = GetPointBetweenVectors(vertices[leftIndex], vertices[rightIndex]);
      }
    }

    return vertices;
  }

  private Vector3 GetPointBetweenVectors(Vector3 left, Vector3 right)
  {
    return new Vector3(
      (left.x + right.x) / 2.0f,
      (left.y + right.y) / 2.0f,
      (left.z + right.z) / 2.0f);
  }

  private Vector3[] FixNormals(Vector3[] normals, int tilesPerBlock, double adjustedTilesPerBlock, double size, double origin)
  {
    for (int z = 0; z < tilesPerBlock + 1; z++)
    {
      for (int x = 0; x < tilesPerBlock + 1; x++)
      {
        int i = x + z * (int)(tilesPerBlock + 1);
        if (x == 0 || x == tilesPerBlock || z == 0 || z == tilesPerBlock)
        {
          double resX = (double)x;
          double resZ = (double)z;
          double offset = 1.0;
          var up = GetCoordinate(resX, resZ + offset, adjustedTilesPerBlock, size, origin, tilesPerBlock);
          var right = GetCoordinate(resX + offset, resZ, adjustedTilesPerBlock, size, origin, tilesPerBlock);
          var down = GetCoordinate(resX, resZ - offset, adjustedTilesPerBlock, size, origin, tilesPerBlock);
          var left = GetCoordinate(resX - offset, resZ, adjustedTilesPerBlock, size, origin, tilesPerBlock);

          normals[i] = -(
            Vector3.Cross(up, right) +
            Vector3.Cross(right, down) +
            Vector3.Cross(down, left) +
            Vector3.Cross(left, up)
            ).normalized;
        }
      }
    }
    return normals;
  }

  private Vector3 GetCoordinate(double x, double z, double tiles, double size, double origin, int tilesPerBlock)
  {
    // Offset from origin for this tile.
    double offset = (x / tiles) * (1 / size) * (Math.PI * 2);
    double radians = origin + offset;

    Vector2 pointInRing = new Vector2(
      (float)Math.Cos(radians),
      (float)Math.Sin(radians)
    );

    var adjustor = this.worldSettings.tilesPerBlock / tilesPerBlock;

    Vector3 pointInSpace = new Vector3(
      (float)(pointInRing.x * this.r),
      (float)(pointInRing.y * this.r),
      (float)((z * adjustor) + this.blockWidthIndex * this.worldSettings.tilesPerBlock));



    double firstLayerHeight = 0;
    double height = 0;

    if (noiseSettingsEditor.noiseSettings.Length > 0)
    {
      firstLayerHeight = noiseSettingsEditor.noiseSettings[0].getValue(pointInSpace);
      if (noiseSettingsEditor.noiseSettings[0].enabled)
      {
        height = firstLayerHeight;
      }
    }

    for (int i = 1; i < noiseSettingsEditor.noiseSettings.Length; i++)
    {
      if (!noiseSettingsEditor.noiseSettings[i].enabled)
      {
        continue;
      }

      double mask = noiseSettingsEditor.noiseSettings[i].useFirstLayerAsMask ? firstLayerHeight : 1;
      height += noiseSettingsEditor.noiseSettings[i].getValue(pointInSpace) * mask;

    }

    // foreach(var layer in noiseSettingsEditor.noiseSettings) {      
    //   height += layer.getValue(pointInSpace);
    // }

    // // Noise functions
    // Vector3 noiseVector = new Vector3(
    //   (float)(pointInRing.x * this.r),
    //   (float)(pointInRing.y * this.r),
    //   (float)(z + this.blockWidthIndex * tilesPerBlock))  * (float)noiseSettings.noiseScale;

    // double height = noise.Evaluate(noiseVector) * noiseSettings.weight;

    // absolute position in space.
    float originX = (float)((this.r - height) * Math.Cos(radians));
    float originY = (float)((this.r - height) * Math.Sin(radians));

    return -new Vector3(originX, originY, (float)(z * adjustor) + this.blockWidthIndex * this.worldSettings.tilesPerBlock);
  }
}
