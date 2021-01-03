using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

  public Block(Mesh mesh, int blockCircumferenceIndex, int blockWidthIndex, double r, WorldSettings worldSettings, NoiseSettingsEditor noiseSettingsEditor)
  {
    this.mesh = mesh;
    this.blockCircumferenceIndex = blockCircumferenceIndex;
    this.blockWidthIndex = blockWidthIndex;
    this.r = r;
    this.worldSettings = worldSettings;
    this.noiseSettingsEditor = noiseSettingsEditor;

    axisA = new Vector3(localUp.y, localUp.z, localUp.x);
    axisB = Vector3.Cross(localUp, axisA);
  }

  public (Vector3[], int[], Color[]) SmoothShading(int tilesPerBlock, double size, double tiles, double origin)
  {
    Vector3[] vertices = new Vector3[(int)((tilesPerBlock + 1) * (tilesPerBlock + 1))];
    Color[] colors = new Color[vertices.Length];
    int[] triangles = new int[(int)(tilesPerBlock * tilesPerBlock * 6)];
    int triIndex = 0;

    for (int z = 0; z < tilesPerBlock + 1; z++)
    {
      for (int x = 0; x < tilesPerBlock + 1; x++)
      {
        int i = x + z * (tilesPerBlock + 1);
        vertices[i] = GetCoordinate(x, z, tiles, size, origin, tilesPerBlock);
        colors[i] = new Color(0.3f, 0.3f, 0.3f, 1.0f);

        if (x != tilesPerBlock && z != tilesPerBlock)
        {
          triangles[triIndex] = i;
          triangles[triIndex + 1] = i + tilesPerBlock + 2;
          triangles[triIndex + 2] = i + tilesPerBlock + 1;

          triangles[triIndex + 3] = i;
          triangles[triIndex + 4] = i + 1;
          triangles[triIndex + 5] = i + tilesPerBlock + 2;
          triIndex += 6;
        }
      }
    }

    return (vertices, triangles, colors);
  }

  public (Vector3[], int[], Color[]) HardShading(int tilesPerBlock, double size, double tiles, double origin)
  {
    Vector3[] vertices = new Vector3[(tilesPerBlock + 1) * (tilesPerBlock + 1) * 6];
    Color[] colors = new Color[vertices.Length];
    int[] triangles = new int[(tilesPerBlock * tilesPerBlock) * 6];

    for (int z = 0; z < tilesPerBlock; z++)
    {
      for (int x = 0; x < tilesPerBlock; x++)
      {
        int i = x + z * (tilesPerBlock);

        vertices[i * 6] = GetCoordinate(x, z, tiles, size, origin, tilesPerBlock);
        vertices[i * 6 + 1] = GetCoordinate(x + 1, z, tiles, size, origin, tilesPerBlock);
        vertices[i * 6 + 2] = GetCoordinate(x, z + 1, tiles, size, origin, tilesPerBlock);

        vertices[i * 6 + 3] = GetCoordinate(x + 1, z, tiles, size, origin, tilesPerBlock);
        vertices[i * 6 + 4] = GetCoordinate(x + 1, z + 1, tiles, size, origin, tilesPerBlock);
        vertices[i * 6 + 5] = GetCoordinate(x, z + 1, tiles, size, origin, tilesPerBlock);

        triangles[i * 6] = i * 6;
        triangles[i * 6 + 1] = i * 6 + 1;
        triangles[i * 6 + 2] = i * 6 + 2;

        triangles[i * 6 + 3] = i * 6 + 3;
        triangles[i * 6 + 4] = i * 6 + 4;
        triangles[i * 6 + 5] = i * 6 + 5;

        for (int c = 0; c < 6; c++)
        {
          colors[i * 6 + c] = new Color(0.3f, 0.3f, 0.3f, 1.0f);
        }
      }
    }

    return (vertices, triangles, colors);
  }

  public void ConstructMesh()
  {
    // Converted units
    double size = (double)this.worldSettings.circumferenceInBlocks;
    double vertexCount = (double)this.worldSettings.tilesPerBlock + 1;
    double tiles = (double)this.worldSettings.tilesPerBlock;
    int tilesPerBlock = this.worldSettings.tilesPerBlock;

    // Start of the block in the ring.
    double origin = ((double)blockCircumferenceIndex / size) * (Math.PI * 2);

    (Vector3[] vertices, int[] triangles, Color[] colors) =
      this.worldSettings.smoothShading ?
        SmoothShading(tilesPerBlock, size, tiles, origin) :
        HardShading(tilesPerBlock, size, tiles, origin);


    mesh.Clear();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.colors = colors;
    mesh.RecalculateNormals();


    Vector3[] normals = mesh.normals;

    if (worldSettings.drawNormals)
    {
      if (worldSettings.smoothShading)
      {
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
          Debug.DrawLine(mesh.vertices[i], mesh.vertices[i] + mesh.normals[i]);
        }
      }
      else
      {
        for (int i = 0; i < mesh.vertices.Length; i += 3)
        {
          Vector3 middle = (mesh.vertices[i] + mesh.vertices[i + 1] + mesh.vertices[i + 2]) / 3;

          Debug.DrawLine(middle, middle + mesh.normals[i]);
        }
      }
    }

    if (worldSettings.fixMeshEdges)
    {
      for (int z = 0; z < tilesPerBlock + 1; z++)
      {
        for (int x = 0; x < tilesPerBlock + 1; x++)
        {
          int i = x + z * (tilesPerBlock + 1);
          //if (x == 0 || x == tilesPerBlock || z == 0 || z == tilesPerBlock)
          {
            var point = GetCoordinate(x, z, tiles, size, origin, tilesPerBlock);
            var up = GetCoordinate(x, z + 1, tiles, size, origin, tilesPerBlock);
            var right = GetCoordinate(x + 1, z, tiles, size, origin, tilesPerBlock);
            var down = GetCoordinate(x, z - 1, tiles, size, origin, tilesPerBlock);
            var left = GetCoordinate(x - 1, z, tiles, size, origin, tilesPerBlock);

            var leftUp = GetCoordinate(x - 1, z + 1, tiles, size, origin, tilesPerBlock);
            var rightDown = GetCoordinate(x + 1, z - 1, tiles, size, origin, tilesPerBlock);

            normals[i] = -(
              Vector3.Cross(up - point, right - point) +
              Vector3.Cross(right - point, rightDown - point) +
              Vector3.Cross(rightDown - point, down - point) +
              Vector3.Cross(down - point, left - point) +
              Vector3.Cross(left - point, leftUp - point) +
              Vector3.Cross(leftUp - point, up - point)
              ).normalized;
          }
        }
      }

      mesh.normals = normals;

    }

    mesh.RecalculateBounds();
    mesh.RecalculateTangents();
  }

  private Vector3 GetCoordinate(int x, int z, double tiles, double size, double origin, int tilesPerBlock)
  {
    // Offset from origin for this tile.
    double offset = (x / tiles) * (1 / size) * (Math.PI * 2);
    double radians = origin + offset;

    Vector2 pointInRing = new Vector2(
      (float)Math.Cos(radians),
      (float)Math.Sin(radians)
    );

    Vector3 pointInSpace = new Vector3(
      (float)(pointInRing.x * this.r),
      (float)(pointInRing.y * this.r),
      (float)(z + this.blockWidthIndex * tilesPerBlock));



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

    return -new Vector3(originX, originY, (float)z + this.blockWidthIndex * tilesPerBlock);
  }
}
