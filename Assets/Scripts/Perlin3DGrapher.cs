using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockyWorld.Perlin {
    [ExecuteInEditMode]
    public class Perlin3DGrapher : MonoBehaviour
    {
        public PerlinSettings settings = new PerlinSettings(2, 0.085f, 4, 2.14f, 1.0f, 5.49f);

        private Vector3Int dimensions = new Vector3Int(10, 10, 10);

        private void CreateCubes() {
            for (int z = 0; z < dimensions.z; z++) {
                for (int y = 0; y < dimensions.y; y++) {
                    for (int x = 0; x < dimensions.x; x++) {
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.name = "Perlin_Cube";
                        cube.transform.parent = this.transform;
                        cube.transform.position = new Vector3(x, y, z);
                    }
                }
            }
        }

        private void Graph() {
            MeshRenderer[] cubes = GetComponentsInChildren<MeshRenderer>();
            if(cubes.Length == 0)
                CreateCubes();
            if(cubes.Length == 0)
                return;
            
            for (int z = 0; z < dimensions.z; z++) {
                for (int y = 0; y < dimensions.y; y++) {
                    for (int x = 0; x < dimensions.x; x++) {
                        float perlin3D = MeshUtils.fBM3D(x, y, z, settings.octives, settings.scale, settings.heightScale, settings.heightOffset);
                        if(perlin3D < settings.drawCutoff)
                            cubes[x + dimensions.x * (y + dimensions.z * z)].enabled = false;
                        else
                            cubes[x + dimensions.x * (y + dimensions.z * z)].enabled = true;
                    }
                }
            }
        }

        void OnValidate() {
            Graph();
        }
    }
}