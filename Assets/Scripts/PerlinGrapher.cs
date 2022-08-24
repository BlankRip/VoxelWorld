using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlockyWorld.Perlin {
    [ExecuteInEditMode]
    public class PerlinGrapher : MonoBehaviour
    {
        public PerlinSettings settings = new PerlinSettings(5, 0.001f, 10.0f, -18.0f, 1.0f);
        public LineRenderer lr;

        private void Start() {
            Graph();
        }

        // //Factorial Browniem Motion
        // float fBM(float x, float z, int octives) {
        //     float total = 0.0f;
        //     float frequency = 1.0f;
        //     for (int i = 0; i < octives; i++) {
        //         total += Mathf.PerlinNoise((offset.x + x) * scale * frequency, (offset.z + z) * scale * frequency) * hightScale;
        //         frequency *= 2;
        //     }
        //     return offset.y + total;
        // }

        private void Graph() {
            lr = GetComponent<LineRenderer>();
            lr.positionCount = 100;
            int z = 11;
            Vector3[] positions = new Vector3[lr.positionCount];
            for (int x = 0; x < lr.positionCount; x++) {
                float y = MeshUtils.fBM(x, z, settings.octives, settings.scale, settings.heightScale, settings.heightOffset);
                positions[x] = new Vector3(x, y, z);
            }
            lr.SetPositions(positions);
        }

        private void OnValidate() {
            Graph();
        }
    }
}