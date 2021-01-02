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

  public void ConstructMesh()
  {
    // Converted units
    double size = (double)this.worldSettings.circumferenceInBlocks;
    double vertexCount = (double)this.worldSettings.tilesPerBlock + 1;
    double tiles = (double)this.worldSettings.tilesPerBlock;
    int tilesPerBlock = this.worldSettings.tilesPerBlock;

    /* Smooth shading
    Vector3[] vertices = new Vector3[(int)((tilesPerBlock + 1) * (tilesPerBlock + 1))];
    int[] triangles = new int[(int)(tilesPerBlock * tilesPerBlock * 6)];
    int triIndex = 0;

    // Start of the block in the ring.
    double origin = ((double)blockCircumferenceIndex / size) * (Math.PI * 2);

    for (int z = 0; z < tilesPerBlock + 1; z++)
    {
      for (int x = 0; x < tilesPerBlock + 1; x++)
      {
        int i = x + z * (tilesPerBlock + 1);
        vertices[i] = GetCoordinate(x, z, tiles, size, origin, tilesPerBlock);

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
    }*/

    // Hard shading
    Vector3[] vertices = new Vector3[(tilesPerBlock + 1) * (tilesPerBlock + 1) * 6];
    int[] triangles = new int[(tilesPerBlock * tilesPerBlock) * 6];

    double origin = ((double)blockCircumferenceIndex / size) * (Math.PI * 2);

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

    mesh.Clear();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.RecalculateNormals();

    Vector3[] normals = mesh.normals;

    for (int i = 0; i < mesh.vertices.Length; i+=3) {
      // Debug.DrawLine(mesh.vertices[i], mesh.vertices[i] + mesh.normals[i]);

      Vector3 middle = (mesh.vertices[i] + mesh.vertices[i + 1] + mesh.vertices[i + 2]) / 3;

      // Debug.DrawLine(middle, middle + mesh.normals[i]);
      // Debug.DrawLine(middle, middle + mesh.normals[i + 1]);
      // Debug.DrawLine(middle, middle + mesh.normals[i + 2]);
    }

    for (int z = 0; z < tilesPerBlock + 1; z++)
    {
      for (int x = 0; x < tilesPerBlock + 1; x++)
      {
        int i = x + z * (tilesPerBlock + 1);
        if (x == 0 || x == tilesPerBlock || z == 0 || z == tilesPerBlock)
        {
          var up = GetCoordinate(x, z + 1, tiles, size, origin, tilesPerBlock);
          var right = GetCoordinate(x + 1, z, tiles, size, origin, tilesPerBlock);
          var down = GetCoordinate(x, z - 1, tiles, size, origin, tilesPerBlock);
          var left = GetCoordinate(x - 1, z, tiles, size, origin, tilesPerBlock);

          var leftUp = GetCoordinate(x - 1, z + 1, tiles, size, origin, tilesPerBlock);
          var rightDown = GetCoordinate(x + 1, z - 1, tiles, size, origin, tilesPerBlock);

          normals[i] = -(
            Vector3.Cross(up, right) +
            Vector3.Cross(right, rightDown) +
            Vector3.Cross(rightDown, down) +
            Vector3.Cross(down, left) +
            Vector3.Cross(left, leftUp) +
            Vector3.Cross(leftUp, up)
            ).normalized;
        }
      }
    }

    //mesh.normals = normals;
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
