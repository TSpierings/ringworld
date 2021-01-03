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

  public (Vector3[], int[]) SmoothShading(int tilesPerBlock, double size, double tiles, double origin)
  {
    int verticesPerRow = (int)(tilesPerBlock * worldSettings.resolution) + 1;
    int trianglesPerRow = (int)(tilesPerBlock * worldSettings.resolution) * 2;
    Vector3[] vertices = new Vector3[verticesPerRow * verticesPerRow];
    int[] triangles = new int[trianglesPerRow * trianglesPerRow * 6];
    int triIndex = 0;

    for (int z = 0; z < verticesPerRow; z++)
    {
      for (int x = 0; x < verticesPerRow; x++)
      {
        int i = x + z * verticesPerRow;
        vertices[i] = GetCoordinate(
          (double)x / worldSettings.resolution,
          (double)z / worldSettings.resolution,
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
      }
    }

    return (vertices, triangles);
  }

  public (Vector3[], int[]) HardShading(int tilesPerBlock, double size, double tiles, double origin)
  {
    Vector3[] vertices = new Vector3[(tilesPerBlock + 1) * (tilesPerBlock + 1) * 6];
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
      }
    }

    return (vertices, triangles);
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

    (Vector3[] vertices, int[] triangles) =
      this.worldSettings.smoothShading ?
        SmoothShading(tilesPerBlock, size, tiles, origin) :
        HardShading(tilesPerBlock, size, tiles, origin);


    mesh.Clear();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
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
      for (int z = 0; z < tilesPerBlock * worldSettings.resolution + 1; z++)
      {
        for (int x = 0; x < tilesPerBlock * worldSettings.resolution + 1; x++)
        {
          int i = x + z * (int)(tilesPerBlock * worldSettings.resolution + 1);
          if (x == 0 || x == tilesPerBlock * worldSettings.resolution || z == 0 || z == tilesPerBlock * worldSettings.resolution)
          {
            double resX = (double)x / worldSettings.resolution;
            double resZ = (double)z / worldSettings.resolution;
            double offset = 1.0 / worldSettings.resolution;
            var up = GetCoordinate(resX, resZ + offset, tiles, size, origin, tilesPerBlock);
            var right = GetCoordinate(resX + offset, resZ, tiles, size, origin, tilesPerBlock);
            var down = GetCoordinate(resX, resZ - offset, tiles, size, origin, tilesPerBlock);
            var left = GetCoordinate(resX - offset, resZ, tiles, size, origin, tilesPerBlock);

            normals[i] = -(
              Vector3.Cross(up, right) +
              Vector3.Cross(right, down) +
              Vector3.Cross(down, left) +
              Vector3.Cross(left, up)
              ).normalized;

            //var leftUp = GetCoordinate(x - 1.0 / worldSettings.resolution, z + 1.0 / worldSettings.resolution, tiles, size, origin, tilesPerBlock);
            //var rightDown = GetCoordinate(x + 1.0 / worldSettings.resolution, z - 1.0 / worldSettings.resolution, tiles, size, origin, tilesPerBlock);

            /*normals[i] = -(
              Vector3.Cross(up, right) +
              Vector3.Cross(right, rightDown) +
              Vector3.Cross(rightDown, down) +
              Vector3.Cross(down, left) +
              Vector3.Cross(left, leftUp) +
              Vector3.Cross(leftUp, up)
              ).normalized;*/
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
