using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class World : MonoBehaviour
{
    public WorldSettings worldSettings;
    public NoiseSettings noiseSettings;

    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    Block[] blocks;
     
	private void OnValidate()
	{
        Initialize();
        GenerateMesh();
	}

	void Initialize()
    {
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[this.worldSettings.blocks];
        }
        blocks = new Block[this.worldSettings.blocks];

        double r = (this.worldSettings.blocks * this.worldSettings.tilesPerBlock) / (Math.PI * 2);

        for (int x = 0; x < this.worldSettings.blocks; x++) {
            if (meshFilters[x] == null) {
                GameObject meshObj = new GameObject("mesh");
                meshObj.transform.parent = transform;

                meshObj.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Standard"));
                meshFilters[x] = meshObj.AddComponent<MeshFilter>();
                meshFilters[x].sharedMesh = new Mesh();
            }

            blocks[x] = new Block(meshFilters[x].sharedMesh, x, r, this.worldSettings, this.noiseSettings);
        }
    }

    void GenerateMesh()
    {
        foreach (Block face in blocks)
        {
            face.ConstructMesh();
        }
    }
}
