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
  NoiseSettings noiseSettings;
  WorldSettings worldSettings;
  Noise noise;

  public Block(Mesh mesh, int blockCircumferenceIndex, int blockWidthIndex, double r, WorldSettings worldSettings, NoiseSettings noiseSettings)
  {
    this.mesh = mesh;
    this.blockCircumferenceIndex = blockCircumferenceIndex;
    this.blockWidthIndex = blockWidthIndex;
    this.r = r;
    this.worldSettings = worldSettings;
    this.noiseSettings = noiseSettings;

    axisA = new Vector3(localUp.y, localUp.z, localUp.x);
    axisB = Vector3.Cross(localUp, axisA);
    noise = new Noise();
  }

  public void ConstructMesh()
  {
    // Converted units
    double size = (double)this.worldSettings.circumferenceInBlocks;
    double vertexCount = (double)this.worldSettings.tilesPerBlock + 1;
    double tiles = (double)this.worldSettings.tilesPerBlock;
    int tilesPerBlock = this.worldSettings.tilesPerBlock;

    Vector3[] vertices = new Vector3[(int)((tilesPerBlock + 1) * (tilesPerBlock + 1))];
    int[] triangles = new int[(int)(tilesPerBlock * tilesPerBlock * 6)];
    int triIndex = 0;

    // Start of the block in the ring.
    double origin = ((double)blockCircumferenceIndex / size) * (Math.PI * 2);

    for (int z = 0; z < tilesPerBlock + 1; z++)
    {
      for (int x = 0; x < tilesPerBlock + 1; x++)
      {
        // Offset from origin for this tile.
        double offset = (x / tiles) * (1 / size) * (Math.PI * 2);
        double radians = origin + offset;

        Vector2 pointInRing = new Vector2(
          (float)Math.Cos(radians),
          (float)Math.Sin(radians)
        );

        // Noise functions
        double height = noise.Evaluate(new Vector3(
          pointInRing.x,
          pointInRing.y,
          (float)(((z + this.blockWidthIndex * tilesPerBlock) / tiles) + noiseSettings.offsetZ)))
          * noiseSettings.weight;

        // absolute position in space.
        float originX = (float)((this.r + height) * Math.Cos(radians));
        float originY = (float)((this.r + height) * Math.Sin(radians));

        int i = x + z * (tilesPerBlock + 1);
        vertices[i] = -new Vector3(originX, originY, (float)z + this.blockWidthIndex * tilesPerBlock);

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

    mesh.Clear();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.RecalculateNormals();
  }
}
