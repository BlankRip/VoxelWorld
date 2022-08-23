using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PerlinGrapher : MonoBehaviour
{
    public float heightOffset = -18;
    public float hightScale = 2.0f;
    [Range(0.0f, 0.5f)] public float scale = 0.5f;
    public int octives = 1;
    [Range(0.0f, 1.0f)] public float probability = 1;
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
            float y = MeshUtils.fBM(x, z, octives, scale, hightScale, heightOffset);
            positions[x] = new Vector3(x, y, z);
        }
        lr.SetPositions(positions);
    }

    private void OnValidate() {
        Graph();
    }
}
