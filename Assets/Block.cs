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

  public Block(Mesh mesh, int blockCircumferenceIndex, int blockWidthIndex, double r, WorldSettings worldSettings, NoiseSettingsEditor noiseSettingsEditor, int playerDistance)
  {
    this.mesh = mesh;
    this.blockCircumferenceIndex = blockCircumferenceIndex;
    this.blockWidthIndex = blockWidthIndex;
    this.r = r;
    this.worldSettings = worldSettings;
    this.noiseSettingsEditor = noiseSettingsEditor;
    this.resolution = playerDistance;

    axisA = new Vector3(localUp.y, localUp.z, localUp.x);
    axisB = Vector3.Cross(localUp, axisA);
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

    mesh.Clear();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.uv = uvs;
    mesh.RecalculateNormals();

    Vector3[] normals = mesh.normals;

    if (worldSettings.fixMeshEdges)
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

      mesh.normals = normals;

    }
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
